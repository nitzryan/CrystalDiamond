let searchBar : SearchBar | null = null

class SearchBar
{
    private mlbItems: JsonArray
    private milbItems : JsonArray
    private searchBox : HTMLInputElement
    private majorsSearchResults : HTMLElement
    private minorsSearchResults : HTMLElement
    private searchResultsContainer : HTMLElement
    private current_count : number

    constructor(json : JsonObject)
    {
        this.searchBox = getElementByIdStrict('searchBar') as HTMLInputElement
        this.majorsSearchResults = getElementByIdStrict('majorsSearchResults')
        this.minorsSearchResults = getElementByIdStrict('minorsSearchResults')
        this.searchResultsContainer = getElementByIdStrict('searchResultsContainer')

        function removeAccents(str : string) {
            return str.normalize("NFD").replace(/[\u0300-\u036f]/g, "");
        }

        let players = json["players"] as JsonArray
        players.forEach(f => {
            // @ts-ignore
            f["fstLoc"] = removeAccents(f["f"]).toLowerCase()
            // @ts-ignore
            f["lstLoc"] = removeAccents(f["l"]).toLowerCase()
        })

        // @ts-ignore
        this.mlbItems = players.filter(f => (f["s"] & 2) === 2)
        // @ts-ignore
        this.milbItems = players.filter(f => (f["s"] & 2) === 0)
        // @ts-ignore
        this.current_count = 0

        this.searchBox.addEventListener('input', (event) => {
            this.current_count++
            const idx = this.current_count
            
            const search_str = this.searchBox.value
            const resultsPair = search_str == "" ? null : this.getResults(search_str)
            // Make sure another request hasn't been started
            if (idx != this.current_count)
                return

            if (resultsPair !== null)
            {
                this.searchResultsContainer.classList.remove('hidden')
                this.majorsSearchResults.innerHTML = resultsPair[0]
                this.minorsSearchResults.innerHTML = resultsPair[1]
            }
                
            else
            {
                this.searchResultsContainer.classList.add('hidden')
                this.majorsSearchResults.innerHTML = ''
                this.minorsSearchResults.innerHTML = ''
            }  
        })
    }

    getResults(text : string) : string[] | null
    {
        text = text.toLowerCase()

        const filterFunction = (f : JsonValue)  => 
        {
            // @ts-ignore
            const first : string = f["fstLoc"]
            // @ts-ignore
            const last : string = f["lstLoc"]

            return first.startsWith(text) 
                || last.startsWith(text) 
                || (first + " " + last).startsWith(text)
                || (last + " " + first).startsWith(text)
        }

        const sortFunction = (a : JsonValue, b : JsonValue) => {
            // @ts-ignore
            const a_s = a["s"] as number
            // @ts-ignore
            const b_s = b["s"] as number
            if (a_s != b_s)
                return b_s - a_s

            // @ts-ignore
            const r = a["f"].localeCompare(b["f"])
            // @ts-ignore
            return r !== 0 ? r : a["l"].localeCompare(b["l"])
        }

        const validMLB = this.mlbItems.filter(filterFunction).sort(sortFunction) as JsonObject[]
        const validMilb = this.milbItems.filter(filterFunction).sort(sortFunction) as JsonObject[]

        if (validMLB.length == 0 && validMilb.length == 0)
            return null

        const elementMap = (f : JsonObject) => {
            const id : number = f["o"] as number
            const teamString : string = id > 0 ? getParentAbbr(id)  : ""
            // @ts-ignore
            return `<li><a href="./player?id=${f["id"]}">${f["f"][0].toUpperCase() + f["f"].substring(1)} ${f["l"][0].toUpperCase() + f["l"].substring(1)}</a><div class="teamsearch team${id}">${teamString}</div></li>`
        }

        const htmlMajorsStrings = validMLB.map(elementMap).join("\n")
        const htmlMinorsStrings = validMilb.map(elementMap).join("\n")

        return [htmlMajorsStrings, htmlMinorsStrings]
    }
}