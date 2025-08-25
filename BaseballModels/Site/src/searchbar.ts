class SearchBar
{
    private items: JsonArray
    private searchBox : HTMLInputElement
    private searchResults : HTMLElement
    private current_count : number

    constructor(json : JsonObject)
    {
        this.searchBox = getElementByIdStrict('searchBar') as HTMLInputElement
        this.searchResults = getElementByIdStrict('searchResults')
        this.items = json["players"] as JsonArray
        this.current_count = 0

        this.searchBox.addEventListener('input', (event) => {
            this.current_count++
            const idx = this.current_count
            
            const search_str = this.searchBox.value
            const results = search_str == "" ? "" : this.getResults(search_str)
            // Make sure another request hasn't been started
            if (idx != this.current_count)
                return

            this.searchResults.innerHTML = results
            if (results.length > 0)
                this.searchResults.classList.remove('hidden')
            else
                this.searchResults.classList.add('hidden')
        })
    }

    getResults(text : string) : string
    {
        text = text.toLowerCase()
        const valid = this.items.filter(f => 
            // @ts-ignore
            f["f"].includes(text) || f["l"].includes(text) || (f["f"] + " " + f["l"]).includes(text)
        ).sort((a, b) => {
            // @ts-ignore
            const r = a["f"].localeCompare(b["f"])
            // @ts-ignore
            return r !== 0 ? r : a["l"].localeCompare(b["l"])
        }) as JsonObject[]

        const htmlStrings = valid.map(f => {
            // @ts-ignore
            return `<li><a href="./player.html?id=${f["id"]}">${f["f"][0].toUpperCase() + f["f"].substring(1)} ${f["l"][0].toUpperCase() + f["l"].substring(1)}</a></li>`
        })

        return htmlStrings.join("\n")
    }
}