let month : number
let year : number
let modelId : number
let isWar : number
let playerType : number

const hitpitch_select = getElementByIdStrict('hitpitch_select') as HTMLSelectElement
async function main()
{
    const datesJsonPromise = retrieveJson('../../assets/dates.json.gz')
    
    const player_search_data = retrieveJson('../../assets/player_search.json.gz')
    const datesJson = await datesJsonPromise

    endYear = datesJson["endYear"] as number
    endMonth = datesJson["endMonth"] as number
    
    month = getQueryParamBackup("month", endMonth)
    year = getQueryParamBackup("year", endYear)
    const mdl = getQueryParamBackupStr("model", "1.1").split(".",2).map(f => Number(f))
    modelId = mdl[0]
    isWar = mdl[1]

    playerType = getQueryParamBackup("type", 1)
    let pType : PlayerLoaderType
    if (playerType == 1)
    {
        hitpitch_select.value = "1"
        pType = PlayerLoaderType.MLBHitter
    }
        
    else if (playerType == 2)
    {
        hitpitch_select.value = "2"
        pType = PlayerLoaderType.MLBStarter
    }
    else
    {
        hitpitch_select.value = "3"
        pType = PlayerLoaderType.MLBReliever
    }
        

    setupSelector({
        month : month,
        year : year,
        modelId : modelId,
        isWar : isWar,
        endYear : endYear,
        endMonth : endMonth,
        startYear : datesJson["startYear"] as number,
        startTeam : null
    })
    org_map = await retrieveJson("../../assets/map.json.gz")
    
    setupRankings({
        month : month,
        year : year,
        model : modelId,
        isWar : isWar,
        teamId : null,
        period : 0,
        type: pType
    }, 100)
    searchBar = new SearchBar(await player_search_data)

    rankings_button.addEventListener('click', (event) => {
        const mnth = month_select.value
        const yr = year_select.value
        const model = model_select.value
        const pt = hitpitch_select.value
        window.location.href = `./mlbranks?year=${yr}&month=${mnth}&model=${model}&type=${pt}`
    })

    getElementByIdStrict('nav_mlbranks').classList.add('selected')
    
    document.title = `${MONTH_CODES[month]} ${year} Rankings`
}

main()