// Stacked bar of WAR already accumulated vs WAR still projected, with a marker showing
// where accumulated WAR would sit if it had been earned evenly over the elapsed months
const WAR_ACCUMULATED_COLOR : string = 'rgb(255, 159, 64)'
const WAR_PROJECTED_COLOR : string = 'rgba(255, 159, 64, 0.3)'
const WAR_PACE_DASH : number[] = [5, 3]
const WAR_PACE_OVERHANG : number = 8
const MODEL_MONTHS_PER_YEAR : number = 6

type WarProgress = {
    title : string,
    warAccumulated : number,
    warProjected : number,
    monthsProjected : number,
    monthsElapsed : number,
    monthsTotal : number,
}

function formatPercent(fraction : number) : string
{
    return `${(fraction * 100).toFixed(0)}%`
}

function formatCount(count : number, unit : string) : string
{
    return `${count} ${unit}${count === 1 ? '' : 's'}`
}

// Format time as years/months, with 6 months in a year
function formatModelMonths(months : number) : string
{
    const years = Math.floor(months / MODEL_MONTHS_PER_YEAR)
    const remainder = months % MODEL_MONTHS_PER_YEAR
    if (years === 0)
        return formatCount(remainder, 'month')
    if (remainder === 0)
        return formatCount(years, 'year')
    return `${formatCount(years, 'year')}, ${formatCount(remainder, 'month')}`
}

class WarProgressGraph
{
    private progress : WarProgress
    private readonly chart : any

    constructor(element : HTMLCanvasElement, progress : WarProgress, yscale : any)
    {
        this.progress = progress

        const pacePlugin = {
            id: 'warPace',
            // @ts-ignore
            afterDatasetsDraw: (chart) => {
                const pace = this.paceWar()
                const bar = chart.getDatasetMeta(0).data[0]
                if (pace === null || bar === undefined)
                    return

                const y = chart.scales.y.getPixelForValue(pace)
                const halfWidth = bar.width / 2 + WAR_PACE_OVERHANG
                const ctx = chart.ctx
                ctx.save()
                ctx.strokeStyle = css.text_high
                ctx.lineWidth = 2
                ctx.setLineDash(WAR_PACE_DASH)
                ctx.beginPath()
                ctx.moveTo(bar.x - halfWidth, y)
                ctx.lineTo(bar.x + halfWidth, y)
                ctx.stroke()
                ctx.restore()
            }
        }

        // @ts-ignore
        this.chart = new Chart(element, {
            type: 'bar',
            data: {
                labels: [''],
                datasets: [
                    {
                        label: 'Accumulated',
                        data: [progress.warAccumulated],
                        backgroundColor: WAR_ACCUMULATED_COLOR,
                        borderColor: WAR_ACCUMULATED_COLOR,
                        borderWidth: 2,
                        maxBarThickness: 80,
                    },
                    {
                        label: 'Projected',
                        data: [progress.warProjected],
                        backgroundColor: WAR_PROJECTED_COLOR,
                        borderColor: WAR_ACCUMULATED_COLOR,
                        borderWidth: 2,
                        maxBarThickness: 80,
                    },
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: {
                    duration: 0,
                },
                plugins: {
                    title: {
                        display: true,
                        text: progress.title,
                    },
                    subtitle: {
                        display: true,
                        text: this.subtitle(),
                    },
                    legend: {
                        position: 'bottom',
                        // Legend is a key only; don't let clicks hide series
                        onClick: () => {},
                        labels: {
                            // @ts-ignore
                            generateLabels: (chart) => {
                                // @ts-ignore
                                let items = Chart.defaults.plugins.legend.labels.generateLabels(chart)
                                if (this.paceWar() !== null)
                                {
                                    items.push({
                                        text: 'Even Pace',
                                        fillStyle: 'rgba(0, 0, 0, 0)',
                                        strokeStyle: css.text_high,
                                        lineWidth: 2,
                                        lineDash: WAR_PACE_DASH,
                                        fontColor: items.length > 0 ? items[0].fontColor : undefined,
                                        hidden: false,
                                    })
                                }
                                return items
                            }
                        }
                    },
                    tooltip: {
                        callbacks: {
                            // @ts-ignore
                            label: (item) => {
                                const p = this.progress
                                if (item.datasetIndex === 0)
                                    return `\tAccumulated: ${p.warAccumulated.toFixed(1)} WAR`
                                return `\tProjected: ${p.warProjected.toFixed(1)} WAR over ${formatModelMonths(p.monthsProjected)}`
                            },
                            // The projected fill is translucent, so show it as it looks on the chart
                            // @ts-ignore
                            labelColor: (item) => getTooltipColorAsDisplayed(item),
                        },
                    }
                },
                scales: {
                    x: {
                        stacked: true,
                        grid: {
                            display: false,
                        },
                    },
                    y: this.stackedScale(yscale),
                },
            },
            plugins: [pacePlugin],
        })
    }

    // WAR accumulated by now if the total had been earned evenly across the months
    private paceWar() : number | null
    {
        const p = this.progress
        if (p.monthsTotal <= 0)
            return null
        return (p.warAccumulated + p.warProjected) * p.monthsElapsed / p.monthsTotal
    }

    private subtitle() : string[]
    {
        const p = this.progress
        const total = p.warAccumulated + p.warProjected

        // Only a meaningful share when both parts are non-negative pieces of a positive total
        const isShare = p.warAccumulated >= 0 && p.warProjected >= 0 && total > 0
        const warShare = isShare ? ` (${formatPercent(p.warAccumulated / total)})` : ''
        const monthShare = p.monthsTotal > 0 ? ` (${formatPercent(p.monthsElapsed / p.monthsTotal)})` : ''
        return [
            `${formatModelMonths(p.monthsElapsed)} elapsed${monthShare}`,
            `${formatModelMonths(p.monthsProjected)} remaining`,
        ]
    }

    private stackedScale(yscale : any) : any
    {
        let scale = Object.assign({}, yscale)
        scale.stacked = true
        return scale
    }

    update(progress : WarProgress, yscale : any) : void
    {
        this.progress = progress
        this.chart.data.datasets[0].data = [progress.warAccumulated]
        this.chart.data.datasets[1].data = [progress.warProjected]
        this.chart.options.plugins.title.text = progress.title
        this.chart.options.plugins.subtitle.text = this.subtitle()
        this.chart.options.scales.y = this.stackedScale(yscale)
        this.chart.update()
    }
}
