let month : number
let year : number
let modelId : number
let teamId : number | null

async function main()
{
    const datesJsonPromise = retrieveJson('../../assets/dates.json.gz')
    
    const player_search_data = retrieveJson('../../assets/player_search.json.gz')
    const datesJson = await datesJsonPromise

    endYear = datesJson["endYear"] as number
    endMonth = datesJson["endMonth"] as number
    
    month = getQueryParamBackup("month", endMonth)
    year = getQueryParamBackup("year", endYear)
    teamId = getQueryParamNullable('team')
    const modelId = getQueryParamBackup("model", 1)

    org_map = await retrieveJson("../../assets/map.json.gz")
    setupSelector({
        month : month,
        year : year,
        modelId : modelId,
        endYear : endYear,
        endMonth : endMonth,
        startYear : datesJson["startYear"] as number,
        startTeam : teamId,
        level : null
    })
    
    setupRankings({
        month : month,
        year : year,
        model : modelId,
        teamId : teamId,
        period : 0,
        type: PlayerLoaderType.Prospect
    }, 100)
    searchBar = new SearchBar(await player_search_data)

    rankings_button.addEventListener('click', (event) => {
        const mnth = month_select.value
        const yr = year_select.value
        const model = model_select.value
        const team = team_select?.value

        if (team === "0")
            window.location.href = `./rankings?year=${yr}&month=${mnth}&model=${model}`
        else
            window.location.href = `./rankings?year=${yr}&month=${mnth}&model=${model}&team=${team}`
    })

    getElementByIdStrict('nav_rankings').classList.add('selected')
    
    document.title = `${MONTH_CODES[month]} ${year} Rankings`
}

main()