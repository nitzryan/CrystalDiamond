class TablePlayer extends DB_PlayerRank
{
    firstName : string
    lastName : string
    birthYear : number
    birthMonth : number

    constructor(obj : JsonObject)
    {
        super(obj)
        this.firstName = getJsonString(obj, 'firstName')
        this.lastName = getJsonString(obj, 'lastName')
        this.birthYear = getJsonNumber(obj, 'birthYear')
        this.birthMonth = getJsonNumber(obj, 'birthMonth')
    }

    get name() : string
    {
        return `${this.firstName} ${this.lastName}`
    }
}
type PlayerView = 'all' | 'drafted' | 'signed'
type PlayerColumn = SortableColumn<TablePlayer>
type PlayerLoaderTableArgs = {
    year : number
    month : number
    model : number
    teamId : number | null
}

// ---------------------- COLUMNS --------------------------
function plRankColumn() : PlayerColumn
{
    return {
        header : '',
        cls : 'c_rank',
        sortable : false,
        value : p => null,
        render : (_, idx) => (idx + 1).toString()
    }
}
function plNameColumn() : PlayerColumn
{
    return {
        header : 'Name',
        cls : 'c_name',
        sortable : false,
        value : p => p.name,
        render : (p, _) =>
            `<a href='./player?id=${p.mlbId}'>${p.name}</a>${renderQualityIcon(p.timestepQuality, p.trainingBias)}`
    }
}
function plTeamColumn(year : number, month : number) : PlayerColumn
{
    return {
        header : 'Team',
        cls : 'c_team',
        sortable : false,
        value : p => p.teamId === 0 ? null : getParentAbbr(p.teamId),
        render : (p, _) => {
            const abbr = p.teamId === 0 ? '' : getParentAbbr(p.teamId)
            return `<a href='./rankings?team=${p.teamId}&year=${year}&month=${month}'>${abbr}</a>`
        }
    }
}
function plWarColumn() : PlayerColumn
{
    return {
        header : 'WAR',
        cls : 'c_value',
        sortable : true,
        value : p => p.war,
        render : (p, _) => formatModelString(p.war)
    }
}
function plLevelColumn() : PlayerColumn
{
    return {
        header : 'Level',
        cls : 'c_lvl',
        sortable : false,
        // Lower level id = higher level; negate so descending-first shows MLB-closest first
        value : p => p.highestLevel === null ? null : -p.highestLevel,
        render : (p, _) => p.highestLevel === null ? '' : level_map[p.highestLevel] as string
    }
}
function plOverallColumn() : PlayerColumn
{
    return {
        header : 'Overall',
        cls : 'c_ovr',
        sortable : false,
        value : p => p.rankWar,
        render : (p, _) => p.rankWar.toString()
    }
}
function plPositionColumn() : PlayerColumn
{
    return {
        header : 'Position',
        cls : 'c_pos',
        sortable : false,
        value : p => p.position,
        render : (p, _) => p.position
    }
}
function plAgeColumn(year : number, month : number) : PlayerColumn
{
    return {
        header : 'Age',
        cls : 'c_age',
        sortable : true,
        value : p => playerAge(p.birthYear, p.birthMonth, year, month),
        render : (p, _) => playerAge(p.birthYear, p.birthMonth, year, month).toString()
    }
}


function parseInitialSplit(split : string) : PlayerView
{
    if (split === 'signed' || split === 'drafted')
        return split


    return 'all'
}


class PlayerLoaderTable
{
    private table : SortableTable<TablePlayer, PlayerView>
    private index : number = 0
    private exhausted : boolean = false
    private readonly args : PlayerLoaderTableArgs
    private readonly pageSize : number


    constructor(args : PlayerLoaderTableArgs, pageSize : number)
    {
        this.args = args
        this.pageSize = pageSize


        this.table = new SortableTable<TablePlayer, PlayerView>({
            head : rankings_table_head,
            body : rankings_table_body,
            getColumns : () => this.getColumns(),
            rowClass : 'rankings_item',
            view : {
                groupId : 'view_select',
                initial : parseInitialSplit(getQueryParamBackupStr('view', 'all')),
                parse : parseInitialSplit,
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
            view_filter : (t, view) => {
                if (view === 'drafted') return t.draftPick !== null
                if (view === 'signed') return t.draftPick === null
                return true
            },
            view_split_default_column : (split, view) => {
                return null
            }
        })


        // From setupRankings: header text. teamString carries its own trailing space.
        const teamString = args.teamId !== null ? getParentName(args.teamId) + ' ' : ''
        rankings_header.innerText =
            `Prospect Rankings for ${teamString}${MONTH_CODES[args.month]} ${args.year}`


        rankings_load.addEventListener('click', () => this.loadMore())
        this.loadMore()
    }


    private getColumns() : PlayerColumn[]
    {
        const a = this.args
        const columns : PlayerColumn[] = [
            plRankColumn(),
            plNameColumn(),
            plTeamColumn(a.year, a.month),
            plWarColumn(),
            plLevelColumn()
        ]


        if (a.teamId !== null)
            columns.push(plOverallColumn())
        columns.push(plPositionColumn())
        columns.push(plAgeColumn(a.year, a.month))
        return columns
    }


    private async loadMore()
    {
        if (this.exhausted)
            return
        const a = this.args
        const endRank = this.index + this.pageSize
        let url = `/rankingsRequest?year=${a.year}&month=${a.month}&startRank=${this.index + 1}&endRank=${endRank}&model=${a.model}`
        if (a.teamId !== null)
            url += `&teamId=${a.teamId}`


        const players = await (await fetch(url)).json() as JsonArray
        const loaded = players.map(f => new TablePlayer(f as JsonObject))

        this.exhausted = players.length !== this.pageSize
        this.index += players.length
        rankings_load.classList.toggle('hidden', this.exhausted)


        this.table.rows.push(...loaded)
        this.table.render()
    }
}