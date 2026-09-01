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

const WAR_LABELS = ["<=0", "0-1", "1-5", "5-10", "10-20", "20-30", "30+"]

function piePointGenerator(model : DB_PlayerModel) : Point[]
{
    let points : Point[] = []
    const probs = getModelProbs(model)
    for (let i = 0; i < WAR_LABELS.length; i++)
    {
        points.push({y: probs[i], label:WAR_LABELS[i]})
    }
    return points
}

function lineCallback(index : number, modelId : number)
{
    if (line_graph === null)
        return

    let model : DB_PlayerModel
    if (line_graph.graphIsHitter())
    {
        model = hitterModels[modelId - 1][index]
    } else
    {
        model = pitcherModels[modelId - 1][index]
    }

    if (model !== null)
    {
        if (pie_graph === null)
            throw new Error("Pie Graph null at lineCallback")

        const title_text : string = model.month == 0 ? 
            "Iniitial Outcome Distribution" :
            `${model.month}-${model.year} Outcome Distribution`
        
        pie_graph.updateChart(getModelProbs(model), title_text, WAR_LABELS)
    } else {
        throw new Error("Model was not set for hitter or pitcher")
    }
}

function getDatasets(hitter_war_list : Point[][], hitter_ranks_list : Point[][], pitcher_war_list : Point[][], pitcher_ranks_list : Point[][]) : GraphDataset[]
{
    let datasets : GraphDataset[] = []
    if (hitter_ranks_list.length !== hitter_war_list.length)
        throw new Error("getDatasets: Hitter War vs Ranks length mismatch")

    for (let i = 0; i < hitter_war_list.length; i++)
    {
        if (hitter_war_list[i].length > 0)
        {
            datasets.push({
                points: hitter_war_list[i],
                title: MODEL_STRINGS[i],
                modelId: MODEL_VALUES[i],
                isLog: false,
                isHitter : true
            })
        }
        if (hitter_ranks_list[i].length > 0)
        {
            datasets.push({
                points: hitter_ranks_list[i],
                title: MODEL_STRINGS[i] + " Rank",
                modelId: MODEL_VALUES[i],
                isLog: true,
                isHitter: true
            })
        }
    }

    if (pitcher_ranks_list.length !== pitcher_war_list.length)
        throw new Error("getDatasets: Pitcher War vs Ranks length mismatch")
    for (let i = 0; i < pitcher_war_list.length; i++)
    {
        if (pitcher_war_list[i].length > 0)
        {
            datasets.push({
                points: pitcher_war_list[i],
                title: MODEL_STRINGS[i],
                modelId: MODEL_VALUES[i],
                isLog: false,
                isHitter : false
            })
        }
        if (pitcher_ranks_list[i].length > 0)
        {
            datasets.push({
                points: pitcher_ranks_list[i],
                title: MODEL_STRINGS[i] + " Rank",
                modelId: MODEL_VALUES[i],
                isLog: true,
                isHitter: false
            })
        }
    }

    return datasets
}

function setupSelector(hitter_war_list : Point[][], hitter_ranks_list : Point[][], pitcher_war_list : Point[][], pitcher_ranks_list : Point[][])
{
    for (let i = 0; i < hitter_war_list.length; i++)
    {
        if (hitter_war_list[i].length > 0)
        {
            let opt = document.createElement('option') as HTMLOptionElement
            opt.text = "Hitter " + MODEL_STRINGS[i]
            opt.value = graph_selector.children.length.toString()

            graph_selector.appendChild(opt)
        }
        if (hitter_ranks_list[i].length > 0)
        {
            let opt = document.createElement('option') as HTMLOptionElement
            opt.text = "Hitter " + MODEL_STRINGS[i] + " Rank"
            opt.value = graph_selector.children.length.toString()

            graph_selector.appendChild(opt)
        }
    }
    for (let i = 0; i < pitcher_war_list.length; i++)
    {
        if (pitcher_war_list[i].length > 0)
        {
            let opt = document.createElement('option') as HTMLOptionElement
            opt.text = "Pitcher " + MODEL_STRINGS[i]
            opt.value = graph_selector.children.length.toString()

            graph_selector.appendChild(opt)
        }
        if (pitcher_ranks_list[i].length > 0)
        {
            let opt = document.createElement('option') as HTMLOptionElement
            opt.text = "Pitcher " + MODEL_STRINGS[i] + " Rank"
            opt.value = graph_selector.children.length.toString()

            graph_selector.appendChild(opt)
        }
    }

    graph_selector.value = "0"

    graph_selector.addEventListener('change', () => {
        if (line_graph !== null)
        {
            const idx = parseInt(graph_selector.value)
            line_graph.setDataset(idx)
            hitterTable?.setPredictions(predHitStats.filter(f => f.Model == Math.trunc(idx / 2) + 1))
            pitcherTable?.setPredictions(predPitStats.filter(f => f.Model == Math.trunc(idx / 2) + 1))
            line_graph.fireCallback()
        }
    })
}

function setupModel(hitterModels : DB_PlayerModel[][], pitcherModels : DB_PlayerModel[][]) : void
{
    let war_map = (f: DB_PlayerModel, buckets : number[]) => {
        const probs = getModelProbs(f)
        let war = 0;
        for (let i = 0; i < probs.length; i++)
            war += probs[i] * buckets[i];

        const label : string = f.month == 0 ? 'Initial' : `${f.month}-${f.year}`
        const p : Point = {y: war, label : label}
        return p;
    }

    let rank_map = (f : DB_PlayerModel) => {
        let month = f.month
        let year = f.year
        if (f.rankWar === null)
            throw new Error("No Rank")
        const p : Point = {y: f.rankWar, label: year == 0 ? "Initial" : `${month}-${year}`}
        return p
    }
    
    let hitter_war_points : Point[][] = []
    let pitcher_war_points : Point[][] = []
    
    for (var idx of MODEL_VALUES)
    {
        if (hitterModels.length > 0)
            hitter_war_points.push(hitterModels[idx - 1].map(f => war_map(f, assetLoader.war_buckets_hitter)))
        if (pitcherModels.length > 0)
            pitcher_war_points.push(pitcherModels[idx - 1].map(f => war_map(f, assetLoader.war_buckets_pitcher)))
    }

    let hitter_rank_points : Point[][] = hitterModels.map(m => m.filter(f => f.rankWar !== null).map(rank_map))
    let pitcher_rank_points : Point[][] = pitcherModels.map(m => m.filter(f => f.rankWar !== null).map(rank_map))

    
    const datasets = getDatasets(hitter_war_points, hitter_rank_points, pitcher_war_points, pitcher_rank_points)
    if (datasets.length > 0)
    {
        line_graph = new LineGraph(model_graph, datasets, lineCallback)
        setupSelector(hitter_war_points, hitter_rank_points, pitcher_war_points, pitcher_rank_points)

        const pie_points = person.isHitter ? 
            piePointGenerator(hitterModels[0][hitterModels[0].length - 1]) :
            piePointGenerator(pitcherModels[0][pitcherModels[0].length - 1])
        pie_graph = new PieGraph(model_pie, pie_points, "Outcome Distribution")
    } else {
        line_graph = null
        graph_selector.classList.add('hidden')
        let noProspectData = getElementByIdStrict('noProspectData')
        noProspectData.classList.remove('hidden')
    }
    
}

const model_pie = getElementByIdStrict("projWarPie") as HTMLCanvasElement
const model_graph = getElementByIdStrict("projWarGraph") as HTMLCanvasElement
let graph_selector = getElementByIdStrict('graph_selector') as HTMLSelectElement
let line_graph : LineGraph | null
let pie_graph : PieGraph
let keyControls : KeyControls

let person : DB_Player
let hitterModels : DB_PlayerModel[][]
let pitcherModels : DB_PlayerModel[][]

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
    hitterModels = person.isHitter ? getModels(pd, "hit_models") : []
    pitcherModels = person.isPitcher ? getModels(pd, "pit_models") : []

    setupModel(hitterModels, pitcherModels)
    if (line_graph !== null)
        line_graph.fireCallback()

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
        if (line_graph !== null)
            line_graph.increment_index(x_inc)
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