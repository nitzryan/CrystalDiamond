const rankings_selector = getElementByIdStrict('ranking_selector')
const year_select = getElementByIdStrict('year_select') as HTMLSelectElement
const month_select = getElementByIdStrict('month_select') as HTMLSelectElement
const team_select = document.getElementById('team_select') as HTMLSelectElement | null
const rankings_button = getElementByIdStrict('rankings_button') as HTMLButtonElement
const rankings_error = getElementByIdStrict('rankings_error')
const rankings_header = getElementByIdStrict('rankings_header')
const rankings_list = getElementByIdStrict('rankings_list') as HTMLOListElement
const rankings_load = getElementByIdStrict('rankings_load') as HTMLButtonElement

let endYear : number = 0
let endMonth : number = 0

type Player = {
    name : string,
    war : number,
    id : number,
    model : string,
    team : number,
    position : string,
}

class PlayerLoader
{
    private players : Player[]
    private index : number
    private numElements : number = 100

    constructor(ps : Player[])
    {
        this.players = ps
        this.index = 0
    }

    getListElements() : HTMLLIElement[]
    {
        var elements : HTMLLIElement[] = []

        for (let i = this.index; i < this.index + this.numElements; i++)
        {
            if (i >= this.players.length)
            {
                rankings_load.classList.add('hidden')
                break
            }
            const player = this.players[i]
            
            let element = document.createElement('li') as HTMLLIElement
            const teamAbbr : string = player.team == 0 ? "" : getParentAbbr(player.team)
            element.innerHTML = `<div><a href='./player.html?id=${player.id}'>${player.name}</a><div>${player.position}</div><div><div class='war'>${player.war.toFixed(1)} WAR</div><div class='team${player.team}'>${teamAbbr}</span></div></div>`
            elements.push(element)
        }

        this.index += this.numElements
        return elements
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

type SelectorArgs = {
    month : number,
    year : number,
    startYear : number,
    endYear : number, 
    endMonth : number,
    startTeam : number | null
}
async function setupSelector(args : SelectorArgs)
{
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

    year_select.addEventListener('change', selectorEventHandler)
    month_select.addEventListener('change', selectorEventHandler)
    
    if (team_select !== null && args.startTeam !== null){
        setupTeamSelector(args.startTeam)
        team_select.addEventListener('change', selectorEventHandler)
    }
}

function getPlayers(rankingJson : JsonObject) : Player[]
{
    const jsonArray = rankingJson["players"] as JsonArray
    return jsonArray.map(f => {
        f = f as JsonObject
        const player : Player = {
            name: getJsonString(f, "name"),
            war : getJsonNumber(f, "war"),
            id : getJsonNumber(f, "id"),
            model : getJsonString(f, "model"),
            team : getJsonNumber(f, "team"),
            position : getJsonString(f, "position")
        }
        return player
    })
}

function setupRankings(obj : JsonObject, month : number, year : number, team : number | null)
{
    const players = getPlayers(obj)
    const playerLoader = new PlayerLoader(players)

    if (team === null)
        rankings_header.innerText = `Rankings for ${MONTH_CODES[month]} ${year}`
    else
        // @ts-ignore
        rankings_header.innerText = `Rankings for ${org_map["parents"][team]["name"]} ${MONTH_CODES[month]} ${year}`

    rankings_load.addEventListener('click', (event) => {
        const elements = playerLoader.getListElements()
        for (var el of elements)
        {
            rankings_list.appendChild(el)
        }
    })

    rankings_load.dispatchEvent(new Event('click'))
}