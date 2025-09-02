const POINT_DEFAULT_SIZE : number = 3
const POINT_HIGHLIGHT_SIZE : number = 9

class LineGraph
{
    private points : Point[]
    private currentIdx : number
    private colorscale :  string[]
    private callback : (index : number) => void
    private readonly chart : any

    private warDataset : JsonObject
    private rankingDataset : JsonObject | null

    constructor(element : HTMLCanvasElement, points : Point[], ranks : Point[], callback : (index : number) => void, colorscale : string[] | null = null)
    {
        this.points = points
        this.callback = callback
        this.currentIdx = 0
        if (colorscale !== null)
            this.colorscale = colorscale
        else
            this.colorscale = [
                        '#d43d51',
                        '#df797d',
                        '#e2acab',
                        '#dddddd',
                        '#a1c0b6',
                        '#63a490',
                        '#00876c']

        const max_war : number = points.map(f => f.y).reduce((a, b) => Math.max(a,b), 0)

        // Setup Datasets
        this.warDataset = {
            label: 'WAR',
            data: points.map(f => f.y),
            pointRadius : new Array(points.length).fill(POINT_DEFAULT_SIZE),
            yAxisID: 'y',
        }

        if (ranks.length > 0)
        {
            this.rankingDataset = {
                label: 'Ranking',
                data: ranks.map(f => f.y),
                pointRadius : new Array(points.length).fill(POINT_DEFAULT_SIZE),
                yAxisID: 'y1',
                hidden: true,
            }
        } else {
            this.rankingDataset = null
        }
        let datasets = [this.warDataset]
        if (this.rankingDataset !== null)
            datasets.push(this.rankingDataset)

        // @ts-ignore
        this.chart = new Chart(element, {
            type: 'line',
            data: {
                labels: points.map(f => f.label),
                datasets : datasets,   
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: {
                    duration: 0,
                },
                // @ts-ignore
                onClick: (e, elements) => {
                    const points = this.chart.getElementsAtEventForMode(e, 'nearest', {intesect: false, axis: 'x'}, true);
                    if (points.length > 0)
                    {
                        const firstPoint = points[0]
                        const index = firstPoint.index;
                        this.highlight_index(index)
                        this.callback(index)
                    }
                },
                scales: {
                    y: {
                        min: 0,
                        max: Math.max(16, max_war + 1),
                        grid: {
                            color: css.background_low
                        },
                        title: {
                            display: true,
                            text: 'Expected WAR Through Control'
                        },
                        position: 'left',
                        ticks: {
                            // @ts-ignore
                            callback: function(value, index, ticks) {
                                if (value % 2 === 0)
                                    return value.toLocaleString()
                                return null
                            }
                        }
                    },
                    y1: {
                        type: 'logarithmic',
                        display: this.rankingDataset !== null,
                        position : 'right',
                        reverse: true,
                        min: 1,
                        max: 20000,
                        grid: {
                            drawOnChartArea: false
                        },
                        title: {
                            display: true,
                            text: 'Prospect Ranking'
                        },
                        ticks: {
                            // @ts-ignore
                            callback: function(value, index, ticks) {
                                if (value === 1 || value === 10 || value === 100 || value === 1000 || value === 10000)
                                    return value.toLocaleString()
                                return null
                            }
                        }
                    },
                    x: {
                        grid: {
                            color: css.background_low
                        }
                    }
                },
                
            }
        })

        if (points.length > 0)
            this.highlight_index(points.length - 1);
    }

    highlight_index(index : number) : void
    {
        this.currentIdx = index
        let pointRadius : number[] = new Array(this.points.length).fill(POINT_DEFAULT_SIZE)
        pointRadius[index] = POINT_HIGHLIGHT_SIZE
        this.chart.data.datasets[0].pointRadius = pointRadius
        this.chart.update()
    }

    increment_index(x_inc : number) : void
    {
        if ((x_inc === -1 && this.currentIdx > 0) || (x_inc === 1 && this.currentIdx < this.points.length - 1))
        {
            this.currentIdx += x_inc
            this.highlight_index(this.currentIdx)
            this.callback(this.currentIdx)
        }
    }
}