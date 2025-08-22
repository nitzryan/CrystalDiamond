class CSS_Style
{
    background_high : string
    background_med : string
    background_low : string
    text_high : string
    text_low : string
    

    constructor()
    {
        const style = window.getComputedStyle(document.body)

        this.background_high = style.getPropertyValue('--background-high')
        this.background_med = style.getPropertyValue('--background-med')
        this.background_low = style.getPropertyValue('--background-low')
        this.text_high = style.getPropertyValue('--text-high')
        this.text_low = style.getPropertyValue('--text-low')
    }
}

let css = new CSS_Style()