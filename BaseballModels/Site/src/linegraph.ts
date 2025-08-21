type Point = 
{
    y : number;
    label : string,
}

class LineGraph
{
    points : Point[];
    colorscale :  string[];
    readonly chart : any

    constructor(element : HTMLCanvasElement, points : Point[], colorscale : string[] | null)
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
            type: 'line',
            data: {
                labels: points.map(f => f.label),
                datasets : [{
                    label: 'Model 1',
                    data: points.map(f => f.y)
                }],   
            },
            options: {
                scales: {
                    y: {
                        min: 0
                    }
                }
            }
        })
    }
}