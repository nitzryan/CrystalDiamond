type SortValue = number | string | null
type TableSplit = 'all' | 'hitters' | 'pitchers'

type SortableColumn<T> = {
    header : string
    cls : string
    sortable : boolean
    value : (t : T) => SortValue
    render : (t : T, idx : number) => string
    total? : (rows : T[]) => string
}

function parseTableSplit(value : string) : TableSplit
{
    if (value === 'hitters' || value === 'pitchers')
        return value
    return 'all'
}

type SortableTableConfig<T, V extends string> = {
    head : HTMLElement
    body : HTMLElement
    getColumns : () => SortableColumn<T>[]
    rowClass : string
    footerClass? : string
    view : {
        groupId : string
        initial : V
        parse : (value : string) => V
        onChange? : (view : V) => void
    }
    split : {
        groupId : string
        initial : TableSplit
        onChange? : (split : TableSplit) => void
    }
    split_filter : (row : T, split : TableSplit) => boolean
    view_filter : (t : T, view : V) => boolean
    view_split_default_column : (split : TableSplit, v : V) => number | null
    
    countPerPage? : number
    forceLastVisible? : boolean
    pageChangeElement : HTMLElement
}

type TablePageTracker = {
    currentPage : number
    maxPage : number
    elementsPerPage : number
    forceLastVisible? : boolean

    // Defined by SortableTable
    pageDownButton? : HTMLButtonElement
    pageUpButton? : HTMLButtonElement
    pageCountElement? : HTMLElement
    pageMaxElement? : HTMLElement
}

class SortableTable<T, V extends string>
{
    rows : T[] = []
    split : TableSplit = 'all'
    view : V

    private head : HTMLElement
    private body : HTMLElement
    private getColumns : () => SortableColumn<T>[]
    private rowClass : string
    private footerClass : string

    private pageTracker? : TablePageTracker
    private pageChangeElement : HTMLElement

    private sortColumn : number | null = null
    private sortAsc : boolean = false

    private split_filter : (row : T, split : TableSplit) => boolean
    private view_filter : (t : T, view : V) => boolean
    private view_split_default_column : (split : TableSplit, v : V) => number | null

    constructor(config : SortableTableConfig<T, V>)
    {
        this.head = config.head
        this.body = config.body
        this.pageChangeElement = config.pageChangeElement
        this.getColumns = config.getColumns
        this.rowClass = config.rowClass ?? 'rankings_item'
        this.footerClass = config.footerClass ?? 'rankings_total'

        this.view = config.view.initial
        this.split = config.split.initial

        this.split_filter = config.split_filter
        this.view_filter = config.view_filter
        this.view_split_default_column = config.view_split_default_column

        this.setupViewToggle(config.view.groupId, config.view.parse, config.view.onChange)
        this.setupSplitToggle(config.split.groupId, config.split.onChange)

        if (config.countPerPage !== undefined)
        {
            this.pageTracker = {
                currentPage : -1,
                maxPage : -1,
                elementsPerPage : config.countPerPage,
                forceLastVisible : config.forceLastVisible
            }

            this.pageTracker.pageDownButton = document.createElement('button')
            this.pageTracker.pageDownButton.innerText = '<'
            this.pageTracker.pageDownButton.addEventListener('click', () => {
                // @ts-ignore
                this.SetPage(this.pageTracker.currentPage - 1)
            })

            this.pageTracker.pageUpButton = document.createElement('button')
            this.pageTracker.pageUpButton.innerText = '>'
            this.pageTracker.pageUpButton.addEventListener('click', () => {
                // @ts-ignore
                this.SetPage(this.pageTracker.currentPage + 1)
            })

            this.pageTracker.pageCountElement = document.createElement('div')
            this.pageTracker.pageMaxElement = document.createElement('div')
            
            const pageTrackCenter = document.createElement('div')
            pageTrackCenter.classList.add('table_page_count_center')
            const ofElement = document.createElement('div')
            ofElement.innerText = "of"

            pageTrackCenter.appendChild(this.pageTracker.pageCountElement)
            pageTrackCenter.appendChild(ofElement)
            pageTrackCenter.appendChild(this.pageTracker.pageMaxElement)

            this.pageChangeElement.appendChild(this.pageTracker.pageDownButton)
            this.pageChangeElement.appendChild(pageTrackCenter)
            this.pageChangeElement.appendChild(this.pageTracker.pageUpButton)

            this.pageChangeElement.classList.add('table_page_count')
        }
        else
        {
            this.pageChangeElement.classList.add('hidden')
        }
    }

    // Call when the column set changes (e.g. switching views)
    private resetSort()
    {
        this.sortColumn = null
        this.sortAsc = false
    }

    // Allows for splitting on TableSplit
    private setupSplitToggle(groupId : string, 
                     onChange? : (split : TableSplit) => void)
    {
        const buttons = Array.from(
            document.querySelectorAll<HTMLButtonElement>(`#${groupId} button`))
        buttons.forEach(btn => {
            btn.addEventListener('click', () => {
                this.split = parseTableSplit(btn.getAttribute('data-split') ?? '')
                this.syncSplitButtons(groupId)
                onChange?.(this.split)
                this.setDefaultColumn()
                this.render()
            })
        })
        this.syncSplitButtons(groupId)
    }

    private syncSplitButtons(groupId : string)
    {
        document.querySelectorAll<HTMLButtonElement>(`#${groupId} button`)
            .forEach(btn => btn.classList.toggle('selected',
                btn.getAttribute('data-split') === this.split))
    }

    private setupViewToggle(groupId : string, 
                    parse : (value : string) => V,
                    onChange? : (view : V) => void)
    {
        const buttons = Array.from(
            document.querySelectorAll<HTMLButtonElement>(`#${groupId} button`))
        buttons.forEach(btn => {
            btn.addEventListener('click', () => {
                this.view = parse(btn.getAttribute('data-view') ?? '')
                this.resetSort()                    // column set changed
                this.syncViewButtons(groupId)
                onChange?.(this.view)
                this.setDefaultColumn()
                this.render()
            })
        })
        this.syncViewButtons(groupId)
        onChange?.(this.view)
    }
    private syncViewButtons(groupId : string)
    {
        document.querySelectorAll<HTMLButtonElement>(`#${groupId} button`)
            .forEach(btn => btn.classList.toggle('selected',
                btn.getAttribute('data-view') === this.view))
    }

    setDefaultColumn()
    {
        const default_order_column = this.view_split_default_column(this.split, this.view)
        if (default_order_column != null)
            this.sortColumn = default_order_column
    }

    // Display the table
    render()
    {
        // Set Page count if not set
        if (this.pageTracker !== undefined && this.pageTracker.currentPage < 0 && this.pageTracker.pageMaxElement !== undefined)
        {
            this.pageTracker.currentPage = 1
        }

        const columns = this.getColumns()

        // Create Header row
        let head = '<tr>'
        for (let i = 0; i < columns.length; i++)
        {
            const c = columns[i]
            
            // Create sortable or non-sortable header
            if (c.sortable === false)
            {
                head += `<th>${c.header}</th>`
                continue
            }

            let arrow = ''
            if (i === this.sortColumn)
                arrow = this.sortAsc ? ' ▲' : ' ▼'
            head += `<th class='sortable' data-col='${i}'>${c.header}${arrow}</th>`
        }
        head += '</tr>'
        this.head.innerHTML = head

        // Attach click handlers to headers
        this.head.querySelectorAll<HTMLTableCellElement>('th.sortable').forEach(th => {
            th.addEventListener('click', () => {
                const idx = parseInt(th.getAttribute('data-col') ?? '0')
                if (this.sortColumn === idx)
                    this.sortAsc = !this.sortAsc           // second click flips direction
                else
                {
                    this.sortColumn = idx
                    this.sortAsc = false              // new column: descending first
                }
                this.render()
            })
        })

        const rows = this.sortedRows(columns)
        this.body.innerHTML = ''
        for (let i = 0; i < rows.length; i++)
        {
            const t = rows[i]
            let row = ''
            for (const c of columns)
                row += `<td class='${c.cls}'>${c.render(t, i)}</td>`
            const el = document.createElement('tr')
            el.classList.add(this.rowClass)
            el.innerHTML = row
            this.body.appendChild(el)
        }

        // Add Summation Row
        if (columns.some(c => c.total !== undefined))
        {
            let row = ''
            for (const c of columns)
                row += `<td class='${c.cls}'>${c.total ? c.total(rows) : ''}</td>`
            const el = document.createElement('tr')
            el.classList.add(this.footerClass)
            el.innerHTML = row
            this.body.appendChild(el)
        }

        if (this.pageTracker !== undefined)
            this.SetPage(this.pageTracker.currentPage)
    }

    private sortedRows(columns : SortableColumn<T>[]) : T[]
    {
        const rows = this.rows.filter(r => this.split_filter(r, this.split) && this.view_filter(r, this.view))
        
        if (this.sortColumn === null || this.sortColumn >= columns.length)
            return rows

        const col = columns[this.sortColumn]
        const sorted = [...rows]
        sorted.sort((a, b) => {
            const av = col.value(a)
            const bv = col.value(b)
            if (av === null && bv === null) return 0
            if (av === null) return 1   // nulls always last
            if (bv === null) return -1
            if (typeof av === 'string' || typeof bv === 'string')
            {
                const cmp = String(av).localeCompare(String(bv))
                return this.sortAsc ? cmp : -cmp
            }
            return this.sortAsc ? av - bv : bv - av
        })
        return sorted
    }

    private SetPage(newPage : number)
    {
        if (this.pageTracker === undefined || this.pageTracker.pageCountElement === undefined || this.pageTracker.pageMaxElement === undefined)
            return

        // Set current page and values
        this.pageTracker.currentPage = newPage
        this.pageTracker.pageCountElement.innerText = newPage.toString()

        const minElement = (this.pageTracker.currentPage - 1) * this.pageTracker.elementsPerPage
        const maxElement = minElement + this.pageTracker.elementsPerPage

        const rows = (this.body as HTMLTableSectionElement).rows

        // Set Max Page
        this.pageTracker.maxPage = Math.ceil(rows.length / this.pageTracker.elementsPerPage)
        if (this.pageTracker.currentPage < 1)
            this.pageTracker.currentPage = 1
        else if (this.pageTracker.currentPage > this.pageTracker.maxPage)
            this.pageTracker.currentPage = this.pageTracker.maxPage
        
        if (this.pageTracker.maxPage === 1)
        {
            this.pageChangeElement.classList.add('hidden')
            return
        }
        
        this.pageTracker.pageMaxElement.innerText = this.pageTracker.maxPage.toString()

        // Hide elements not on page
        for (const row of Array.from(rows))
        {
            row.classList.toggle('hidden', row.rowIndex < minElement || row.rowIndex >= maxElement)
        }

        if (this.pageTracker.forceLastVisible)
            rows[rows.length - 1].classList.remove('hidden')
    }
}