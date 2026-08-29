type DraftView = 'eligible' | 'results'
type DraftColumn = SortableColumn<DB_DraftRank>

function parseDraftView(value : string) : DraftView
{
    if (value === 'results')
        return value
    return 'eligible'
}

function dNameColumn() : DraftColumn
{
    return {
        header : 'Name',
        cls : 'c_name',
        sortable : false,
        value : p => p.Name,
        render : (p, _) => {
            const player_href = p.draftPick === null ? '' : `href='./player?id=${p.mlbId}'`
            return `<a ${player_href}>${p.Name}</a>${renderQualityIcon(p.timestepQuality, p.trainingBias)}`
        }
    }
}
function dPreWarColumn() : DraftColumn
{
    return {
        header : 'College Model WAR',
        cls : 'c_pre',
        sortable : true,
        value : p => p.warPre,
        render : (p, _) => p.warPre === null ? '---' : p.warPre.toFixed(1)
    }
}
function dPostWarColumn() : DraftColumn
{
    return {
        header : 'Pro Model WAR',
        cls : 'c_post',
        sortable : true,
        value : p => p.warPost,
        render : (p, _) => p.warPost === null ? '---' : p.warPost.toFixed(1)
    }
}
function dPickColumn() : DraftColumn
{
    return {
        header : 'Draft Pick',
        cls : 'c_pick',
        // Picks are better when lower; negate so descending-first shows pick 1 first
        sortable : true,
        value : p => p.draftPick === null ? null : -p.draftPick,
        render : (p, _) => p.draftPick === null ? '---' : p.draftPick.toString()
    }
}
function plRankColumn() : DraftColumn
{
    return {
        header : '',
        cls : 'c_rank',
        sortable : false,
        value : p => null,
        render : (_, idx) => (idx + 1).toString()
    }
}
function plPositionColumn() : DraftColumn
{
    return {
        header : 'Position',
        cls : 'c_pos',
        sortable : false,
        value : p => p.Position,
        render : (p, _) => p.Position
    }
}
function plAgeColumn(year : number, month : number) : DraftColumn
{
    return {
        header : 'Age',
        cls : 'c_age',
        sortable : true,
        value : p => playerAge(p.BirthYear, p.BirthMonth, year, month),
        render : (p, _) => playerAge(p.BirthYear, p.BirthMonth, year, month).toString()
    }
}

class DraftLoaderTable
{
    private table : SortableTable<DB_DraftRank, DraftView>
    private readonly year : number
    private readonly month : number
    private readonly model : number

    constructor(year : number, month : number, model : number)
    {
        this.year = year
        this.month = month
        this.model = model

        this.table = new SortableTable<DB_DraftRank, DraftView>({
            head : rankings_table_head,
            body : rankings_table_body,
            getColumns : () => this.getColumns(),
            rowClass : 'rankings_item',
            view : {
                groupId : 'view_select',
                initial : parseDraftView(getQueryParamBackupStr('view', 'eligible')),
                parse : parseDraftView,
            },
            split : {
                groupId : 'split_select',
                initial : parseTableSplit(getQueryParamBackupStr('split', 'all')),
            },
            split_filter : (p, split) => {
                if (split === 'hitters') return p.isHitter
                if (split === 'pitchers') return !p.isHitter
                return true
            },
            view_filter : (p, view) => {
                if (view === 'eligible') return p.isEligible
                return p.draftPick !== null       // 'results'
            }
        })

        rankings_header.innerText = `Draft Prospect Rankings for ${year}`
        rankings_load.classList.add('hidden')    // single fetch, no pagination
        this.loadAll()
    }

    get view() : DraftView { return this.table.view }
    get split() : TableSplit { return this.table.split }

    private getColumns() : DraftColumn[]
    {
        return [
            plRankColumn(),
            dNameColumn(),
            dPreWarColumn(),
            dPostWarColumn(),
            dPickColumn(),
            plPositionColumn(),
            plAgeColumn(this.year, this.month)
        ]
    }

    private async loadAll()
    {
        const players = await (await fetch(
            `/draft_rank?year=${this.year}&model=${this.model}`)).json() as JsonArray

        this.table.rows = players.map(f => new DB_DraftRank(f as JsonObject))
        this.table.render()
    }
}