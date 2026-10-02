async function fetchTeamDraftOverview(year : number, modelId : number) : Promise<DB_TeamDraftOverview[]>
{
    const response = await fetch(`/team_draft_overview?year=${year}&model=${modelId}`)
    if (!response.ok)
        throw new Error(`team_draft_overview request failed: ${response.status}`)

    const json = await response.json() as JsonObject[]
    return json.map(f => new DB_TeamDraftOverview(f))
}

type TeamDraftOverviewRow = {
    teamId : number
    hitterCapital : number
    pitcherCapital : number
    byYear : Map<number, DB_TeamDraftOverview>
}

type TeamDraftView = 'default'

function tdoCapital(t : TeamDraftOverviewRow, split : TableSplit) : number
{
    if (split === 'hitters') return t.hitterCapital
    if (split === 'pitchers') return t.pitcherCapital
    return t.hitterCapital + t.pitcherCapital
}

function tdoValue(t : TeamDraftOverviewRow, evalYear : number, split : TableSplit) : number | null
{
    const d = t.byYear.get(evalYear)
    if (d === undefined) return null
    if (split === 'hitters') return d.HitterValue
    if (split === 'pitchers') return d.PitcherValue
    return d.HitterValue + d.PitcherValue
}

function tdoNameColumn() : SortableColumn<TeamDraftOverviewRow>
{
    return {
        header : 'Team',
        cls : 'c_name',
        sortable : true,
        value : t => getParentName(t.teamId),
        render : (t, _) => `<a href='#' data-team='${t.teamId}'>${getParentName(t.teamId)}</a>`,
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

function tdoValueColumn(evalYear : number, split : TableSplit) : SortableColumn<TeamDraftOverviewRow>
{
    return {
        header : `${evalYear}`,
        cls : 'c_value',
        sortable : true,
        value : t => tdoValue(t, evalYear, split),
        render : (t, _) => {
            const value = tdoValue(t, evalYear, split)
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
                const v = tdoValue(t, evalYear, split)
                if (v === null) 
                    continue
                value += v
                capital += tdoCapital(t, split)
            }
            return valueShareHtml(value, capital)
        }
    }
}

class TeamDraftOverviewTable
{
    private table : SortableTable<TeamDraftOverviewRow, TeamDraftView>
    private readonly year : number
    private readonly model : number
    private evalYears : number[] = []

    constructor(year : number, model : number, onTeamClick : (teamId : number) => void)
    {
        this.year = year
        this.model = model

        this.table = new SortableTable<TeamDraftOverviewRow, TeamDraftView>({
            head : getElementByIdStrict('team_draft_head'),
            body : getElementByIdStrict('team_draft_body'),
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
            view_split_default_column : (_split, _view) => 1
        })

        getElementByIdStrict('team_draft_body').addEventListener('click', (event) => {
            const link = (event.target as HTMLElement).closest('a[data-team]')
            if (link === null) return

            event.preventDefault()
            onTeamClick(parseInt(link.getAttribute('data-team') ?? '0'))
        })

        this.table.setDefaultColumn()
        this.loadAll()

        let header = getElementByIdStrict('team_rank_header')
        header.innerText = `Team Results for ${year} Draft`
    }

    private getColumns() : SortableColumn<TeamDraftOverviewRow>[]
    {
        const split = this.table.split
        return [
            tdoNameColumn(),
            tdoCapitalColumn(split),
            ...this.evalYears.map(yr => tdoValueColumn(yr, split))
        ]
    }

    private async loadAll()
    {
        const data = await fetchTeamDraftOverview(this.year, this.model)

        this.evalYears = Array.from(new Set(data.map(f => f.EvaluationYear))).sort((a, b) => a - b)

        const teams = new Map<number, TeamDraftOverviewRow>()
        for (const d of data)
        {
            let row = teams.get(d.TeamId)
            if (row === undefined)
            {
                row = {
                    teamId : d.TeamId,
                    hitterCapital : d.HitterCapital,
                    pitcherCapital : d.PitcherCapital,
                    byYear : new Map()
                }
                teams.set(d.TeamId, row)
            }
            row.byYear.set(d.EvaluationYear, d)
        }

        this.table.rows = Array.from(teams.values())
        this.table.render()
    }
}