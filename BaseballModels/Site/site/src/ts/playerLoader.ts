const rankings_header = getElementByIdStrict('rankings_header')
const rankings_table = getElementByIdStrict('rankings_table') as HTMLTableElement
const rankings_table_head = getElementByIdStrict('rankings_table_head') as HTMLTableSectionElement
const rankings_table_body = getElementByIdStrict('rankings_table_body') as HTMLTableSectionElement
const rankings_load = getElementByIdStrict('rankings_load') as HTMLButtonElement

type Player = {
    name : string,
    war : number,
    id : number,
    team : number,
    position : string,
    birthYear : number,
    birthMonth : number,
    level : number | null,
    playingTime : number | null,

    // Draft data
    draftPick : number | null,
    preDraftWar : number | null,
    postDraftWar : number | null,
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
        playingTime : null,
        draftPick : null,
        preDraftWar : null,
        postDraftWar : null,
    }
    return p
}

function createMLBHitter(obj : JsonObject)
{
    const p : Player = {
        name : getJsonString(obj, "firstName") + " " + getJsonString(obj, "lastName"),
        war : getJsonNumber(obj, "war"),
        id : getJsonNumber(obj, "mlbId"),
        team : getJsonNumber(obj, "teamId"),
        position : getJsonString(obj, "position"),
        birthYear : getJsonNumber(obj, "birthYear"),
        birthMonth : getJsonNumber(obj, "birthMonth"),
        level : null,
        playingTime : getJsonNumber(obj, "pa"),
        draftPick : null,
        preDraftWar : null,
        postDraftWar : null,
    }
    return p
}

function createMLBStarter(obj : JsonObject)
{
    const p : Player = {
        name : getJsonString(obj, "firstName") + " " + getJsonString(obj, "lastName"),
        war : getJsonNumber(obj, "spWar"),
        id : getJsonNumber(obj, "mlbId"),
        team : getJsonNumber(obj, "teamId"),
        position : "SP",
        birthYear : getJsonNumber(obj, "birthYear"),
        birthMonth : getJsonNumber(obj, "birthMonth"),
        level : null,
        playingTime : getJsonNumber(obj, "spIP"),
        draftPick : null,
        preDraftWar : null,
        postDraftWar : null,
    }
    return p
}

function createMLBReliever(obj : JsonObject)
{
    const p : Player = {
        name : getJsonString(obj, "firstName") + " " + getJsonString(obj, "lastName"),
        war : getJsonNumber(obj, "rpWar"),
        id : getJsonNumber(obj, "mlbId"),
        team : getJsonNumber(obj, "teamId"),
        position : "RP",
        birthYear : getJsonNumber(obj, "birthYear"),
        birthMonth : getJsonNumber(obj, "birthMonth"),
        level : null,
        playingTime : getJsonNumber(obj, "rpIP"),
        draftPick : null,
        preDraftWar : null,
        postDraftWar : null,
    }
    return p
}

function createDraftProspect(obj : JsonObject)
{
    const p : Player = {
        name : getJsonString(obj, "Name"),
        war : 0,
        id : getJsonNumber(obj, "mlbId"),
        team : 0, // TODO
        position : getJsonString(obj, "Position"),
        birthYear : 0, // TODO
        birthMonth : 0, // TODO
        level : null,
        playingTime : null,
        draftPick : getJsonNumberNullable(obj, "draftPick"),
        preDraftWar : getJsonNumberNullable(obj, "warPre"),
        postDraftWar : getJsonNumberNullable(obj, "warPost"),
    }
    return p
}

let __current_rank = 1
function createPlayerElement(player : Player, year : number, month : number, modelId : number) : HTMLTableRowElement
{
    const el = document.createElement('tr') as HTMLTableRowElement
    el.classList.add('rankings_item')

    const teamAbbr : string = player.team == 0 ? "" : getParentAbbr(player.team)
    let ageInYears = year - player.birthYear
    if (month < player.birthMonth)
        ageInYears--

    const levelString = player.level !== null ? `<td class='c_lvl'>${level_map[player.level]}</td>` : ""
    const ptString = player.playingTime !== null ? `<td class='c_pt'>${player.playingTime.toFixed(0)}</td>` : ""
    el.innerHTML = `
            <td>${__current_rank}</td>
            <td class='c_name'><a href='./player?id=${player.id}'>${player.name}</a></td>
            <td class='c_team'><a href='./teams?id=${player.team}&year=${year}&month=${month}'>${teamAbbr}</a></td>
            <td class='c_value'>${formatModelString(player.war)}</td>
            ${levelString}
            ${ptString}
            <td class='c_pos'>${player.position}</td>
            <td class='c_age'>${ageInYears}</td>
        `

    __current_rank += 1
    return el
}

function createDraftPlayerElement(player : Player, year : number, month : number) : HTMLTableRowElement
{
    const el = document.createElement('tr') as HTMLTableRowElement
    el.classList.add('rankings_item')

    const teamAbbr : string = player.team == 0 ? "" : getParentAbbr(player.team)
    let ageInYears = year - player.birthYear
    if (month < player.birthMonth)
        ageInYears--

    const pre_war = player.preDraftWar === null ? '---' : player.preDraftWar.toFixed(1)
    const post_war = player.postDraftWar === null ? '---' : player.postDraftWar.toFixed(1)
    const draft_pick = player.draftPick === null ? '---' : player.draftPick
    
    const player_href = player.draftPick === null ? '' : `href='./player?id=${player.id}'`

    el.innerHTML = `
            <td>${__current_rank}</td>
            <td class='c_name'><a ${player_href}>${player.name}</a></td>
            <td class='c_pre'>${pre_war}</td>
            <td class='c_post'>${post_war}</td>
            <td class='c_pick'>${draft_pick}</td>
            <td class='c_pos'>${player.position}</td>
            <td class='c_age'>${ageInYears}</td>
        `

    __current_rank += 1
    return el
}

enum PlayerLoaderType {
    Prospect,
    MLBHitter = 1,
    MLBStarter = 2,
    MLBReliever = 3,

    DraftRank = 4,
    DraftResult = 5,
}

type PlayerLoaderArgs = {
    teamId : number | null,
    period : number,
    year : number,
    month : number,
    model : number,
    type : PlayerLoaderType
}

class PlayerLoader
{
    private index : number
    private exhaustedElements : boolean = false
    private year : number
    private month : number
    private model : number
    private period : number
    private teamId : number | null
    private type : PlayerLoaderType

    constructor(args : PlayerLoaderArgs)
    {
        this.index = 0
        this.year = args.year
        this.month = args.month
        this.teamId = args.teamId
        this.model = args.model
        this.type = args.type
        this.period = args.period
    }

    async getElements(num_elements : number) : Promise<HTMLTableRowElement[]>
    {
        if (this.exhaustedElements)
            return []

        const endRank = this.index + num_elements
        let response : Response
        if (this.type === PlayerLoaderType.Prospect)
        {
            response = this.teamId !== null ?
            await fetch(`/rankingsRequest?year=${this.year}&month=${this.month}&startRank=${this.index + 1}&endRank=${endRank}&teamId=${this.teamId}&model=${this.model}`) : 
            await fetch(`/rankingsRequest?year=${this.year}&month=${this.month}&startRank=${this.index + 1}&endRank=${endRank}&model=${this.model}`)
        } else if (this.type === PlayerLoaderType.MLBHitter || this.type == PlayerLoaderType.MLBStarter || this.type == PlayerLoaderType.MLBReliever)
        {
            const typeInt : number = this.type
            response = this.teamId !== null ? 
            await fetch(`/mlbRank?year=${this.year}&month=${this.month}&startRank=${this.index + 1}&endRank=${endRank}&teamId=${this.teamId}&model=${this.model}&period=${this.period}&reqType=${typeInt}`) : 
            await fetch(`/mlbRank?year=${this.year}&month=${this.month}&startRank=${this.index + 1}&endRank=${endRank}&model=${this.model}&period=${this.period}&reqType=${typeInt}`)
        } else if (this.type == PlayerLoaderType.DraftRank || this.type == PlayerLoaderType.DraftResult)
        {
            const typeInt : number = this.type
            response = this.teamId !== null ?
                await fetch(`/draft_rank?year=${this.year}&startRank=${this.index + 1}&endRank=${endRank}&teamId=${this.teamId}&model=${this.model}&reqType=${typeInt}`) :
                await fetch(`/draft_rank?year=${this.year}&startRank=${this.index + 1}&endRank=${endRank}&teamId=${this.teamId}&model=${this.model}&reqType=${typeInt}`)
        } else {
            throw new Error("No type in PlayerLoader")
        }

        const players = await response.json() as JsonArray

        console.log(players)

        this.exhaustedElements = (players.length != num_elements)
        this.index += players.length

        if (this.type === PlayerLoaderType.Prospect)
            return players.map(f => {
                return createPlayerElement(createPlayer(f as JsonObject), this.year, this.month, this.model)
            })
        else if (this.type === PlayerLoaderType.MLBHitter)
            return players.map(f => {
                return createPlayerElement(createMLBHitter(f as JsonObject), this.year, this.month, this.model)
            })
        else if (this.type === PlayerLoaderType.MLBStarter)
            return players.map(f => {
                return createPlayerElement(createMLBStarter(f as JsonObject), this.year, this.month, this.model)
            })
        else if (this.type === PlayerLoaderType.MLBReliever)
            return players.map(f => {
                return createPlayerElement(createMLBReliever(f as JsonObject), this.year, this.month, this.model)
            })
        else if (this.type === PlayerLoaderType.DraftRank || this.type === PlayerLoaderType.DraftResult)
            return players.map(f => {
                return createDraftPlayerElement(createDraftProspect(f as JsonObject), this.year, this.month)
            })
        else
            throw Error("Unprogrammed PlayerLoaderType")
    }
}

let playerLoader : PlayerLoader
function setupRankings(args : PlayerLoaderArgs, num_elements : number)
{
    playerLoader = new PlayerLoader(args)

    // @ts-ignore
    const teamString : string = args.teamId !== null ? org_map["parents"][args.teamId]["name"] + " " : ""
    let typeString : string = "INVALID"
    switch(args.type)
    {
        case PlayerLoaderType.Prospect:
            typeString = "Prospect"
            break
        case PlayerLoaderType.MLBHitter:
            typeString = "MLB HITTER"
            break
        case PlayerLoaderType.MLBStarter:
            typeString = "MLB Starter"
            break
        case PlayerLoaderType.MLBReliever:
            typeString = "MLB Reliever"
            break
        case PlayerLoaderType.DraftRank:
        case PlayerLoaderType.DraftResult:
            typeString = "Draft Prospect"
            break
    }

    // Intentionally leave no space between teamString and month, teamString will have the space if it isn't blank
    rankings_header.innerText = `${typeString} Rankings for ${teamString}${MONTH_CODES[month]} ${year}`

    rankings_load.addEventListener('click', async (event) => {
        const elements = await playerLoader.getElements(num_elements)
        for (var el of elements)
        {
            rankings_table_body.appendChild(el)
        }
    })
    
    let headerString : string
    let valueString : string
    let levelString : string = ""
    let ptString : string = ""
    
    // Setup visibility for rankings table
    if (args.type === PlayerLoaderType.Prospect)
    {
        headerString = "<th>Team</th><th>WAR</th><th>Level</th>"
    } else 
    {
        if (args.type === PlayerLoaderType.MLBHitter)
        {
            headerString = "<th>Team</th><th>WAR / 600 PA</th><th>PA</th>"
        } 
        else if (args.type === PlayerLoaderType.MLBStarter)
        {
            headerString = "<th>Team</th><th>WAR / 150 IP</th><th>PA</th>"
        } 
        else if (args.type === PlayerLoaderType.MLBReliever)
        {
            headerString = "<th>Team</th><th>WAR / 50 IP</th><th>PA</th>"
        }
        else if (args.type === PlayerLoaderType.DraftRank || args.type === PlayerLoaderType.DraftResult)
        {
            headerString = "<th>College Model WAR</th><th>Pro Model War</th><th>Draft Pick</th>"
        }
        else {
            throw new Error("Invalid args.type in PlayerLoader")
        }
    }

    rankings_table_head.innerHTML = `
        <tr>
            <th></th>
            <th>Name</th>
            ${headerString}
            <th>Position</th>
            <th>Age</th>
        <tr>
    `

    rankings_load.dispatchEvent(new Event('click'))
}