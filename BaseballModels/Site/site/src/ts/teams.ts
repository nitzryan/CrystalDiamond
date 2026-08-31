const rankings_table_head = getElementByIdStrict('rankings_table_head') as HTMLTableSectionElement
const rankings_table_body = getElementByIdStrict('rankings_table_body') as HTMLTableSectionElement
const rankings_header = getElementByIdStrict('rankings_header')
const rankings_table = getElementByIdStrict('rankings_table') as HTMLTableElement
const rankings_load = getElementByIdStrict('rankings_load') as HTMLButtonElement

let month : number
let year : number
let modelId : number

type TeamView = 'overview' | 'breakdown' | 'draft'

type Column = SortableColumn<DB_TeamRank>

let table : SortableTable<DB_TeamRank, TeamView> | null = null
function getTable() : SortableTable<DB_TeamRank, TeamView>
{
    if (table == null)
        throw new Error("Table was null")
    return table
}

async function main()
{
    await assetLoader.ready;

    const dates = await assetLoader.dates();
    endYear = dates.endYear;
    endMonth = dates.endMonth;
    month = getQueryParamBackup('month', endMonth)
    year = getQueryParamBackup('year', endYear)
    modelId = getQueryParamBackup("model", 1)

    table = new SortableTable<DB_TeamRank, TeamView>({
        head : rankings_table_head,
        body : rankings_table_body,
        getColumns : getColumns,
        rowClass : 'rankings_item',
        view : {
            groupId : 'view_select',
            initial : parseView(getQueryParamBackupStr('view', 'overview')),
            parse : parseView,
            onChange : v => {
                // Overview doesn't have split
                getElementByIdStrict('split_select').classList.toggle('vis_hidden', v === 'overview')
            }
        },
        split : {
            groupId : 'split_select',
            initial : parseTableSplit(getQueryParamBackupStr('split', 'all'))
        },
        split_filter : (row, split) => true,
        view_filter : (t, view) => true,
        view_split_default_column : (split, view) => null
    })

    setupSelector({
        month : month,
        year : year,
        modelId : modelId,
        endYear : endYear,
        endMonth : endMonth,
        startYear : dates.startYear,
        startTeam : null,
        level : null
    })

    createOverviewPage()

    getElementByIdStrict('nav_teams').classList.add('selected')
}

function parseView(value : string) : TeamView
{
    if (value === 'breakdown' || value === 'draft')
        return value
    return 'overview'
}

///////// Get column data from DB_TeamRank based on current split
function warTotal(t : DB_TeamRank) : number
{
    const tbl = getTable()
    if (tbl.split === 'hitters') return t.warHitter
    if (tbl.split === 'pitchers') return t.warPitcher
    return t.war
}
function warDraft(t : DB_TeamRank) : number
{
    const tbl = getTable()
    if (tbl.split === 'hitters') return t.warDraftHitter
    if (tbl.split === 'pitchers') return t.warDraftPitcher
    return t.warDraftHitter + t.warDraftPitcher
}
function warTrade(t : DB_TeamRank) : number
{
    const tbl = getTable()
    if (tbl.split === 'hitters') return t.warTradeHitter
    if (tbl.split === 'pitchers') return t.warTradePitcher
    return t.warTradeHitter + t.warTradePitcher
}
function warSign(t : DB_TeamRank) : number
{
    const tbl = getTable()
    
    if (tbl.split === 'hitters') return t.warSignHitter
    if (tbl.split === 'pitchers') return t.warSignPitcher
    return t.warSignHitter + t.warSignPitcher
}
function draftCapital(t : DB_TeamRank) : number
{
    const tbl = getTable()
    if (tbl.split === 'hitters') return t.draftCapitalHitter
    if (tbl.split === 'pitchers') return t.draftCapitalPitcher
    return t.draftCapitalHitter + t.draftCapitalPitcher
}

////////// Column definitions
function rankColumn() : Column
{
    return {
        header : '',
        cls : 'c_rank',
        sortable : false,
        value : t => t.rank,
        render : (_, idx) => (idx + 1).toString()
    }
}
function nameColumn() : Column
{
    return {
        header : 'Team',
        cls : 'c_name',
        sortable : true,
        value : t => getParentName(t.teamId),
        render : (t, _) => `<a href='./rankings?team=${t.teamId}&year=${year}&month=${month}&model=${modelId}'>${getParentName(t.teamId)}</a>`
    }
}
function warColumn() : Column
{
    return {
        header : 'WAR',
        cls : 'c_value',
        sortable : true,
        value : t => warTotal(t),
        render : (t, _) => warTotal(t).toFixed(1)
    }
}
function warDraftColumn() : Column
{
    return {
        header : 'WAR',
        cls : 'c_value',
        sortable : true,
        value : t => warDraft(t),
        render : (t, _) => warDraft(t).toFixed(1)
    }
}
function countColumn(header : string, get : (t : DB_TeamRank) => number) : Column
{
    return {
        header : header,
        cls : 'c_value',
        sortable : true,
        value : t => get(t),
        render : (t, _) => get(t).toString()
    }
}
function warValueColumn(header : string, get : (t : DB_TeamRank) => number) : Column
{
    return {
        header : header,
        cls : 'c_value',
        sortable : true,
        value : t => get(t),
        render : (t, _) => get(t).toFixed(1)
    }
}


/** WAR value plus its share of the current split's total, e.g. "12.3 (34%)". */
function warShareColumn(header : string, get : (t : DB_TeamRank) => number) : Column
{
    return {
        header : header,
        cls : 'c_value',
        sortable : true,
        value : t => get(t),
        render : (t, _) => {
            const v = get(t)
            const total = warTotal(t)
            const share = total > 0 ? Math.round(100 * v / total) : 0
            return `${v.toFixed(1)}<span class='c_share'>(${share}%)</span>`
        }
    }
}

function warCapitalColumn() : Column
{
    return {
        header : "Capital",
        cls : "c_value",
        sortable : true,
        value : t => draftCapital(t),
        render : (t, _) => draftCapital(t).toFixed(1)
    }
}
// Get the % of draft capital
function warDraftPercOfCapital(header : string, get : (t : DB_TeamRank) => number) : Column
{ 
    return {
        header : header,
        cls : 'c_value',
        sortable : true,
        value : t => get(t),
        render : (t, _) => {
            return `${Math.round(100 * get(t))}%`
        }
    }
}

function getColumns() : Column[]
{
    const tbl = getTable()
    const columns : Column[] = [rankColumn(), nameColumn()]
    switch (tbl.view)
    {
        case 'overview':
            columns.push(countColumn('Highest Ranked', t => t.highestRank))
            columns.push(warColumn())
            columns.push(countColumn('Top 10', t => t.top10))
            columns.push(countColumn('Top 50', t => t.top50))
            columns.push(countColumn('Top 100', t => t.top100))
            columns.push(countColumn('Top 200', t => t.top200))
            columns.push(countColumn('Top 500', t => t.top500))
            break
        case 'breakdown':
            columns.push(warColumn())
            if (tbl.split === 'all')
            {
                columns.push(warValueColumn('Hitters', t => t.warHitter))
                columns.push(warValueColumn('Pitchers', t => t.warPitcher))
            }
            columns.push(warShareColumn('Draft', warDraft))
            columns.push(warShareColumn('Trade', warTrade))
            columns.push(warShareColumn('Signed', warSign))
            break
        case 'draft':
            columns.push(warDraftColumn())
            if (tbl.split === 'all')
            {
                columns.push(warValueColumn('Hitters', t => t.warDraftHitter))
                columns.push(warValueColumn('Pitchers', t => t.warDraftPitcher))
            }
            columns.push(warCapitalColumn())
            columns.push(warDraftPercOfCapital('%', t => warDraft(t) / draftCapital(t)))

            break
    }
    return columns
}

async function createOverviewPage()
{
    const tbl = getTable()
    const json = await (await fetch(`./teamRanks?year=${year}&month=${month}&model=${modelId}`)).json() as JsonArray
    tbl.rows = json.map(f => new DB_TeamRank(f as JsonObject))
    tbl.render()
    rankings_header.innerText = `Team Rankings for ${MONTH_CODES[month]} ${year}`
    rankings_load.classList.add('hidden')
    team_select?.classList.add('hidden')
    rankings_button.addEventListener('click', (event) => {
        const mnth = month_select.value
        const yr = year_select.value
        const model = model_select.value
        window.location.href = `./teams?year=${yr}&month=${mnth}&model=${model}&view=${tbl.view}&split=${tbl.split}`
    })
    document.title = `${MONTH_CODES[month]} ${year} Team Rankings`
}

main()