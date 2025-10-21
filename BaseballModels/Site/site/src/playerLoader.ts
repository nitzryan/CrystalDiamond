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
    level : number | null,
    playingTime : string | null
}

function createPlayer(obj : JsonObject, isWar : number)
{
    const p : Player = {
        name : getJsonString(obj, "firstName") + " " + getJsonString(obj, "lastName"),
        war : isWar === 1 ? getJsonNumber(obj, "war") : getJsonNumber(obj, "value"),
        id : getJsonNumber(obj, "mlbId"),
        team : getJsonNumber(obj, "teamId"),
        position : getJsonString(obj, "position"),
        birthYear : getJsonNumber(obj, "birthYear"),
        birthMonth : getJsonNumber(obj, "birthMonth"),
        level : getJsonNumber(obj, "highestLevel"),
        playingTime : null
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
        playingTime : getJsonNumber(obj, "pa").toFixed(0).toString() + " PA"
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
        playingTime : getJsonNumber(obj, "spIP").toFixed(0).toString() + " IP"
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
        playingTime : getJsonNumber(obj, "rpIP").toFixed(0).toString() + " IP"
    }
    return p
}

function createPlayerElement(player : Player, year : number, month : number, modelId : number, isWar : number) : HTMLLIElement
{
    const el = document.createElement('li') as HTMLLIElement
    const teamAbbr : string = player.team == 0 ? "" : getParentAbbr(player.team)
    let ageInYears = year - player.birthYear
    if (month < player.birthMonth)
        ageInYears--

    const levelString = player.level !== null ? `<div>${level_map[player.level]}</div>` : ""
    const ptString = player.playingTime !== null ? `<div>${player.playingTime}</div>` : ""
    el.innerHTML = 
        `
        <div class='rankings_item'>
            <div class='rankings_row'>
                <div class='rankings_name'><a href='./player?id=${player.id}'>${player.name}</a></div>
                <div class='rankings_rightrow'>
                    <div><a href='./teams?id=${player.team}&year=${year}&month=${month}'>${teamAbbr}</a></div>
                    ${levelString}
                    ${ptString}
                </div>
            </div>
            <div class='rankings_row'>
                <div>${formatModelString(player.war, isWar)}</div>
                <div class='rankings_rightrow'>
                    <div>${player.position}</div>
                    <div>${ageInYears}yrs</div>
                </div>
            </div>
        </div>
        `

    return el
}

enum PlayerLoaderType {
    Prospect,
    MLBHitter = 1,
    MLBStarter = 2,
    MLBReliever = 3
}

type PlayerLoaderArgs = {
    isWar : number,
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
    private isWar : number
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
        this.isWar = args.isWar
        this.type = args.type
        this.period = args.period
    }

    async getElements(num_elements : number) : Promise<HTMLLIElement[]>
    {
        if (this.exhaustedElements)
            return []

        const endRank = this.index + num_elements
        let response : Response
        if (this.type === PlayerLoaderType.Prospect)
        {
            response = this.teamId !== null ?
            await fetch(`/rankingsRequest?year=${this.year}&month=${this.month}&startRank=${this.index + 1}&endRank=${endRank}&teamId=${this.teamId}&model=${this.model}.${this.isWar}`) : 
            await fetch(`/rankingsRequest?year=${this.year}&month=${this.month}&startRank=${this.index + 1}&endRank=${endRank}&model=${this.model}.${this.isWar}`)
        } else if (this.type === PlayerLoaderType.MLBHitter || this.type == PlayerLoaderType.MLBStarter || this.type == PlayerLoaderType.MLBReliever)
        {
            const typeInt : number = this.type
            response = this.teamId !== null ? 
            await fetch(`/mlbRank?year=${this.year}&month=${this.month}&startRank=${this.index + 1}&endRank=${endRank}&teamId=${this.teamId}&model=${this.model}&period=${this.period}&reqType=${typeInt}`) : 
            await fetch(`/mlbRank?year=${this.year}&month=${this.month}&startRank=${this.index + 1}&endRank=${endRank}&model=${this.model}&period=${this.period}&reqType=${typeInt}`)
        } else {
            throw new Error("No type in PlayerLoader")
        }

        const players = await response.json() as JsonArray

        this.exhaustedElements = (players.length != num_elements)
        this.index += players.length

        if (this.type === PlayerLoaderType.Prospect)
            return players.map(f => {
                return createPlayerElement(createPlayer(f as JsonObject, this.isWar), this.year, this.month, this.model, this.isWar)
            })
        else if (this.type === PlayerLoaderType.MLBHitter)
            return players.map(f => {
                return createPlayerElement(createMLBHitter(f as JsonObject), this.year, this.month, this.model, 1)
            })
        else if (this.type === PlayerLoaderType.MLBStarter)
            return players.map(f => {
                return createPlayerElement(createMLBStarter(f as JsonObject), this.year, this.month, this.model, 1)
            })
        else
            return players.map(f => {
                return createPlayerElement(createMLBReliever(f as JsonObject), this.year, this.month, this.model, 1)
            })
    }
}

let playerLoader : PlayerLoader
function setupRankings(args : PlayerLoaderArgs, num_elements : number)
{
    playerLoader = new PlayerLoader(args)

    // @ts-ignore
    const teamString : string = args.teamId !== null ? org_map["parents"][args.teamId]["name"] + " " : ""
    const typeString : string = args.type === PlayerLoaderType.Prospect ?
        "Prospect" : args.type === PlayerLoaderType.MLBHitter ? "MLB Hitter" : 
        args.type === PlayerLoaderType.MLBStarter ? "MLB Starter" : "MLB Reliever"

    // Intentionally leave no space between teamString and month, teamString will have the space if it isn't blank
    rankings_header.innerText = `${typeString} Rankings for ${teamString}${MONTH_CODES[month]} ${year}`

    rankings_load.addEventListener('click', async (event) => {
        const elements = await playerLoader.getElements(num_elements)
        for (var el of elements)
        {
            rankings_list.appendChild(el)
        }
    })

    rankings_load.dispatchEvent(new Event('click'))
}