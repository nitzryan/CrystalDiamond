type SortValue = number | string | null
type TableSplit = 'all' | 'hitters' | 'pitchers'

type SortableColumn<T> = {
    header : string
    cls : string
    value : (t : T) => SortValue
    render : (t : T, idx : number) => string
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

    private sortColumn : number | null = null
    private sortAsc : boolean = false

    constructor(config : SortableTableConfig<T, V>)
    {

        this.head = config.head
        this.body = config.body
        this.getColumns = config.getColumns
        this.rowClass = config.rowClass ?? 'rankings_item'

        this.view = config.view.initial
        this.split = config.split.initial

        this.setupViewToggle(config.view.groupId, config.view.parse, config.view.onChange)
        this.setupSplitToggle(config.split.groupId, config.split.onChange)
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

    // Display the table
    render()
    {
        const columns = this.getColumns()

        // Create Header row
        let head = '<tr>'
        for (let i = 0; i < columns.length; i++)
        {
            const c = columns[i]
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
    }

    private sortedRows(columns : SortableColumn<T>[]) : T[]
    {
        if (this.sortColumn === null || this.sortColumn >= columns.length)
            return this.rows

        const col = columns[this.sortColumn]
        const sorted = [...this.rows]
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
}