const rankings_selector = getElementByIdStrict('rankings_selector')
const year_select = getElementByIdStrict('year_select') as HTMLSelectElement
const month_select = getElementByIdStrict('month_select') as HTMLSelectElement
const model_select = getElementByIdStrict('model_select') as HTMLSelectElement
const team_select = document.getElementById('team_select') as HTMLSelectElement | null
const rankings_button = getElementByIdStrict('rankings_button') as HTMLButtonElement
const rankings_error = getElementByIdStrict('rankings_error')

let endYear : number = 0
let endMonth : number = 0

type SelectorArgs = {
    month : number,
    year : number,
    modelId : number,
    startYear : number,
    endYear : number, 
    endMonth : number,
    startTeam : number | null
}
function setupSelector(args : SelectorArgs)
{
    endYear = args.endYear
    endMonth = args.endMonth

    for (let i = args.startYear; i <= endYear; i++)
    {
        let opt = document.createElement('option')
        opt.value = i.toString()
        opt.innerText = i.toString()
        year_select.appendChild(opt)
    }

    for (let i = 4; i <= 9; i++)
    {
        let opt = document.createElement('option')
        opt.value = i.toString()
        opt.innerText = MONTH_CODES[i]
        month_select.appendChild(opt)
    }

    year_select.value = args.year.toString()
    month_select.value = args.month.toString()
    model_select.value = args.modelId.toString()

    year_select.addEventListener('change', selectorEventHandler)
    month_select.addEventListener('change', selectorEventHandler)
    
    if (team_select !== null && args.startTeam !== null){
        setupTeamSelector(args.startTeam)
        team_select.addEventListener('change', selectorEventHandler)
    }
}

function selectorEventHandler(this : HTMLSelectElement, ev : Event) : void
{
    const selectedMonth : number = parseInt(month_select.value)
    const selectedYear : number = parseInt(year_select.value)

    if (endYear == selectedYear && endMonth < selectedMonth)
    {
        rankings_button.classList.add('hidden')
        rankings_error.classList.remove('hidden')
    } else {
        rankings_error.classList.add('hidden')
        rankings_button.classList.remove('hidden')
    }
}

function setupTeamSelector(teamId : number)
{
    if (org_map === null)
        throw new Error("org_map null at setupSelector")
        
    if (team_select === null)
        throw new Error('team_select null in setupTeamSelector')

    var parents = org_map["parents"] as JsonObject
    var teams = []
    for (var id in parents)
    {
        teams.push({
            id: parseInt(id),
            // @ts-ignore
            abbr : parents[id]['abbr'] as string
        })
    }

    teams.sort((a,b) => {
        return a.abbr.localeCompare(b.abbr)
    })

    const elements : HTMLOptionElement[] = teams.map(f => {
        let el = document.createElement('option') as HTMLOptionElement

        el.value = f.id.toString()
        el.innerText = f.abbr

        return el
    })

    for (var el of elements)
        team_select.appendChild(el)

    team_select.value = teamId.toString()
}