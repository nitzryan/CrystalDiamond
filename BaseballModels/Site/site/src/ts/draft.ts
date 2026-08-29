let year : number
let modelId : number
let month : number = 8
let draftTable : DraftLoaderTable | null = null

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

    rankings_button.addEventListener('click', (event) => {
        const yr = year_select.value
        const model = model_select.value

        window.location.href = `./draft?year=${yr}&model=${model}&view=${draftTable!.view}&split=${draftTable!.split}`
    })

    getElementByIdStrict('nav_draft').classList.add('selected')
    
    document.title = `${year} Draft Rankings`
}

main()