const POINT_DEFAULT_SIZE : number = 3
const POINT_HIGHLIGHT_SIZE : number = 9

class LineGraph
{
    private points : Point[];
    private colorscale :  string[]
    private callback : (index : number) => void
    private readonly chart : any

    constructor(element : HTMLCanvasElement, points : Point[], callback : (index : number) => void, colorscale : string[] | null = null)
    {
        this.points = points
        this.callback = callback
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
            type: 'line',
            data: {
                labels: points.map(f => f.label),
                datasets : [{
                    label: 'Model 1',
                    data: points.map(f => f.y),
                    pointRadius : new Array(points.length).fill(POINT_DEFAULT_SIZE),
                }],   
            },
            options: {
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
                        min: 0
                    }
                }
            }
        })

        if (points.length > 0)
            this.highlight_index(points.length - 1);
    }

    highlight_index(index : number) : void
    {
        let pointRadius : number[] = new Array(this.points.length).fill(POINT_DEFAULT_SIZE)
        pointRadius[index] = POINT_HIGHLIGHT_SIZE
        this.chart.data.datasets[0].pointRadius = pointRadius
        this.chart.update()
    }
}