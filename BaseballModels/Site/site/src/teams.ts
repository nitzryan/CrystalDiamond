let month : number
let year : number
let modelId : number
let teamId : number | null = null
let isWar : number
let rankings_overall = document.getElementById('rankings_overall') as HTMLButtonElement

async function main()
{
    const datesJsonPromise = retrieveJson('../../assets/dates.json.gz')
    
    
    const player_search_data = retrieveJson('../../assets/player_search.json.gz')
    const datesJson = await datesJsonPromise
    org_map = await retrieveJson("../../assets/map.json.gz")

    endYear = datesJson["endYear"] as number
    endMonth = datesJson["endMonth"] as number
    month = getQueryParamBackup('month', endMonth)
    year = getQueryParamBackup('year', endYear)
    const mdl = getQueryParamBackupStr("model", "1.1").split(".",2).map(f => Number(f))
    modelId = mdl[0]
    isWar = mdl[1]
    teamId = getQueryParamNullable('team')
    
    setupSelector({
        month : month,
        year : year,
        modelId : modelId,
        isWar : isWar,
        endYear : endYear,
        endMonth : endMonth,
        startYear : datesJson["startYear"] as number,
        startTeam : teamId
    })
    if (teamId === null)
    {
        createOverviewPage(datesJson)
    } else 
    {
        createTeamPage(datesJson)
    }

    searchBar = new SearchBar(await player_search_data)
    getElementByIdStrict('nav_teams').classList.add('selected')

    rankings_overall.addEventListener('click', () => {
        const mnth = month_select.value
        const yr = year_select.value
        window.location.href = `./teams?year=${yr}&month=${mnth}`
    })
}

async function createTeamPage(datesJson : JsonObject)
{
    if (teamId === null)
        throw new Error("Null month in createTeamPage")
    
    setupRankings(month, year, modelId, isWar, teamId, 30)

    rankings_button.addEventListener('click', (event) => {
        const mnth = month_select.value
        const yr = year_select.value
        const tm = team_select?.value
        const model = model_select.value
        
        window.location.href = `./teams?year=${yr}&month=${mnth}&team=${tm}&model=${model}`
    })

    document.title = `${getParentAbbr(teamId)} ${MONTH_CODES[month]} ${year} Rankings`
}

function TeamOverviewMap(team : JsonValue) : HTMLLIElement
{
    team = team as JsonObject

    const id = team["teamId"] as number

    const value_string = isWar === 1 ?
        (team["value"] as number).toFixed(1) + " WAR" :
        "$" + (team["value"] as number).toFixed(0) + "M"

    let el = document.createElement('li')
    el.innerHTML = 
        `
        <div class='rankings_item'>
            <div class='rankings_row'>
                <div class='rankings_name'><a href='./teams?team=${id}&year=${year}&month=${month}&model=${modelId}.${isWar}'>${getParentName(id)}</a></div>
                <div class='rankings_rightrow'>
                    <div>Highest Rank: ${team["highestRank"]}</div>
                </div>
            </div>
            <div class='rankings_row'>
                <div>${value_string}</div>
                <div class='rankings_rightrow'>
                    <div>${team["top10"]}</div>
                    <div>${team["top50"]}</div>
                    <div>${team["top100"]}</div>
                    <div>${team["top200"]}</div>
                    <div>${team["top500"]}</div>
                </div>
            </div>
        </div>
        `

    return el
}

async function createOverviewPage(datesJson : JsonObject)
{
    const team_rankings_data = await (await (await fetch(`./teamRanks?year=${year}&month=${month}&model=${modelId}.${isWar}`)).json()) as JsonArray
    let elements = team_rankings_data.map(TeamOverviewMap)
    elements.forEach(f => {
        rankings_list.appendChild(f)
    })
    
    rankings_header.innerText = `Team Rankings for ${MONTH_CODES[month]} ${year}`
    rankings_load.classList.add('hidden')
    team_select?.classList.add('hidden')
    rankings_overall.classList.add('hidden')

    rankings_button.addEventListener('click', (event) => {
        const mnth = month_select.value
        const yr = year_select.value
        const model = model_select.value
        
        window.location.href = `./teams?year=${yr}&month=${mnth}&model=${model}`
    })

    document.title = `${MONTH_CODES[month]} ${year} Team Rankings`
}

main()