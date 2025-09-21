const POINT_DEFAULT_SIZE : number = 3
const POINT_HIGHLIGHT_SIZE : number = 9

type GraphDataset = {
    points : Point[],
    title : string,
    isLog : boolean,
    isHitter : boolean,
    modelId : number,
}

type GraphObj = {
    dataset : JsonObject,
    yscale : JsonObject
}

class LineGraph
{
    private datasets : GraphDataset[]
    private graphObjs : GraphObj[]
    private points : Point[]
    private datasetIdx : number
    private pointIdx : number
    private callback : (index : number, modelId : number) => void
    private readonly chart : any

    constructor(element : HTMLCanvasElement, datasets : GraphDataset[], callback : (index : number, modelId : number) => void, colorscale : string[] | null = null)
    {
        this.datasets = datasets
        this.datasetIdx = 0

        this.callback = callback
        this.pointIdx = 0

        this.points = this.datasets[0].points

        // Setup Datasets
        this.graphObjs = datasets.map(f => {
            let go : GraphObj = {
                dataset: {
                    label: f.title,
                    data : f.points.map(f => f.y),
                    pointRadius : new Array(f.points.length).fill(POINT_DEFAULT_SIZE),
                },
                yscale: {}
            }

            if (f.isLog)
            {
                go.yscale = {
                type: 'logarithmic',
                    reverse: true,
                    min: 0.1,
                    max: 20000,
                    title: {
                        display: true,
                        text: 'Prospect Ranking'
                    },
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
                    }
                }
            }
            else
            {
                go.yscale = {
                    min: 0,
                    max: Math.max(16, 
                        f.points.map(g => g.y).reduce((a,b) => Math.max(a,b), 0) + 2),
                    grid: {
                        color: css.background_low
                    },
                    title: {
                        display: true,
                        text: 'Expected WAR Through Control'
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

            return go
        })

        // @ts-ignore
        this.chart = new Chart(element, {
            type: 'line',
            data: {
                labels: this.points.map(f => f.label),
                datasets : [this.graphObjs[0].dataset],   
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: {
                    duration: 0,
                },
                plugins: {
                    legend: {
                        display: false,
                    },
                },
                // @ts-ignore
                onClick: (e, elements) => {
                    const points = this.chart.getElementsAtEventForMode(e, 'nearest', {intesect: false, axis: 'x'}, true);
                    if (points.length > 0)
                    {
                        const firstPoint = points[0]
                        const index = firstPoint.index;
                        this.highlight_index(index)
                        this.callback(index, this.getSelectedModel())
                    }
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

        if (this.points.length > 0)
            this.highlight_index(this.points.length - 1);
    }

    highlight_index(index : number) : void
    {
        this.pointIdx = index
        let pointRadius : number[] = new Array(this.points.length).fill(POINT_DEFAULT_SIZE)
        pointRadius[index] = POINT_HIGHLIGHT_SIZE
        this.chart.data.datasets[0].pointRadius = pointRadius
        this.chart.update()
    }

    increment_index(x_inc : number) : void
    {
        if ((x_inc === -1 && this.pointIdx > 0) || (x_inc === 1 && this.pointIdx < this.points.length - 1))
        {
            this.pointIdx += x_inc
            this.highlight_index(this.pointIdx)
            this.callback(this.getAdjustedPointIndex(), this.getSelectedModel())
        }
    }

    fireCallback() : void
    {
        
        this.callback(this.getAdjustedPointIndex(), this.getSelectedModel())
    }

    private getAdjustedPointIndex() : number
    {
        let index = this.pointIdx
        if (this.datasets[this.datasetIdx].isLog)
            index++
        return index
    }

    graphIsHitter() : boolean
    {
        return this.datasets[this.datasetIdx].isHitter
    }

    setDataset(idx : number) : void
    {
        this.datasetIdx = idx
        this.points = this.datasets[this.datasetIdx].points
        this.chart.data.datasets[0] = this.graphObjs[this.datasetIdx].dataset
        this.chart.data.labels = this.points.map(f => f.label)
        this.chart.options.scales.y = this.graphObjs[this.datasetIdx].yscale
        this.chart.update()

        if (this.points.length > 0)
            this.highlight_index(this.points.length - 1);
    }

    getSelectedModel() : number
    {
        return this.datasets[this.datasetIdx].modelId
    }
}