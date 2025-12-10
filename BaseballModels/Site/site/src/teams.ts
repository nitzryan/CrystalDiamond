let month : number
let year : number
let modelId : number
let teamId : number | null = null
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
    teamId = getQueryParamNullable('team')
    
    setupSelector({
        month : month,
        year : year,
        modelId : modelId,
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
    
    setupRankings({
        month : month,
        year : year,
        model : modelId,
        teamId : teamId,
        period : 0,
        type : PlayerLoaderType.Prospect
        }, 30)

    rankings_button.addEventListener('click', (event) => {
        const mnth = month_select.value
        const yr = year_select.value
        const tm = team_select?.value
        const model = model_select.value
        
        window.location.href = `./teams?year=${yr}&month=${mnth}&team=${tm}&model=${model}`
    })

    document.title = `${getParentAbbr(teamId)} ${MONTH_CODES[month]} ${year} Rankings`
}

function TeamOverviewMap(team : JsonValue) : HTMLTableRowElement
{
    team = team as JsonObject
    const id = team["teamId"] as number
    const value_string = (team["war"] as number).toFixed(1) 

    let el = document.createElement('tr')
    el.classList.add('rankings_item')
    el.innerHTML = 
        `
        <td class='c_rank'>${__current_rank}</td>
        <td class='c_name'><a href='./teams?team=${id}&year=${year}&month=${month}&model=${modelId}'>${getParentName(id)}</a></td>
        <td class='c_value'>${team["highestRank"]}</td>
        <td class='c_value'>${value_string}</td>
        <td class='c_value'>${team["top10"]}</td>
        <td class='c_value'>${team["top50"]}</td>
        <td class='c_value'>${team["top100"]}</td>
        <td class='c_value'>${team["top200"]}</td>
        <td class='c_value'>${team["top500"]}</td>
        `

    __current_rank += 1

    return el
}

async function createOverviewPage(datesJson : JsonObject)
{
    const team_rankings_data = await (await (await fetch(`./teamRanks?year=${year}&month=${month}&model=${modelId}`)).json()) as JsonArray
    let elements = team_rankings_data.map(TeamOverviewMap)
    elements.forEach(f => {
        rankings_table_body.appendChild(f)
    })
    
    let valueString = "WAR"
    rankings_table_head.innerHTML = `
        <tr>
            <th></th>
            <th>Team</th>
            <th>Highest Ranked</th>
            <th>${valueString}</th>
            <th>Top 10</th>
            <th>Top 50</th>
            <th>Top 100</th>
            <th>Top 200</th>
            <th>Top 500</th>
        <tr>
        `

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