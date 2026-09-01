type LevelFilter = 'mlb' | 'milb' | 'all'

function parseLevelFilter(value : string) : LevelFilter
{
    if (value === 'mlb' || value === 'milb')
        return value
    return 'all'
}

function levelFilterMatch(levelId : number, filter : LevelFilter) : boolean
{
    if (filter === 'mlb')
        return levelId === 1
    if (filter === 'milb')
        return levelId !== 1
    return true
}

function ipStringToOuts(ip : string) : number
{
    const parts = ip.split('.')
    const full = parseInt(parts[0])
    const partial = parts.length > 1 ? parseInt(parts[1]) : 0
    return full * 3 + partial
}

function getDefaultLevelFilterHitter(stats : HitterStatLine[]) : LevelFilter
{
    const mlbYearLines = stats.filter(f => f.levelId === 1 && getStatMonth(f) === null)
    const mlbYears = new Set(mlbYearLines.map(f => f.year))
    const mlbPa = mlbYearLines.reduce((total, f) => total + f.PA, 0)

    if (mlbYears.size >= 4)
        return 'mlb'
    if (mlbYears.size >= 2 && mlbPa >= 600)
        return 'mlb'
    return 'all'
}

function getDefaultLevelFilterPitcher(stats : PitcherStatLine[]) : LevelFilter
{
    const mlbYearLines = stats.filter(f => f.levelId === 1 && getStatMonth(f) === null)
    const mlbYears = new Set(mlbYearLines.map(f => f.year))
    const mlbOuts = mlbYearLines.reduce((total, f) => total + ipStringToOuts(f.IP), 0)

    if (mlbYears.size >= 4)
        return 'mlb'
    if (mlbYears.size >= 2 && mlbOuts >= 150 * 3)
        return 'mlb'
    return 'all'
}

// -------------------------- Base Class ------------------------------
abstract class PlayerStatsTable<TStat extends HitterStatLine | PitcherStatLine, TPred>
{
    private container : HTMLElement
    private body : HTMLElement
    private toggleButtons : HTMLButtonElement[]

    private rows : TStat[]
    private predictions : TPred[] = []
    private filter : LevelFilter
    private expandedYears : Set<number> = new Set()

    constructor(containerId : string, bodyId : string, toggleGroupId : string,
                rows : TStat[], initialFilter : LevelFilter)
    {
        this.container = getElementByIdStrict(containerId)
        this.body = getElementByIdStrict(bodyId)
        this.rows = rows
        this.filter = initialFilter

        this.toggleButtons = Array.from(
            document.querySelectorAll<HTMLButtonElement>(`#${toggleGroupId} button`))
        this.toggleButtons.forEach(btn => {
            btn.addEventListener('click', () => {
                this.setFilter(parseLevelFilter(btn.getAttribute('data-level') ?? ''))
            })
        })

        this.syncButtons()
        this.render()
        this.container.classList.remove('hidden')
    }

    private setFilter(filter : LevelFilter) : void
    {
        this.filter = filter
        this.syncButtons()
        this.render()
    }

    setPredictions(predictions : TPred[]) : void
    {
        this.predictions = predictions
        this.render()
    }

    private syncButtons() : void
    {
        this.toggleButtons.forEach(btn => btn.classList.toggle('selected',
            btn.getAttribute('data-level') === this.filter))
    }

    // ----------- Abstract Functions -----------------------------
    protected abstract statCells(row : TStat) : string
    protected abstract predictionCells(pred : TPred) : string

    // -------------- Rendering -----------------------------------
    private visibleRows() : TStat[]
    {
        return this.rows.filter(f => {
            if (!levelFilterMatch(f.levelId, this.filter))
                return false
            const month = getStatMonth(f)
            return month === null ?
                !this.expandedYears.has(f.year) :
                this.expandedYears.has(f.year)
        })
    }

    private render() : void
    {
        const visible = this.visibleRows()
        const showMonthCol = visible.some(f => getStatMonth(f) !== null)
        const monthClass = showMonthCol ? "" : "hidden-col"

        this.body.innerHTML = ''

        let prevYear : number | null = null
        visible.forEach(f => {
            const month = getStatMonth(f)
            const tr = this.makeStatRow(f, month, monthClass)

            if (f.year !== prevYear)
            {
                prevYear = f.year
                tr.classList.add('row_first')
                tr.getElementsByTagName('td')[0]
                    .appendChild(this.makeToggleButton(f.year, month !== null))
            }

            this.body.appendChild(tr)
        })

        this.predictions.forEach((p, i) => {
            const tr = this.makePredictionRow(p, monthClass)
            if (i === 0)
                tr.classList.add('row_first')
            this.body.appendChild(tr)
        })

        this.container.querySelectorAll('thead .table_month').forEach(f =>
            f.classList.toggle('hidden-col', !showMonthCol))
    }

    // ---------------------- Building Rows ------------------------
    private makeToggleButton(year : number, isMonthRow : boolean) : HTMLButtonElement
    {
        const button = document.createElement('button')
        button.classList.add('table_button')

        if (isMonthRow)
        {
            button.innerText = '-'
            button.classList.add('table_retract')
            button.addEventListener('click', () => {
                this.expandedYears.delete(year)
                this.render()
            })
        } else {
            button.innerText = '+'
            button.classList.add('table_expand')
            button.addEventListener('click', () => {
                this.expandedYears.add(year)
                this.render()
            })
        }

        return button
    }

    private makeStatRow(f : TStat, month : number | null, monthClass : string) : HTMLTableRowElement
    {
        let teamAbbr : string = ""
        // VSL/DSL has some teams that split orgs
        try {
            teamAbbr = getTeamAbbr(f.teamId, f.year)
        } catch(e)
        {
            if (f.leagueId != 134 && f.leagueId != 130) // If not VSL/DSL, it is an error
                throw e
        }

        const tr = document.createElement('tr')
        tr.innerHTML = `
            <td></td>
            <td>${f.year}</td>
            <td class='table_month ${monthClass}'>${month !== null ? MONTH_CODES[month] : ""}</td>
            <td>${level_map[f.levelId]}</td>
            <td>${teamAbbr}</td>
            <td>${getLeagueAbbr(f.leagueId)}</td>
            ${this.statCells(f)}
        `
        return tr
    }

    private makePredictionRow(p : TPred, monthClass : string) : HTMLTableRowElement
    {
        const tr = document.createElement('tr')
        tr.innerHTML = `
            <td></td>
            <td>PRED</td>
            <td class='table_month ${monthClass}'></td>
            ${this.predictionCells(p)}
        `
        tr.classList.add('pred')
        return tr
    }
}

// ------------------ Hitter/Pitcher Implementations -----------------
class HitterStatsTable extends PlayerStatsTable<HitterStatLine, DB_Prediction_HitterStats>
{
    constructor(rows : HitterStatLine[])
    {
        super('hitter_stats', 'h_stats_body', 'hitter_level_toggle', rows, getDefaultLevelFilterHitter(rows))
    }

    protected statCells(f : HitterStatLine) : string
    {
        return `
            <td class="align_right">${f.PA}</td>
            <td class="align_right">${f.AVG.toFixed(3)}</td>
            <td class="align_right">${f.OBP.toFixed(3)}</td>
            <td class="align_right">${f.SLG.toFixed(3)}</td>
            <td class="align_right">${f.ISO.toFixed(3)}</td>
            <td class="align_right">${f.WRC}</td>
            <td class="align_right">${f.HR}</td>
            <td class="align_right">${f.BBPerc.toFixed(1)}</td>
            <td class="align_right">${f.KPerc.toFixed(1)}</td>
            <td class="align_right">${f.SB}</td>
            <td class="align_right">${f.CS}</td>
        `
    }

    protected predictionCells(f : DB_Prediction_HitterStats) : string
    {
        return `
            <td>${level_map2[f.LevelId]}</td>
            <td></td>
            <td></td>
            <td class="align_right">${f.Pa}</td>
            <td class="align_right">${f.AVG.toFixed(3)}</td>
            <td class="align_right">${f.OBP.toFixed(3)}</td>
            <td class="align_right">${f.SLG.toFixed(3)}</td>
            <td class="align_right">${f.ISO.toFixed(3)}</td>
            <td class="align_right">${f.wRC.toFixed(0)}</td>
            <td class="align_right">${f.HitHR.toFixed(1)}</td>
            <td class="align_right">${(f.BB / f.Pa * 100).toFixed(1)}</td>
            <td class="align_right">${(f.K / f.Pa * 100).toFixed(1)}</td>
            <td class="align_right">${f.SB.toFixed(1)}</td>
            <td class="align_right">${f.CS.toFixed(1)}</td>
        `
    }
}

class PitcherStatsTable extends PlayerStatsTable<PitcherStatLine, DB_Prediction_PitcherStats>
{
    constructor(rows : PitcherStatLine[])
    {
        super('pitcher_stats', 'p_stats_body', 'pitcher_level_toggle', rows, getDefaultLevelFilterPitcher(rows))
    }

    protected statCells(f : PitcherStatLine) : string
    {
        return `
            <td class="align_right">${f.IP}</td>
            <td class="align_right">${f.ERA.toFixed(2)}</td>
            <td class="align_right">${f.FIP.toFixed(2)}</td>
            <td class="align_right">${f.ERAMinus.toFixed(0)}</td>
            <td class="align_right">${f.FIPMinus.toFixed(0)}</td>
            <td class="align_right">${f.HR9.toFixed(1)}</td>
            <td class="align_right">${f.BBPerc.toFixed(1)}</td>
            <td class="align_right">${f.KPerc.toFixed(1)}</td>
            <td class="align_right">${f.GOPerc.toFixed(1)}</td>
        `
    }

    protected predictionCells(f : DB_Prediction_PitcherStats) : string
    {
        return `
            <td>${level_map2[f.levelId]}</td>
            <td></td>
            <td></td>
            <td class="align_right">${formatOutsToIP(f.Outs_RP + f.Outs_SP)}</td>
            <td class="align_right">${f.ERA.toFixed(2)}</td>
            <td class="align_right">${f.FIP.toFixed(2)}</td>
            <td class="align_right">${f.ERAMinus.toFixed(0)}</td>
            <td class="align_right">${f.FIPMinus.toFixed(0)}</td>
            <td class="align_right">${f.HR9.toFixed(1)}</td>
            <td class="align_right">${f.BBPerc.toFixed(1)}</td>
            <td class="align_right">${f.KPerc.toFixed(1)}</td>
            <td class="align_right"></td>
        `
    }
}