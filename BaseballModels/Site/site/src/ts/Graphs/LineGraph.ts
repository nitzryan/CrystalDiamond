const POINT_DEFAULT_SIZE : number = 3
const POINT_HIGHLIGHT_SIZE : number = 9

const PRIMARY_LINE_COLOR : string = 'rgb(54, 162, 235)'
const PRIMARY_FILL_COLOR : string = 'rgba(54, 162, 235, 0.5)'
const CONTINUATION_LINE_COLOR : string = 'rgb(255, 159, 64)'
const CONTINUATION_FILL_COLOR : string = 'rgba(255, 159, 64, 0.5)'
const CONTINUATION_DASH : number[] = [6, 4]

type GraphSeries = {
    points : Point[],
    title : string,
}

type GraphDataset = {
    points : Point[],
    title : string,
    yscale : any,
    continuation? : GraphSeries,
}

type GraphObj = {
    datasets : JsonObject[],
    labels : string[],
    yscale : JsonObject,
    showLegend : boolean,
}

function buildGraphObj(f : GraphDataset) : GraphObj
{
    const primary : JsonObject = {
        label: f.title,
        data: f.points.map(g => g.y),
        pointRadius: new Array(f.points.length).fill(POINT_DEFAULT_SIZE),
        borderColor: PRIMARY_LINE_COLOR,
        backgroundColor: PRIMARY_FILL_COLOR,
    }

    if (f.continuation === undefined || f.continuation.points.length === 0)
        return { datasets: [primary], labels: f.points.map(g => g.label), yscale: f.yscale, showLegend: false }

    const cont = f.continuation
    if (f.points.length === 0)
        throw new Error(`LineGraph dataset '${f.title}' has a continuation '${cont.title}' but no primary points`)
    const lastPrimary = f.points[f.points.length - 1]
    if (cont.points[0].label !== lastPrimary.label)
        throw new Error(`LineGraph dataset '${f.title}' ends at '${lastPrimary.label}' but continuation '${cont.title}' starts at '${cont.points[0].label}'`)

    // Shared x axis: primary labels, then continuation labels after the overlapping point
    const labels = f.points.map(g => g.label).concat(cont.points.slice(1).map(g => g.label))
    const offset = f.points.length - 1

    // Pad each series with nulls so both line up on the shared axis; Chart.js skips null points
    let primaryData : (number | null)[] = f.points.map(g => g.y)
    let continuationData : (number | null)[] = new Array(offset).fill(null)
    while (primaryData.length < labels.length)
        primaryData.push(null)
    cont.points.forEach(g => continuationData.push(g.y))

    primary.data = primaryData
    primary.pointRadius = new Array(labels.length).fill(POINT_DEFAULT_SIZE)

    const continuation : JsonObject = {
        label: cont.title,
        data: continuationData,
        pointRadius: new Array(labels.length).fill(POINT_DEFAULT_SIZE),
        borderColor: CONTINUATION_LINE_COLOR,
        backgroundColor: CONTINUATION_FILL_COLOR,
        borderDash: CONTINUATION_DASH,
    }

    return { datasets: [primary, continuation], labels: labels, yscale: f.yscale, showLegend: true }
}

function getRankScale(title : string) : any
{
    return {
        type: 'logarithmic',
        reverse: true,
        min: 0.1,
        max: 20000,
        grid: {
            color: css.background_low
        },
        // @ts-ignore
        ticks: {
            // @ts-ignore
            callback: function(value, index, ticks) {
                if (value === 1 || value === 10 || value === 100 || value === 1000 || value === 10000)
                    return value.toLocaleString()
                return null
            }
        },
        title: {
            display: true,
            text: title
        },
    }
}

function getWarScale(title : string, points : Point[], minMax : number) : any
{
    const values = points.map(g => g.y)
    return {
        // grid, position, ticks: copy from the else branch of the LineGraph constructor
        min: Math.min(0, Math.floor(values.reduce((a,b) => Math.min(a,b), 0))),
        max: Math.max(minMax, values.reduce((a,b) => Math.max(a,b), 0) + 2),
        title: {
            display: true,
            text: title
        },
        grid: {
            color: css.background_low
        },
        position: 'left',
        // @ts-ignore
        ticks: {
            // @ts-ignore
            callback: function(value, index, ticks) {
                if (value % 2 === 0)
                    return value.toLocaleString()
                return null
            }
        }
    }
}

type LineGraphCallback = (datasetIdx : number, pointIdx : number) => void

class LineGraph
{
    private datasets : GraphDataset[]
    private graphObjs : GraphObj[]
    private datasetIdx : number
    private callback : LineGraphCallback
    private continuationCallback : LineGraphCallback | null
    private selectedPos : number
    private readonly chart : any

    constructor(element : HTMLCanvasElement, datasets : GraphDataset[], callback : LineGraphCallback, continuationCallback : LineGraphCallback | null = null)
    {
        this.selectedPos = 0

        this.datasets = datasets
        this.datasetIdx = 0

        this.callback = callback

        this.continuationCallback = continuationCallback

        // Setup Datasets
        this.graphObjs = datasets.map(buildGraphObj)

        // @ts-ignore
        this.chart = new Chart(element, {
            type: 'line',
            data: {
                labels: this.graphObjs[0].labels,
                datasets : this.graphObjs[0].datasets,  
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: {
                    duration: 0,
                },
                plugins: {
                    legend: {
                        // Only shown when a continuation series needs distinguishing
                        display: this.graphObjs[0].showLegend,
                        onClick: () => {},
                    },
                    tooltip: {
                        callbacks: {
                            // @ts-ignore
                            labelColor: (item) => getTooltipColorAsDisplayed(item),
                        },
                    },
                },
                // @ts-ignore
                onClick: (e, elements) => {
                    const items = this.chart.getElementsAtEventForMode(e, 'nearest', {intersect: false, axis: 'x'}, true);
                    const pos = this.getClickedPosition(e, items)
                    if (pos !== null)
                        this.select(pos)
                },
                scales: {
                    y: this.graphObjs[0].yscale,
                    x: {
                        grid: {
                            color: css.background_low
                        }
                    }
                },
                
            }
        })

        this.selectLatest()
    }

    private primaryCount() : number
    {
        return this.datasets[this.datasetIdx].points.length
    }

    private selectableCount() : number
    {
        const continuation = this.datasets[this.datasetIdx].continuation
        if (this.continuationCallback === null || continuation === undefined)
            return this.primaryCount()
        return this.primaryCount() + continuation.points.length
    }

    // Where the continuation series' first point sits on the shared x axis
    private continuationOffset() : number
    {
        return this.primaryCount() - 1
    }

    // Both series share an x position where they join, so several points can come back for one click.
    // Take the one closest to the cursor; on an exact tie take the primary point, as it is drawn on top
    // @ts-ignore
    private getClickedPosition(e, items) : number | null
    {
        let best : number | null = null
        let bestDistance = Number.POSITIVE_INFINITY
        for (const item of items)
        {
            let pos : number
            if (item.datasetIndex === 0)
                pos = item.index
            else if (item.datasetIndex === 1 && this.continuationCallback !== null)
                pos = this.primaryCount() + item.index - this.continuationOffset()
            else
                continue

            const distance = Math.hypot(item.element.x - e.x, item.element.y - e.y)
            if (distance < bestDistance)
            {
                best = pos
                bestDistance = distance
            }
        }
        return best
    }

    // Enlarge the selected point without firing callbacks
    private highlightSelected() : void
    {
        const obj = this.graphObjs[this.datasetIdx]
        const primaryCount = this.primaryCount()

        let primaryRadius : number[] = new Array(obj.labels.length).fill(POINT_DEFAULT_SIZE)
        if (this.selectedPos < primaryCount)
            primaryRadius[this.selectedPos] = POINT_HIGHLIGHT_SIZE
        this.chart.data.datasets[0].pointRadius = primaryRadius

        if (obj.datasets.length > 1)
        {
            let continuationRadius : number[] = new Array(obj.labels.length).fill(POINT_DEFAULT_SIZE)
            if (this.selectedPos >= primaryCount)
                continuationRadius[this.continuationOffset() + this.selectedPos - primaryCount] = POINT_HIGHLIGHT_SIZE
            this.chart.data.datasets[1].pointRadius = continuationRadius
        }
        this.chart.update()
    }

    private selectLatest() : void
    {
        this.selectedPos = Math.max(0, this.selectableCount() - 1)
        if (this.selectableCount() > 0)
            this.highlightSelected()
    }

    private select(pos : number) : void
    {
        this.selectedPos = pos
        this.highlightSelected()
        this.fireCallback()
    }

    increment_index(x_inc : number) : void
    {
        const pos = this.selectedPos + x_inc
        if (pos >= 0 && pos < this.selectableCount())
            this.select(pos)
    }

    fireCallback() : void
    {
        const primaryCount = this.primaryCount()
        if (this.selectedPos < primaryCount)
            this.callback(this.datasetIdx, this.selectedPos)
        else if (this.continuationCallback !== null)
            this.continuationCallback(this.datasetIdx, this.selectedPos - primaryCount)
    }

    setDataset(idx : number) : void
    {
        this.datasetIdx = idx
        this.chart.data.datasets = this.graphObjs[this.datasetIdx].datasets
        this.chart.data.labels = this.graphObjs[this.datasetIdx].labels
        this.chart.options.scales.y = this.graphObjs[this.datasetIdx].yscale
        this.chart.options.plugins.legend.display = this.graphObjs[this.datasetIdx].showLegend
        this.chart.update()

        this.selectLatest()
    }
}