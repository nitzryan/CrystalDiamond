class PieGraph
{
    private points : Point[];
    private colorscale :  string[];
    private readonly chart : any

    constructor(element : HTMLCanvasElement, points : Point[], title_text: string, colorscale : string[] | null = null)
    {
        this.points = points
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

        // @ts-ignore
        this.chart = new Chart(element, {
            type: 'pie',
            data: {
                labels: points.map(f => f.label),
                datasets: [{
                    label: '',
                    data: points.map(f => f.y),
                    backgroundColor: this.colorscale
                }]
            },
            options: {
                responseive: true,
                maintainAspectRatio: false,
                animation: {
                    duration: 0,
                },
                plugins: {
                    legend: {
                        display: false,
                    },
                    title: {
                        display:true,
                        text:title_text,
                    },
                    tooltip: {
                        callbacks: {
                            // @ts-ignore
                            label: (item) => {
                                const point = this.points[item.dataIndex]
                                return `\t${(point.y * 100).toFixed(1)}%`
                            }
                        },
                    }
                },
                hover: {
                    mode: null,
                }
            }
        })
    }

    updateChart(values : number[], title_text : string, labels : string[])
    {
        this.points.forEach((f, idx) => {f.y = values[idx]})
        this.chart.data.datasets[0].data = values
        this.chart.data.labels = labels
        this.chart.options.plugins.title.text = title_text
        this.chart.update()
    }
}