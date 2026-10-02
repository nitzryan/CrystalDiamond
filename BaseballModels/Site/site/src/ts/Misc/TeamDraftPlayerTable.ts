type PlayerRankWar = {
    mlbId : number
    isHitter : boolean
    year : number
    month : number
    war : number
}

type TeamDraftPlayerRow = {
    draft : DB_DraftRank
    capital : number
    values : TdpValue[]
}

type TdpValue = {
    war : number
    stale : boolean
}

const TDP_MAX_YEARS = 6

////////// Fetching //////////
async function fetchPlayerRankWar(modelId : number, mlbIds : number[]) : Promise<PlayerRankWar[]>
{
    const response = await fetch('/player_model_list', {
        method : 'POST',
        headers : { 'Content-Type' : 'application/json' },
        body : JSON.stringify({ model : modelId, mlbIds : mlbIds })
    })
    if (!response.ok)
        throw new Error(`player_model_list request failed: ${response.status}`)

    return await response.json() as PlayerRankWar[]
}

async function fetchModelDraftPickValues() : Promise<DB_ModelDraftPickValues[]>
{
    const response = await fetch('/model_draft_pick_values')
    if (!response.ok)
        throw new Error(`model_draft_pick_values request failed: ${response.status}`)

    const json = await response.json() as JsonObject[]
    return json.map(f => new DB_ModelDraftPickValues(f))
}

////////// Helpers //////////
function tdpKey(mlbId : number, isHitter : boolean) : string
{
    return `${mlbId}_${isHitter ? 1 : 0}`
}

function tdpCapital(draft : DB_DraftRank, pickValues : Map<number, DB_ModelDraftPickValues>) : number
{
    if (draft.draftPick === null)
        throw Error("Draft Pick Null")

    const pv = pickValues.get(draft.draftPick)
    if (pv === undefined) 
        throw Error(`No Draft Pick Value for ${draft.draftPick}`)

    return draft.isHitter ? pv.WarHitter : pv.WarPitcher
}

function tdpYearColumns(draftYear : number, maxYears : number, maxDataYear : number) : number[]
{
    const end = Math.min(draftYear + maxYears, maxDataYear - 1)

    const years : number[] = []
    for (let yr = draftYear + 1; yr <= end; yr++)
        years.push(yr)
    return years
}

function tdpBuildRow(
    draft : DB_DraftRank,
    ranks : PlayerRankWar[],
    pickValues : Map<number, DB_ModelDraftPickValues>,
    years : number[],
    maxDataYear : number) : TeamDraftPlayerRow
{
    const initials = ranks.filter(r => r.year === 0 && r.month === 0)
    if (initials.length !== 1)
        throw new Error(`Expected exactly 1 initial PlayerRank entry for mlbId=${draft.mlbId} isHitter=${draft.isHitter}, found ${initials.length}`)
    const initial = initials[0]

    const valueAt = (year : number) : TdpValue => {
        let last = initial
        for (const r of ranks)
        {
            if (r.year > year) break
            last = r
        }
        return { war : last.war, stale : last !== initial && last.year !== year && year !== 2020}
    }

    return {
        draft : draft,
        capital : tdpCapital(draft, pickValues),
        values : [
            { war : initial.war, stale : false },
            ...years.map(yr => valueAt(yr)),
            valueAt(maxDataYear)
        ]
    }
}

////////// COLUMNS //////////
type DraftPlayerColumn = SortableColumn<TeamDraftPlayerRow>
function tdpDraftColumn(col : DraftColumn) : DraftPlayerColumn
{
    return {
        header : col.header,
        cls : col.cls,
        sortable : col.sortable,
        value : t => col.value(t.draft),
        render : (t, idx) => col.render(t.draft, idx)
    }
}

function tdpNameColumn() : DraftPlayerColumn
{
    return {
        header : 'Name',
        cls : 'c_name',
        sortable : false,
        value : t => t.draft.Name,
        render : (t, _) => `<a href='./player?id=${t.draft.mlbId}'>${t.draft.Name}</a>`,
        total : _ => 'Total'
    }
}

function tdpCapitalColumn() : DraftPlayerColumn
{
    return {
        header : 'Capital',
        cls : 'c_value',
        sortable : true,
        value : t => t.capital,
        render : (t, _) => t.capital.toFixed(1),
        total : rows => rows.reduce((sum, t) => sum + t.capital, 0).toFixed(1)
    }
}

function tdpValueColumn(header : string, idx : number) : DraftPlayerColumn
{
    return {
        header : header,
        cls : 'c_value',
        sortable : true,
        value : t => t.values[idx].war,
        render : (t, _) => {
            const val = t.values[idx]
            const prefix = val.stale ? '*' : ''
            return `${prefix}${val.war.toFixed(1)}<span class='c_share'>${Math.round(100 * val.war / t.capital)}%</span>`
        },
        total : rows => valueShareHtml(
            rows.reduce((sum, t) => sum + t.values[idx].war, 0),
            rows.reduce((sum, t) => sum + t.capital, 0))
    }
}

class TeamDraftPlayerTable
{
    private table : SortableTable<TeamDraftPlayerRow, TeamDraftView>
    private readonly model : number
    private readonly year : number
    private years : number[] = []
    private readonly teamSelect : HTMLSelectElement

    constructor(year : number, model : number, teamId : number | null, draftRows : Promise<DB_DraftRank[]>)
    {
        this.year = year
        this.model = model
        this.teamSelect = getElementByIdStrict('team_select') as HTMLSelectElement
        setupTeamSelector(teamId)

        this.table = new SortableTable<TeamDraftPlayerRow, TeamDraftView>({
            head : getElementByIdStrict('team_draft_players_head'),
            body : getElementByIdStrict('team_draft_players_body'),
            getColumns : () => this.getColumns(),
            rowClass : 'rankings_item',
            view : {
                groupId : 'team_draft_players_view_select',
                initial : 'default',
                parse : _ => 'default',
            },
            split : {
                groupId : 'split_select',
                initial : parseTableSplit(getQueryParamBackupStr('split', 'all')),
            },
            split_filter : (t, split) => {
                if (split === 'hitters') return !!t.draft.isHitter
                if (split === 'pitchers') return !t.draft.isHitter
                return true
            },
            view_filter : (t, _view) => this.teamId === 0 || t.draft.draftTeamid === this.teamId,
            view_split_default_column : (_split, _view) => 1
        })

        this.teamSelect.addEventListener('change', () => this.table.render())

        this.table.setDefaultColumn()
        this.loadAll(draftRows)

        let header = getElementByIdStrict('team_draft_header')
        header.innerText = `Team Player Results for ${year} Draft`
    }

    get teamId() : number { return parseInt(this.teamSelect.value) }

    setTeam(teamId : number)
    {
        this.teamSelect.value = teamId.toString()
        this.table.render()
    }

    private getColumns() : SortableColumn<TeamDraftPlayerRow>[]
    {
        return [
            tdpNameColumn(),
            tdpDraftColumn(dPickColumn()),
            tdpCapitalColumn(),
            tdpValueColumn('Initial', 0),
            ...this.years.map((yr, i) => tdpValueColumn(`${yr}`, i + 1)),
            tdpValueColumn('Current', this.years.length + 1)
        ]
    }

    private async loadAll(draftRows : Promise<DB_DraftRank[]>)
    {
        const drafted = (await draftRows).filter(f => f.draftTeamid !== null)
        const mlbIds = Array.from(new Set(drafted.map(f => f.mlbId)))
        
        // Fetch Data
        const [ranks, pickValues] = await Promise.all([
            fetchPlayerRankWar(this.model, mlbIds),
            fetchModelDraftPickValues()
        ])
        const pickMap = new Map<number, DB_ModelDraftPickValues>()
        for (const pv of pickValues)
            pickMap.set(pv.Pick, pv)

        const history = new Map<string, PlayerRankWar[]>()
        for (const r of ranks)
        {
            const key = tdpKey(r.mlbId, r.isHitter)
            if (!history.has(key))
                history.set(key, [])
            history.get(key)!.push(r)
        }

        let maxDataYear = 0
        for (const r of ranks)
            maxDataYear = Math.max(maxDataYear, r.year)
        this.years = tdpYearColumns(this.year, TDP_MAX_YEARS, maxDataYear)

        this.table.rows = drafted.map(f =>
            tdpBuildRow(f, history.get(tdpKey(f.mlbId, f.isHitter)) ?? [], pickMap, this.years, maxDataYear))
        this.table.render()
    }
}