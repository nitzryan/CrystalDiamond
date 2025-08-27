const rankings_selector = getElementByIdStrict('ranking_selector')
const year_select = getElementByIdStrict('year_select') as HTMLSelectElement
const month_select = getElementByIdStrict('month_select') as HTMLSelectElement
const rankings_button = getElementByIdStrict('rankings_button') as HTMLButtonElement
const rankings_error = getElementByIdStrict('rankings_error')
const rankings_header = getElementByIdStrict('rankings_header')
const rankings_list = getElementByIdStrict('rankings_list') as HTMLOListElement
const rankings_load = getElementByIdStrict('rankings_load') as HTMLButtonElement

let searchBar : SearchBar | null = null

type Player = {
    name : string,
    war : number,
    id : number,
    model : string,
    team : number
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
            element.innerHTML = `<div><a href='./player.html?id=${player.id}'>${player.name}</a><div><div class='war'>${player.war.toFixed(1)} WAR</div><div class='team${player.team}'>${teamAbbr}</span></div></div>`
            elements.push(element)
        }

        this.index += this.numElements
        return elements
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
            team : getJsonNumber(f, "team")
        }
        return player
    })
}

let endYear = 0
let endMonth = 0
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

async function setupSelector(month : number, year : number)
{
    const datesJson = await retrieveJson('../../assets/ranking/dates.json.gz')
    endYear = datesJson["endYear"] as number
    endMonth = datesJson["endMonth"] as number
    const startYear = datesJson["startYear"] as number

    for (let i = startYear; i <= endYear; i++)
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

    year_select.value = year.toString()
    month_select.value = month.toString()

    year_select.addEventListener('change', selectorEventHandler)
    month_select.addEventListener('change', selectorEventHandler)
}

async function main()
{
    const month : number = getQueryParam('month')
    const year : number = getQueryParam('year')
    const player_search_data = retrieveJson('../../assets/player_search.json.gz')
    const selector = setupSelector(month, year)
    org_map = await retrieveJson("../../assets/map.json.gz")
    
    
    const rankingJson = await retrieveJson(`../../assets/ranking/${month}-${year}.json.gz`)
    const players = getPlayers(rankingJson)
    const playerLoader = new PlayerLoader(players)

    rankings_header.innerText = `Rankings for ${MONTH_CODES[month]} ${year}`

    rankings_load.addEventListener('click', (event) => {
        const elements = playerLoader.getListElements()
        for (var el of elements)
        {
            rankings_list.appendChild(el)
        }
    })

    rankings_load.dispatchEvent(new Event('click'))
    searchBar = new SearchBar(await player_search_data)

    await selector
    rankings_button.addEventListener('click', (event) => {
        const mnth = month_select.value
        const yr = year_select.value
        
        window.location.href = `./rankings.html?year=${yr}&month=${mnth}`
    })
}

main()