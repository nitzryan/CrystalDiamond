const CURRENT_YEAR_OFFSET = 100

async function fetchTeamDraftOverview(startYear : number, endYear : number, modelId : number) : Promise<DB_TeamDraftOverview[]>
{
    const response = await fetch(`/team_draft_overview?startYear=${startYear}&endYear=${endYear}&model=${modelId}`)
    if (!response.ok)
        throw new Error(`team_draft_overview request failed: ${response.status}`)

    const json = await response.json() as JsonObject[]
    return json.map(f => new DB_TeamDraftOverview(f))
}

type TeamDraftOverviewRow = {
    teamId : number
    draftYear : number
    hitterCapital : number
    pitcherCapital : number
    byOffset : Map<number, DB_TeamDraftOverview>
}

type TeamDraftView = 'default'

function tdoCapital(t : TeamDraftOverviewRow, split : TableSplit) : number
{
    if (split === 'hitters') return t.hitterCapital
    if (split === 'pitchers') return t.pitcherCapital
    return t.hitterCapital + t.pitcherCapital
}

function tdoValue(t : TeamDraftOverviewRow, offset : number, split : TableSplit) : number | null
{
    const d = t.byOffset.get(offset)
    if (d === undefined) return null
    if (split === 'hitters') return d.HitterValue
    if (split === 'pitchers') return d.PitcherValue
    return d.HitterValue + d.PitcherValue
}

function tdoNameColumn(model : number, split : TableSplit) : SortableColumn<TeamDraftOverviewRow>
{
    return {
        header : 'Team',
        cls : 'c_name',
        sortable : true,
        value : t => getParentName(t.teamId),
        render : (t, _) => `<a href='/draft?year=${t.draftYear}&model=${model}&split=${split}&section=team_draft&team=${t.teamId}'>${getParentName(t.teamId)}</a>`,
        total : rows => "All",
    }
}

function tdoCapitalColumn(split : TableSplit) : SortableColumn<TeamDraftOverviewRow>
{
    return {
        header : 'Capital',
        cls : 'c_value',
        sortable : true,
        value : t => tdoCapital(t, split),
        render : (t, _) => tdoCapital(t, split).toFixed(1),
        total : rows => rows.reduce((sum, t) => sum + tdoCapital(t, split), 0).toFixed(1)
    }
}

function tdoValueColumn(offset : number, header : string, split : TableSplit) : SortableColumn<TeamDraftOverviewRow>
{
    return {
        header : header,
        cls : 'c_value',
        sortable : true,
        value : t => tdoValue(t, offset, split),
        render : (t, _) => {
            const value = tdoValue(t, offset, split)
            if (value === null) return ''

            const capital = tdoCapital(t, split)
            if (capital === 0) return value.toFixed(1)

            return `${value.toFixed(1)}<span class='c_share'>${Math.round(100 * value / capital)}%</span>`
        },
        total : rows => {
            let value = 0
            let capital = 0
            for (const t of rows)
            {
                const v = tdoValue(t, offset, split)
                if (v === null) 
                    continue
                value += v
                capital += tdoCapital(t, split)
            }
            return valueShareHtml(value, capital)
        }
    }
}

function tdoYearColumn() : SortableColumn<TeamDraftOverviewRow>
{
    return {
        header : 'Year',
        cls : 'c_year',
        sortable : true,
        value : t => t.draftYear,
        render : (t, _) => t.draftYear.toString()
    }
}

class TeamDraftOverviewTable
{
    private table : SortableTable<TeamDraftOverviewRow, TeamDraftView>
    private readonly startYear : number
    private readonly endYear : number
    private readonly isRange : boolean
    private readonly model : number
    private offsets : number[] = []

    constructor(startYear : number, endYear : number, model : number)
    {
        this.startYear = startYear
        this.endYear = endYear
        this.isRange = startYear !== endYear
        this.model = model

        this.table = new SortableTable<TeamDraftOverviewRow, TeamDraftView>({
            head : getElementByIdStrict('team_draft_head'),
            body : getElementByIdStrict('team_draft_body'),
            pageChangeElement : getElementByIdStrict('team_draft_pageselector'),
            getColumns : () => this.getColumns(),
            rowClass : 'rankings_item',
            view : {
                groupId : 'team_draft_view_select',
                initial : 'default',
                parse : _ => 'default',
            },
            split : {
                groupId : 'split_select',
                initial : parseTableSplit(getQueryParamBackupStr('split', 'all')),
            },
            split_filter : (_t, _split) => true,
            view_filter : (_t, _view) => true,
            view_split_default_column : (_split, _view) => this.isRange ? 2 : 1,
            countPerPage : 30,
            forceLastVisible : true
        })

        this.table.setDefaultColumn()
        this.loadAll()

        let header = getElementByIdStrict('team_rank_header')
        header.innerText = this.isRange
            ? `Team Results for ${startYear}-${endYear} Drafts`
            : `Team Results for ${startYear} Draft`
    }

    private getColumns() : SortableColumn<TeamDraftOverviewRow>[]
    {
        const split = this.table.split
        return [
            tdoNameColumn(this.model, split),
            ...(this.isRange ? [tdoYearColumn()] : []),
            tdoCapitalColumn(split),
            ...this.offsets.map(n => tdoValueColumn(n, this.isRange ? `Yr ${n}` : `${this.startYear + n}`, split)),
            ...(this.isRange ? [tdoValueColumn(CURRENT_YEAR_OFFSET, "Current", split)] : [])
        ]
    }

    private async loadAll()
    {
        const data = await fetchTeamDraftOverview(this.startYear, this.endYear, this.model)

        this.offsets = Array
            .from(new Set(data.map(f => f.EvaluationYear - f.DraftYear)))
            .filter(a => a < CURRENT_YEAR_OFFSET)
            .sort((a, b) => a - b)

        const teams = new Map<string, TeamDraftOverviewRow>()
        for (const d of data)
        {
            const key = `${d.TeamId}_${d.DraftYear}`
            let row = teams.get(key)
            if (row === undefined)
            {
                row = {
                    teamId : d.TeamId,
                    draftYear : d.DraftYear,
                    hitterCapital : d.HitterCapital,
                    pitcherCapital : d.PitcherCapital,
                    byOffset : new Map()
                }
                teams.set(key, row)
            }

            let offset = d.EvaluationYear - d.DraftYear
            offset = Math.min(offset, CURRENT_YEAR_OFFSET)
            row.byOffset.set(offset, d)
        }

        this.table.rows = Array.from(teams.values())
        this.table.render()
    }
}