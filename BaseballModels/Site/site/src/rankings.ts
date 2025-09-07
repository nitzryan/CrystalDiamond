let searchBar : SearchBar | null = null

let month : number | null = null
let year : number | null = null
async function main()
{
    const datesJsonPromise = retrieveJson('../../assets/ranking/dates.json.gz')
    try {
        month = getQueryParam('month')
        year = getQueryParam('year')
    } catch (error) {}
    
    const player_search_data = retrieveJson('../../assets/player_search.json.gz')
    const datesJson = await datesJsonPromise

    endYear = datesJson["endYear"] as number
    endMonth = datesJson["endMonth"] as number
    if (month === null || year === null)
    {
        month = endMonth
        year = endYear
    }

    const selector = setupSelector({
        month : month,
        year : year,
        endYear : endYear,
        endMonth : endMonth,
        startYear : datesJson["startYear"] as number,
        startTeam : null
    })
    org_map = await retrieveJson("../../assets/map.json.gz")
    
    setupRankings(month, year, null, 100)
    searchBar = new SearchBar(await player_search_data)

    await selector
    rankings_button.addEventListener('click', (event) => {
        const mnth = month_select.value
        const yr = year_select.value
        
        window.location.href = `./rankings?year=${yr}&month=${mnth}`
    })

    getElementByIdStrict('nav_rankings').classList.add('selected')
    
    document.title = `${MONTH_CODES[month]} ${year} Rankings`
}

main()