type TdpValue = {
    war : number
    postEligible : boolean
}

// Index into TeamDraftPlayerRow.values: 0 = Initial, 1..6 = draftYear + n, 7 = Current
const TDP_INITIAL = 0
const TDP_YEAR_COUNT = 6
const TDP_CURRENT = 7


type TeamDraftPlayerRow = {
    draft : DB_DraftRank
    capital : number
    values : (TdpValue | null)[]
}

////////// Fetching //////////
async function fetchTeamDraftPlayers(draftYear : number, modelId : number) : Promise<DB_TeamDraftPlayer[]>
{
    const response = await fetch(`/team_draft_players?draftYear=${draftYear}&model=${modelId}`)
    if (!response.ok)
        throw new Error(`team_draft_players request failed: ${response.status}`)

    const json = await response.json() as JsonObject[]
    return json.map(f => new DB_TeamDraftPlayer(f))
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

function tdpCapital(player : DB_TeamDraftPlayer, pickValues : Map<number, DB_ModelDraftPickValues>) : number
{
    const pv = pickValues.get(player.draftPick)
    if (pv === undefined)
        throw Error(`No Draft Pick Value for ${player.draftPick}`)

    return player.isHitter ? pv.WarHitter : pv.WarPitcher
}

function tdpYearColumns(draftYear : number, maxYears : number, maxDataYear : number) : number[]
{
    const end = Math.min(draftYear + maxYears, maxDataYear - 1)

    const years : number[] = []
    for (let yr = draftYear + 1; yr <= end; yr++)
        years.push(yr)
    return years
}

function tdpBuildRow(player : DB_TeamDraftPlayer, draft : DB_DraftRank, pickValues : Map<number, DB_ModelDraftPickValues>) : TeamDraftPlayerRow
{
    // A year is flagged from the post-eligible year onward
    const yearValue = (war : number | null, year : number) : TdpValue | null =>
        war === null
            ? null
            : { war : war, postEligible : player.postEligibleYear !== null && year >= player.postEligibleYear }


    const yearWars = [
        player.warYear1, player.warYear2, player.warYear3,
        player.warYear4, player.warYear5, player.warYear6
    ]


    return {
        draft : draft,
        capital : tdpCapital(player, pickValues),
        values : [
            { war : player.initialWar, postEligible : false },
            ...yearWars.map((war, i) => yearValue(war, player.draftYear + i + 1)),
            { war : player.currentWar, postEligible : player.postEligibleYear !== null }
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
        value : t => t.values[idx]?.war ?? Number.NEGATIVE_INFINITY,
        render : (t, _) => {
            const val = t.values[idx]
            if (val === null)
                return ''


            const prefix = val.postEligible ? '*' : ''
            return `${prefix}${val.war.toFixed(1)}<span class='c_share'>${Math.round(100 * val.war / t.capital)}%</span>`
        },
        total : rows => tdpValueTotal(rows, idx)
    }
}


function tdpValueTotal(rows : TeamDraftPlayerRow[], idx : number) : string
{
    const present = rows.filter(r => r.values[idx] !== null)
    return valueShareHtml(
        present.reduce((sum, r) => sum + r.values[idx]!.war, 0),
        present.reduce((sum, r) => sum + r.capital, 0))
}

class TeamDraftPlayerTable
{
    private table : SortableTable<TeamDraftPlayerRow, TeamDraftView>
    private readonly model : number
    private readonly year : number
    private yearSlots : number[] = []
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
            tdpValueColumn('Initial', TDP_INITIAL),
            ...this.yearSlots.map(n => tdpValueColumn(`${this.year + n}`, n)),
            tdpValueColumn('Current', TDP_CURRENT)
        ]
    }

    private async loadAll(draftRows : Promise<DB_DraftRank[]>)
    {
        const [drafts, players, pickValues] = await Promise.all([
            draftRows,
            fetchTeamDraftPlayers(this.year, this.model),
            fetchModelDraftPickValues()
        ])


        const pickMap = new Map<number, DB_ModelDraftPickValues>()
        for (const pv of pickValues)
            pickMap.set(pv.Pick, pv)


        // DraftRank still supplies the name and draft-pick presentation
        const draftMap = new Map<string, DB_DraftRank>()
        for (const d of drafts)
            draftMap.set(tdpKey(d.mlbId, d.isHitter), d)


        const rows = players.map(p => {
            const draft = draftMap.get(tdpKey(p.mlbId, p.isHitter))
            if (draft === undefined)
                throw new Error(`No DraftRank entry for mlbId=${p.mlbId} isHitter=${p.isHitter}`)
            return tdpBuildRow(p, draft, pickMap)
        })


        // Only show year columns that have data for at least one player in this draft
        this.yearSlots = Array.from({ length: TDP_YEAR_COUNT }, (_, i) => i + 1)
            .filter(n => rows.some(r => r.values[n] !== null))

        this.table.rows = rows
        this.table.render()
    }    
}