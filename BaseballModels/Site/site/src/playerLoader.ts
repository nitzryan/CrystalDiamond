const rankings_header = getElementByIdStrict('rankings_header')
const rankings_list = getElementByIdStrict('rankings_list') as HTMLOListElement
const rankings_load = getElementByIdStrict('rankings_load') as HTMLButtonElement

type Player = {
    name : string,
    war : number,
    id : number,
    team : number,
    position : string,
    birthYear : number,
    birthMonth : number,
    level : number,
}

function createPlayer(obj : JsonObject)
{
    const p : Player = {
        name : getJsonString(obj, "firstName") + " " + getJsonString(obj, "lastName"),
        war : getJsonNumber(obj, "war"),
        id : getJsonNumber(obj, "mlbId"),
        team : getJsonNumber(obj, "teamId"),
        position : getJsonString(obj, "position"),
        birthYear : getJsonNumber(obj, "birthYear"),
        birthMonth : getJsonNumber(obj, "birthMonth"),
        level : getJsonNumber(obj, "highestLevel"),
    }
    return p
}

function createPlayerElement(player : Player, year : number, month : number) : HTMLLIElement
{
    const el = document.createElement('li') as HTMLLIElement
    const teamAbbr : string = player.team == 0 ? "" : getParentAbbr(player.team)
    let ageInYears = year - player.birthYear
    if (month < player.birthMonth)
        ageInYears--

    el.innerHTML = 
        `
        <div class='rankings_item'>
            <div class='rankings_row'>
                <div class='rankings_name'><a href='./player?id=${player.id}'>${player.name}</a></div>
                <div class='rankings_rightrow'>
                    <div><a href='./teams?id=${player.team}&year=${year}&month=${month}'>${teamAbbr}</a></div>
                    <div>${level_map[player.level]}</div>
                </div>
            </div>
            <div class='rankings_row'>
                <div>${player.war.toFixed(1)} WAR</div>
                <div class='rankings_rightrow'>
                    <div>${player.position}</div>
                    <div>${ageInYears}yrs</div>
                </div>
            </div>
        </div>
        `

    return el
}

class PlayerLoader
{
    private index : number
    private exhaustedElements : boolean = false
    private year : number
    private month : number
    private teamId : number | null

    constructor(year : number, month : number, teamId : number | null = null)
    {
        this.index = 0
        this.year = year
        this.month = month
        this.teamId = teamId
    }

    async getElements(num_elements : number) : Promise<HTMLLIElement[]>
    {
        if (this.exhaustedElements)
            return []

        const endRank = this.index + num_elements
        const response = this.teamId !== null ?
            await fetch(`/rankingsRequest?year=${this.year}&month=${this.month}&startRank=${this.index + 1}&endRank=${endRank}&teamId=${this.teamId}`) : 
            await fetch(`/rankingsRequest?year=${this.year}&month=${this.month}&startRank=${this.index + 1}&endRank=${endRank}`)

        const players = await response.json() as JsonArray

        this.exhaustedElements = (players.length != num_elements)
        this.index += players.length

        return players.map(f => {
            return createPlayerElement(createPlayer(f as JsonObject), this.year, this.month)
        })
    }
}

let playerLoader : PlayerLoader
function setupRankings(month : number, year : number, team : number | null, num_elements : number)
{
    playerLoader = new PlayerLoader(year, month, team)

    if (team === null)
        rankings_header.innerText = `Rankings for ${MONTH_CODES[month]} ${year}`
    else
        // @ts-ignore
        rankings_header.innerText = `Rankings for ${org_map["parents"][team]["name"]} ${MONTH_CODES[month]} ${year}`

    rankings_load.addEventListener('click', async (event) => {
        const elements = await playerLoader.getElements(num_elements)
        for (var el of elements)
        {
            rankings_list.appendChild(el)
        }
    })

    rankings_load.dispatchEvent(new Event('click'))
}