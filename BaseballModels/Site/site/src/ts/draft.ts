let year : number
let modelId : number
let month : number = 8
const draft_select : HTMLSelectElement = getElementByIdStrict('draft_type_select') as HTMLSelectElement

async function main()
{
    const datesJsonPromise = retrieveJson('../../assets/dates.json.gz')
    const player_search_data = retrieveJson('../../assets/player_search.json.gz')
    const datesJson = await datesJsonPromise

    endYear = datesJson["draftEndYear"] as number
    year = getQueryParamBackup("year", endYear)
    modelId = getQueryParamBackup("model", 1)
    const draftType = getQueryParamBackup("type", 4)

    org_map = await retrieveJson("../../assets/map.json.gz")
    setupSelector({
        month : month,
        year : year,
        modelId : modelId,
        endYear : endYear,
        endMonth : month,
        startYear : datesJson["startYear"] as number,
        startTeam : null,
        level : null
    })

    setupRankings({
        month : month,
        year : year,
        model : modelId,
        teamId : null,
        period : 0,
        type : draftType as PlayerLoaderType
    }, 100)

    searchBar = new SearchBar(await player_search_data)

    rankings_button.addEventListener('click', (event) => {
        const yr = year_select.value
        const model = model_select.value
        const draftType = draft_select.value

        window.location.href = `./draft?year=${yr}&model=${model}&type=${draftType}`
    })

    getElementByIdStrict('nav_draft').classList.add('selected')
    
    document.title = `${year} Draft Rankings`
}

main()