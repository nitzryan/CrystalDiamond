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

    let element = document.createElement('li') as HTMLLIElement
    element.innerHTML = `<div><a href='./player?id=${player.id}'>${player.name}</a><div>${player.position}</div><div><div class='war'>${player.war.toFixed(1)} WAR</div><div class='team${player.team}'>${teamAbbr}</span></div></div>`
    return element
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