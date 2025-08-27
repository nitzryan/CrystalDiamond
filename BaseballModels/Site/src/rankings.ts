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

async function main()
{
    const player_search_data = retrieveJson('../../assets/player_search.json.gz')
    org_map = await retrieveJson("../../assets/map.json.gz")
    const month : number = getQueryParam('month')
    const year : number = getQueryParam('year')
    
    const rankingJson = await retrieveJson(`../../assets/ranking/${month}-${year}.json.gz`)
    const players = getPlayers(rankingJson)
    const playerLoader = new PlayerLoader(players)

    rankings_load.addEventListener('click', (event) => {
        const elements = playerLoader.getListElements()
        for (var el of elements)
        {
            rankings_list.appendChild(el)
        }
    })

    rankings_load.dispatchEvent(new Event('click'))
    searchBar = new SearchBar(await player_search_data)
}

main()