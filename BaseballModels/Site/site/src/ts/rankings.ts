let month : number
let year : number
let modelId : number
async function main()
{
    const datesJsonPromise = retrieveJson('../../assets/dates.json.gz')
    
    const player_search_data = retrieveJson('../../assets/player_search.json.gz')
    const datesJson = await datesJsonPromise

    endYear = datesJson["endYear"] as number
    endMonth = datesJson["endMonth"] as number
    
    month = getQueryParamBackup("month", endMonth)
    year = getQueryParamBackup("year", endYear)
    const modelId = getQueryParamBackup("model", 1)

    setupSelector({
        month : month,
        year : year,
        modelId : modelId,
        endYear : endYear,
        endMonth : endMonth,
        startYear : datesJson["startYear"] as number,
        startTeam : null,
        level : null
    })
    org_map = await retrieveJson("../../assets/map.json.gz")
    
    setupRankings({
        month : month,
        year : year,
        model : modelId,
        teamId : null,
        period : 0,
        type: PlayerLoaderType.Prospect
    }, 100)
    searchBar = new SearchBar(await player_search_data)

    rankings_button.addEventListener('click', (event) => {
        const mnth = month_select.value
        const yr = year_select.value
        const model = model_select.value
        window.location.href = `./rankings?year=${yr}&month=${mnth}&model=${model}`
    })

    getElementByIdStrict('nav_rankings').classList.add('selected')
    
    document.title = `${MONTH_CODES[month]} ${year} Rankings`
}

main()