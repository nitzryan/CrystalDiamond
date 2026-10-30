let startYear : number
let rangeEndYear : number
let modelId : number
let month : number = 8
let draftTable : DraftLoaderTable | null = null
let teamDraftTable : TeamDraftOverviewTable | null = null
let teamDraftPlayerTable : TeamDraftPlayerTable | null = null
let sectionSelector : SectionSelector | null = null

async function main()
{
    await assetLoader.ready;

    const dates = await assetLoader.dates();
    endYear = dates.draftEndYear;

    const singleYear = getQueryParamNullable("year")
    startYear = getQueryParamNullable("startYear") ?? singleYear ?? endYear
    rangeEndYear = getQueryParamNullable("endYear") ?? singleYear ?? endYear
    modelId = getQueryParamBackup("model", 1)
    if (startYear > rangeEndYear)
        throw new Error(`Invalid year range ${startYear}-${rangeEndYear}`)

    setupSelector({
        month : month,
        year : startYear,
        rangeEndYear : rangeEndYear,
        modelId : modelId,
        endYear : endYear,
        endMonth : month,
        startYear : dates.startYear,
        startTeam : null,
        level : null,
        splitSeasons : startYear !== rangeEndYear
    })

    draftTable = new DraftLoaderTable(startYear, rangeEndYear, month, modelId)
    teamDraftTable = new TeamDraftOverviewTable(startYear, rangeEndYear, modelId)
    teamDraftPlayerTable = new TeamDraftPlayerTable(startYear, rangeEndYear, modelId, getQueryParamBackup("team", 0), draftTable.loaded)

    sectionSelector = new SectionSelector({
        groupId : 'panel_select',
        initial : getQueryParamBackupStr('section', 'players'),
        sections : {
            players : [
                getElementByIdStrict('view_select'),
                getElementByIdStrict('rankings'),
                getElementByIdStrict('page_info_players')
            ],
            team_rank : [
                getElementByIdStrict('team_rank'),
                getElementByIdStrict('page_info_team_rank')
            ],
            team_draft : [
                getElementByIdStrict('team_select'),
                getElementByIdStrict('team_draft'),
                getElementByIdStrict('page_info_team_draft')
            ]
        }
    })

    rankings_button.addEventListener('click', (event) => {
        const model = model_select.value
        
        const multiYearSelected = splitSeasonsCheckbox!.checked
        const start = year_select.value
        const end = multiYearSelected ? 
            year_end_select!.value :
            start
        if (parseInt(start) > parseInt(end))
            return

        window.location.href = `/draft?startYear=${start}&endYear=${end}&model=${model}&view=${draftTable!.view}&split=${draftTable!.split}&section=${sectionSelector!.selected}&team=${teamDraftPlayerTable!.teamId}`
    })

    getElementByIdStrict('nav_draft').classList.add('selected')
    
    document.title = startYear === rangeEndYear ? `${startYear} Draft Rankings` : `${startYear}-${rangeEndYear} Draft Rankings`
}

main()