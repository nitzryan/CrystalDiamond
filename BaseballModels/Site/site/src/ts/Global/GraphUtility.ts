// Tooltip colour boxes paint the fill over white, so a translucent fill comes out much lighter
// there than on the chart, where it sits on the dark canvas background. Blending the fill onto
// the canvas background gives the colour that is actually seen on the chart
function getFillAsDisplayed(color : string) : string
{
    let canvas = document.createElement('canvas')
    canvas.width = 1
    canvas.height = 1
    const ctx = canvas.getContext('2d', { willReadFrequently: true })
    if (ctx === null)
        return color

    ctx.fillStyle = css.background_high.trim()
    ctx.fillRect(0, 0, 1, 1)
    ctx.fillStyle = color
    ctx.fillRect(0, 0, 1, 1)
    const pixel = ctx.getImageData(0, 0, 1, 1).data
    return `rgb(${pixel[0]}, ${pixel[1]}, ${pixel[2]})`
}

// Default tooltip colour box, with the fill swapped for how it looks on the chart
// @ts-ignore
function getTooltipColorAsDisplayed(item) : any
{
    // @ts-ignore
    let colors = Chart.defaults.plugins.tooltip.callbacks.labelColor(item)
    colors.backgroundColor = getFillAsDisplayed(colors.backgroundColor)
    return colors
}