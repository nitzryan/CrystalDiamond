type StatPlayer<T> = 
{
    player : DB_Player,
    obj : T
}

class PageSelector
{
    private callback : (page : number) => void
    public baseElement : HTMLElement

    constructor(numPages : number, callback : (page : number) => void)
    {
        this.callback = callback
        this.baseElement = document.createElement('div')
        this.baseElement.classList.add('page_selector')

        const pageInput = document.createElement('input') as HTMLInputElement
        pageInput.type = "number"
        pageInput.min = "1"
        pageInput.value = pageInput.min
        pageInput.max = numPages.toString()
        pageInput.addEventListener('change', f => this.callback(Number(pageInput.value) - 1))

        const minPageButton = document.createElement('button')
        minPageButton.innerText = "<<"
        minPageButton.addEventListener('click', f => {
            pageInput.value = pageInput.min
            this.callback(Number(pageInput.value) - 1)
            })

        const maxPageButton = document.createElement('button')
        maxPageButton.innerText = ">>"
        maxPageButton.addEventListener('click', f => {
            pageInput.value = pageInput.max
            this.callback(Number(pageInput.value) - 1)
            })

        this.baseElement.appendChild(minPageButton)
        this.baseElement.appendChild(pageInput)
        this.baseElement.appendChild(maxPageButton)
    }
}

class SortableStatsViewer<T>
{
    private data : StatPlayer<T>[]
    private readonly fRow : (sp : StatPlayer<T>) => [string, string[]][]

    private startIdx : number
    private numEntriesVisible : number

    private tableElement : HTMLTableElement
    private tableBody : HTMLTableSectionElement
    private pageSelector : PageSelector
    public baseElement : HTMLElement

    public constructor(
        vars : JsonArray,
        private readonly itemConstructor : new (data: JsonObject) => T,
        titles : string[],
        fRow : (sp : StatPlayer<T>) => [string, string[]][],
        startIdx : number = 1,
        numEntriesVisible : number = 30,
    )
    {
        this.data = vars.map(f => {
            f = f as JsonObject;
            return {
                player : new DB_Player(f),
                obj : new itemConstructor(f)
            }
        })

        this.fRow = fRow
        this.startIdx = startIdx
        this.numEntriesVisible = numEntriesVisible

        this.tableElement = document.createElement('table')
        this.tableElement.classList.add('stats_table')
        const tableHeaderText = titles.map(f => `<td>${f}</td>`).reduce((a, b) => a + b)
        this.tableElement.innerHTML = `<thead><tr>${tableHeaderText}</tr></thead>`
        this.tableBody = document.createElement('tbody')
        this.tableElement.appendChild(this.tableBody)

        this.pageSelector = new PageSelector(
            Math.ceil(this.data.length / this.numEntriesVisible),
            (page) => this.ChangePage(page));

        this.baseElement = document.createElement('div')
        this.baseElement.classList.add('sortableStatsViewer')
        this.baseElement.appendChild(this.tableElement)
        this.baseElement.appendChild(this.pageSelector.baseElement)

        this.ChangePage(0)
    }

    private ChangePage(pageIdx : number) : void
    {
        if (this.data.length == 0)
            return;
        
        this.startIdx = pageIdx * this.numEntriesVisible
        if (this.startIdx >= this.data.length)
            return this.ChangePage(pageIdx - 1)

        const tableRows : HTMLElement[] = 
            this.data.slice(this.startIdx, this.startIdx + this.numEntriesVisible)
            .map(f => {
                const tr = document.createElement('tr')
                const row_values = this.fRow(f)
                row_values.forEach(g => {
                    const td = document.createElement('td')
                    td.innerHTML = g[0]
                    g[1].forEach(h => td.classList.add(h))
                    tr.appendChild(td)
                })
                return tr
            })
        this.tableBody.replaceChildren(...tableRows)
    }
}