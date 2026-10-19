type HitterStatLine = DB_HitterYearStats | DB_HitterMonthStats
type PitcherStatLine = DB_PitcherYearStats | DB_PitcherMonthStats

function getStatMonth(stat : HitterStatLine | PitcherStatLine) : number | null
{
    if (stat instanceof DB_HitterMonthStats || stat instanceof DB_PitcherMonthStats)
        return stat.month
    return null
}

function getModelProbs(model : DB_PlayerModel) : number[]
{
    return model.probsWar.split(',').map(Number)
}

function getHitterStats(hitterObject : JsonObject) : HitterStatLine[]
{
    let stats : HitterStatLine[] = getJsonArray(hitterObject, "hit_stats")
        .map(f => new DB_HitterYearStats(f as JsonObject))

    stats = stats.concat(getJsonArray(hitterObject, "hit_month_stats")
        .map(f => new DB_HitterMonthStats(f as JsonObject)))

    stats.sort((a,b) => {
        if (a.year < b.year)
            return -1
        if (a.year > b.year)
            return 1

        const aMonth = getStatMonth(a)
        const bMonth = getStatMonth(b)
        if (aMonth === null && bMonth === null)
            return b.levelId - a.levelId
        if (aMonth === null)
            return -1
        if (bMonth === null)
            return 1
        if (aMonth === bMonth)
            return b.levelId - a.levelId
        return aMonth - bMonth
    })

    return stats
}

function getPitcherStats(pitcherObject : JsonObject) : PitcherStatLine[]
{
    let stats : PitcherStatLine[] = getJsonArray(pitcherObject, "pit_stats")
        .map(f => new DB_PitcherYearStats(f as JsonObject))

    stats = stats.concat(getJsonArray(pitcherObject, "pit_month_stats")
        .map(f => new DB_PitcherMonthStats(f as JsonObject)))

    stats.sort((a,b) => {
        if (a.year < b.year)
            return -1
        if (a.year > b.year)
            return 1

        const aMonth = getStatMonth(a)
        const bMonth = getStatMonth(b)
        if (aMonth === null && bMonth === null)
            return b.levelId - a.levelId
        if (aMonth === null)
            return -1
        if (bMonth === null)
            return 1
        if (aMonth === bMonth)
            return b.levelId - a.levelId
        return aMonth - bMonth
    })

    return stats
}

function getModels(obj : JsonObject, name : string) : DB_PlayerModel[][]
{
    const models : DB_PlayerModel[] = getJsonArray(obj, name)
        .map(f => new DB_PlayerModel(f as JsonObject))

    let models_list : DB_PlayerModel[][] = []
    MODEL_VALUES.forEach(f => {
        models_list.push(models.filter(g => g.modelId === f))
    })

    return models_list;
}

function getPostProspectModels(obj : JsonObject, name : string) : DB_PostProspectPlayerModel[][]
{
    const models : DB_PostProspectPlayerModel[] = getJsonArray(obj, name)
        .map(f => new DB_PostProspectPlayerModel(f as JsonObject))

    let models_list : DB_PostProspectPlayerModel[][] = []
    MODEL_VALUES.forEach(f => {
        models_list.push(models.filter(g => g.modelId === f))
    })

    return models_list;
}

const PROSPECT_WAR_LABELS = ["<=0", "0-1", "1-5", "5-10", "10-20", "20-30", "30+"]
const MLB_HIT_WAR_LABELS = ["<0", "0", "0-1", "1-2", "2-3", "3-4", "4-6", "6+"]
const MLB_SP_WAR_LABELS = ["<0", "0", "0-1", "1-2", "2-3", "3-4", "4-6", "6+"]
const MLB_RP_WAR_LABELS = ["<0", "0", "0-0.3", "0.3-0.6", "0.6-1", "1-1.5", "1.5-2", "2+"]

// Show whichever model has the more recent timestep, preferring prospect on a tie
function getDefaultModelSection() : string
{
    const prospect = prospectSources[0][prospectSources[0].length - 1]
    const mlb = mlbSources[0].timesteps[mlbSources[0].timesteps.length - 1]
    const prospectDate = prospect.year * 100 + prospect.month
    const mlbDate = mlb.year * 100 + mlb.month
    return prospectDate >= mlbDate ? 'prospect' : 'mlb'
}

function setupModelSections(hasProspect : boolean, hasMlb : boolean) : void
{
    const prospectSection = getElementByIdStrict('prospect_section')
    const mlbSection = getElementByIdStrict('mlb_section')

    if (hasProspect && hasMlb)
    {
        modelSections = new SectionSelector({
            groupId: 'model_toggle',
            initial: getDefaultModelSection(),
            sections: { prospect: [prospectSection], mlb: [mlbSection] }
        })
        return
    }

    // Set single button visible and selected, other invisible
    const prospect_toggle_button = getElementByIdStrict('prospect_toggle')
    const mlb_toggle_button = getElementByIdStrict('mlb_toggle')
    if (hasProspect)
    {
        prospect_toggle_button.classList.add('selected')
        mlb_toggle_button.classList.add('hidden')
    }
    else
    {
        mlb_toggle_button.classList.add('selected')
        prospect_toggle_button.classList.add('hidden')
        mlb_toggle_button.style.setProperty("border-left", "1px solid var(--text-high)")
    }
    
    prospectSection.classList.toggle('hidden', !hasProspect)
    mlbSection.classList.toggle('hidden', !hasMlb)
    if (!hasProspect && !hasMlb)
        getElementByIdStrict('noModelData').classList.remove('hidden')
}

function getVisibleLineGraph() : LineGraph | null
{
    if (modelSections === null)
        return null
    return modelSections.selected === 'mlb' ? mlb_line_graph : prospect_line_graph
}

const prospect_pie_canvas = getElementByIdStrict("prospect_pie_canvas") as HTMLCanvasElement
const prospect_line_canvas = getElementByIdStrict("prospect_line_canvas") as HTMLCanvasElement
let prospect_graph_selector = getElementByIdStrict('prospect_graph_selector') as HTMLSelectElement
let prospect_line_graph : LineGraph | null = null
let prospect_pie_graph : PieGraph
const prospect_bar_canvas = getElementByIdStrict("prospect_bar_canvas") as HTMLCanvasElement
let prospect_bar_graph : WarProgressGraph | null = null
let keyControls : KeyControls

const mlb_pie_canvas = getElementByIdStrict("mlb_pie_canvas") as HTMLCanvasElement
const mlb_graph = getElementByIdStrict("mlb_line_canvas") as HTMLCanvasElement
let mlb_graph_selector = getElementByIdStrict('mlb_graph_selector') as HTMLSelectElement
let mlb_year_selector = getElementByIdStrict('mlb_year_selector') as HTMLSelectElement
let mlb_line_graph : LineGraph | null = null
let mlb_pie_graph : PieGraph | null = null

let modelSections : SectionSelector | null = null
const MLB_PIE_COLORS = ['#d43d51', '#dd7077', '#e19d9e', '#dfc8c8', '#c3d1cc', '#8fb8ab', '#55a08b', '#00876c']

let person : DB_Player
let prospectSources : DB_PlayerModel[][] = []
let postProspectSources : DB_PostProspectPlayerModel[][] = []
let postProspectBarScales : any[] = []

let hitterTable : HitterStatsTable | null = null
let pitcherTable : PitcherStatsTable | null = null

let predHitStats : DB_Prediction_HitterStats[] = []
let predPitStats : DB_Prediction_PitcherStats[] = []

async function main()
{
    const id = getQueryParam("id")
    var player_data = fetch(`/player/${id}`)

    await assetLoader.ready
    const pd = await (await player_data).json() as JsonObject
    person = new DB_Player(pd)

    // Include stats
    let hitterStats = person.isHitter ? getHitterStats(pd) : []
    let pitcherStats = person.isPitcher ? getPitcherStats(pd) : []

    if (hitterStats.length > 0)
        hitterTable = new HitterStatsTable(hitterStats)
    if (pitcherStats.length > 0)
        pitcherTable = new PitcherStatsTable(pitcherStats)
    
    // Get Models
    const hitterModels = person.isHitter ? getModels(pd, "hit_models") : []
    const pitcherModels = person.isPitcher ? getModels(pd, "pit_models") : []

    const hitterPostModels = person.isHitter ? getPostProspectModels(pd, "hit_post_models") : []
    const pitcherPostModels = person.isPitcher ? getPostProspectModels(pd, "pit_post_models") : []

    const hasProspect = setupProspectModel(hitterModels, pitcherModels, hitterPostModels, pitcherPostModels)

    const hitterMlbWar = person.isHitter ? getHitterMlbWar(pd) : []
    const pitcherMlbWar = person.isPitcher ? getPitcherMlbWar(pd) : { starter: [], reliever: [] }
    const hasMlb = setupMlbModel(hitterMlbWar, pitcherMlbWar.starter, pitcherMlbWar.reliever)

    setupModelSections(hasProspect, hasMlb)

    // Set person
    updateElementText("player_name", `${person.firstName} ${person.lastName}`)
    updateElementText("player_position", person.position)
    updateElementText("player_status", person.status)

    // Hide training warning for players not in training data
    if (!person.inTraining)
    {
        const trainingWarning = getElementByIdStrict('playerInTraining')
        trainingWarning.classList.add('hidden')
    }

    if (person.orgId !== null && person.orgId !== 0)
    {
        const player_team = getElementByIdStrict("player_team") as HTMLLinkElement
        player_team.innerText = getParentName(person.orgId)
        player_team.href = `teams?team=${person.orgId}`
    } else 
    {
        updateElementText("player_team", "Free Agent")
    }

    const birthDate = new Date(person.birthYear, person.birthMonth, person.birthDate)
    const age = getDateDelta(birthDate, new Date())
    updateElementText("player_age", `${age[0]}y, ${age[1]}m, ${age[2]}d`)
    if (person.draftPick !== null)
    {
        if (person.draftRound === null || person.draftBonus === null)
            throw new Error(`Player ${person.mlbId} has draftPick=${person.draftPick} but draftRound=${person.draftRound}, draftBonus=${person.draftBonus}; both should be non-null`)

        const round : string = isNaN(parseFloat(person.draftRound)) ? person.draftRound : "Round " + person.draftRound
        updateElementText("player_draft", `${person.startYear} Draft, ${round} (${getOrdinalNumber(person.draftPick)} Overall)\n$${person.draftBonus.toLocaleString()} Bonus`)
    }
    document.title = person.firstName + " " + person.lastName

    keyControls = new KeyControls(document, (x_inc) => {
        const graph = getVisibleLineGraph()
        if (graph !== null)
            graph.increment_index(x_inc)
    })

    // Update stats date
    const dates = await assetLoader.dates();
    const endYear = dates.endYear;
    const endMonth = dates.endMonth;
    let hitter_title_element = getElementByIdStrict('hitter_stats_title')
    let pitcher_title_element = getElementByIdStrict('pitcher_stats_title')
    hitter_title_element.textContent = `Hitter Stats through ${MONTH_CODES[endMonth]} ${endYear}`
    pitcher_title_element.textContent = `Pitcher Stats through ${MONTH_CODES[endMonth]} ${endYear}`

    // Load predictions
    // TODO : Add back in once model doesn't break this
    // const hitterStatsPredictionPromise = fetch(`/prediction_hitter?id=${id}&year=${endYear}&month=${endMonth}`)
    // const pitcherStatsPredictionPromise = fetch(`/prediction_pitcher?id=${id}&year=${endYear}&month=${endMonth}`)

    // const hitterStatsPredictions = await (await hitterStatsPredictionPromise).json() as JsonArray
    // const pitcherStatsPredictions = await (await pitcherStatsPredictionPromise).json() as JsonArray
    // predHitStats = hitterStatsPredictions.map(f => new DB_Prediction_HitterStats(f as JsonObject)).sort(f => f.LevelId)
    // predPitStats = pitcherStatsPredictions.map(f => new DB_Prediction_PitcherStats(f as JsonObject)).sort(f => f.levelId)
    // hitterTable?.setPredictions(predHitStats.filter(f => f.Model === 1))
    // pitcherTable?.setPredictions(predPitStats.filter(f => f.Model === 1))
}

main()