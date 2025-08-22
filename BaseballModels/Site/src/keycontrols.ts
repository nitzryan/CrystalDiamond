class KeyControls
{
    private callback : (x_inc : number) => void

    constructor(document : HTMLDocument, callback : (x_inc : number) => void)
    {
        this.callback = callback

        document.addEventListener('keydown', (event) => {
            if (event.key === "ArrowLeft")
                callback(-1)
            else if (event.key === "ArrowRight")
                callback(1)
        })
    }
}