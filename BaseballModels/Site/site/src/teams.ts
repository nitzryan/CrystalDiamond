let month : number | null = null
let year : number | null = null
let teamId : number | null = null
async function main()
{
    const datesJsonPromise = retrieveJson('../../assets/dates.json.gz')
    try {
        month = getQueryParam('month')
        year = getQueryParam('year')
        teamId = getQueryParam('team')
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
    if (teamId === null)
        teamId = 142

    org_map = await retrieveJson("../../assets/map.json.gz")
    const selector = setupSelector({
        month : month,
        year : year,
        endYear : endYear,
        endMonth : endMonth,
        startYear : datesJson["startYear"] as number,
        startTeam : teamId
    })
    
    setupRankings(month, year, teamId, 30)
    searchBar = new SearchBar(await player_search_data)

    rankings_button.addEventListener('click', (event) => {
        const mnth = month_select.value
        const yr = year_select.value
        const tm = team_select?.value
        
        window.location.href = `./teams?year=${yr}&month=${mnth}&team=${tm}`
    })

    getElementByIdStrict('nav_teams').classList.add('selected')

    document.title = `${getParentAbbr(teamId)} ${MONTH_CODES[month]} ${year} Rankings`
}

main()