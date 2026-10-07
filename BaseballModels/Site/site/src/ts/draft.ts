let year : number
let modelId : number
let month : number = 8
let draftTable : DraftLoaderTable | null = null
let teamDraftTable : TeamDraftOverviewTable | null = null
let teamDraftPlayerTable : TeamDraftPlayerTable | null = null
let sectionSelector : SectionSelector | null = null

function showTeamDraft(teamId : number)
{
    sectionSelector!.select('team_draft')
    teamDraftPlayerTable!.setTeam(teamId)
}

async function main()
{
    await assetLoader.ready;

    const dates = await assetLoader.dates();
    endYear = dates.draftEndYear;
    year = getQueryParamBackup("year", endYear)
    modelId = getQueryParamBackup("model", 1)

    setupSelector({
        month : month,
        year : year,
        modelId : modelId,
        endYear : endYear,
        endMonth : month,
        startYear : dates.startYear,
        startTeam : null,
        level : null
    })

    draftTable = new DraftLoaderTable(year, month, modelId)
    teamDraftTable = new TeamDraftOverviewTable(year, modelId, showTeamDraft)
    teamDraftPlayerTable = new TeamDraftPlayerTable(year, modelId, getQueryParamBackup("team", 0), draftTable.loaded)

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
        const yr = year_select.value
        const model = model_select.value

        window.location.href = `./draft?year=${yr}&model=${model}&view=${draftTable!.view}&split=${draftTable!.split}&section=${sectionSelector!.selected}&team=${teamDraftPlayerTable!.teamId}`
    })

    getElementByIdStrict('nav_draft').classList.add('selected')
    
    document.title = `${year} Draft Rankings`
}

main()