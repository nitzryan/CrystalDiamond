function getProspectDateLabel(year : number, month : number) : string
{
    return month == 0 ? 'Initial' : `${month}-${year}`
}

function getProspectWarPoint(f : DB_PlayerModel, buckets : number[]) : Point
{
    const probs = getModelProbs(f)
    let war = 0;
    for (let i = 0; i < probs.length; i++)
        war += probs[i] * buckets[i];

    return {y: war, label : getProspectDateLabel(f.year, f.month)}
}

function getPostProspectWarPoint(f : DB_PostProspectPlayerModel) : Point
{
    return {y: f.war, label: getProspectDateLabel(f.year, f.month)}
}

function getProspectRankPoint(f : DB_PlayerModel) : Point
{
    if (f.rankWar === null)
        throw new Error("No Rank")
    return {y: f.rankWar, label: f.year == 0 ? "Initial" : `${f.month}-${f.year}`}
}

// Check that the last datapoint in the prospect model has the same date as
// the first of the post prospect model
function validatePostProspectModels(role : string, models : DB_PlayerModel[][], postModels : DB_PostProspectPlayerModel[][]) : void
{
    for (let i = 0; i < models.length; i++)
    {
        const pm = models[i]
        const pppm = i < postModels.length ? postModels[i] : []
        if (pm.length === 0 || pppm.length === 0)
            continue

        const last = pm[pm.length - 1]
        const first = pppm[0]
        if (last.year !== first.year || last.month !== first.month)
            throw new Error(`Player ${last.mlbId} ${role} ${MODEL_STRINGS[i]} (modelId=${last.modelId}): ` +
                `last PlayerModel timestep is ${last.year}-${last.month} but first PostProspectPlayerModel timestep is ${first.year}-${first.month}; they must be the same`)
    }
}

function getPostProspectProgress(models : DB_PostProspectPlayerModel[], idx : number) : WarProgress
{
    const model = models[idx]
    const monthsTotal = models[0].monthsProjected + 1
    return {
        title: `${getProspectDateLabel(model.year, model.month)} Post-Prospect WAR`,
        warAccumulated: model.warAccumulated,
        warProjected: model.warProjected,
        monthsProjected: model.monthsProjected,
        monthsElapsed: Math.max(0, monthsTotal - model.monthsProjected),
        monthsTotal: monthsTotal,
    }
}

function getPostProspectBarScale(linePoints : Point[], models : DB_PostProspectPlayerModel[]) : any
{
    let extents : Point[] = []
    models.forEach(f => {
        extents.push({y: Math.max(f.warAccumulated, 0) + Math.max(f.warProjected, 0), label: ''})
        extents.push({y: Math.min(f.warAccumulated, 0) + Math.min(f.warProjected, 0), label: ''})
    })
    return getWarScale('WAR Through Control', linePoints.concat(extents), 23)
}

function addProspectDataset(optionText : string, title : string, points : Point[], yscale : any, models : DB_PlayerModel[], datasets : GraphDataset[]) : void
{
    let opt = document.createElement('option') as HTMLOptionElement
    opt.text = optionText
    opt.value = datasets.length.toString()
    prospect_graph_selector.appendChild(opt)

    datasets.push({points: points, title: title, yscale: yscale})
    prospectSources.push(models)
    postProspectSources.push([])
    postProspectBarScales.push(null)
}

function addProspectDatasets(role : string, models : DB_PlayerModel[][], postModels : DB_PostProspectPlayerModel[][], warBuckets : number[], datasets : GraphDataset[]) : void
{
    for (let i = 0; i < models.length; i++)
    {
        // PostProspectPlayerModel is only drawn as a continuation of PlayerModel, never on its own
        if (models[i].length > 0)
        {
            const points = models[i].map(f => getProspectWarPoint(f, warBuckets))
            const post = i < postModels.length ? postModels[i] : []
            const postPoints = post.map(getPostProspectWarPoint)
            const linePoints = points.concat(postPoints)
            
            addProspectDataset(`${role} ${MODEL_STRINGS[i]}`, MODEL_STRINGS[i], points,
                getWarScale('Expected WAR Through Control', linePoints, 23), models[i], datasets)

            if (post.length > 0)
            {
                const idx = datasets.length - 1
                datasets[idx].continuation = {points: postPoints, title: `${MODEL_STRINGS[i]} (Post-Prospect)`}
                postProspectSources[idx] = post
                postProspectBarScales[idx] = getPostProspectBarScale(linePoints, post)
            }
        }

        const ranked = models[i].filter(f => f.rankWar !== null)
        if (ranked.length > 0)
        {
            const points = ranked.map(getProspectRankPoint)
            addProspectDataset(`${role} ${MODEL_STRINGS[i]} Rank`, MODEL_STRINGS[i] + " Rank", points,
                getRankScale('Prospect Ranking'), ranked, datasets)
        }
    }
}

function setupProspectModel(hitterModels : DB_PlayerModel[][], pitcherModels : DB_PlayerModel[][],
    hitterPostModels : DB_PostProspectPlayerModel[][], pitcherPostModels : DB_PostProspectPlayerModel[][]) : boolean
{
    // Check data before anything is plotted
    validatePostProspectModels("Hitter", hitterModels, hitterPostModels)
    validatePostProspectModels("Pitcher", pitcherModels, pitcherPostModels)

    let datasets : GraphDataset[] = []
    addProspectDatasets("Hitter", hitterModels, hitterPostModels, assetLoader.war_buckets_hitter, datasets)
    addProspectDatasets("Pitcher", pitcherModels, pitcherPostModels, assetLoader.war_buckets_pitcher, datasets)
    if (datasets.length === 0)
        return false

    const graph = new LineGraph(prospect_line_canvas, datasets, prospectLineCallback, postProspectLineCallback)
    prospect_line_graph = graph

    const latest = prospectSources[0][prospectSources[0].length - 1]
    prospect_pie_graph = new PieGraph(prospect_pie_canvas, prospectPiePointGenerator(latest), "Outcome Distribution")

    prospect_graph_selector.value = "0"
    prospect_graph_selector.addEventListener('change', () => {
        const idx = parseInt(prospect_graph_selector.value)
        const modelId = prospectSources[idx][0].modelId
        graph.setDataset(idx)
        hitterTable?.setPredictions(predHitStats.filter(f => f.Model == modelId))
        pitcherTable?.setPredictions(predPitStats.filter(f => f.Model == modelId))
        graph.fireCallback()
    })

    graph.fireCallback()
    return true
}

function prospectLineCallback(datasetIdx : number, pointIdx : number)
{
    const model = prospectSources[datasetIdx][pointIdx]
    const title_text : string = model.month == 0 ? 
        "Initial Outcome Distribution" :
        `${model.month}-${model.year} Outcome Distribution`

    prospect_pie_graph.updateChart(getModelProbs(model), title_text, PROSPECT_WAR_LABELS)
    setProspectBarVisible(false)
}

function prospectPiePointGenerator(model : DB_PlayerModel) : Point[]
{
    let points : Point[] = []
    const probs = getModelProbs(model)
    for (let i = 0; i < PROSPECT_WAR_LABELS.length; i++)
    {
        points.push({y: probs[i], label:PROSPECT_WAR_LABELS[i]})
    }
    return points
}

function setProspectBarVisible(visible : boolean) : void
{
    prospect_bar_canvas.classList.toggle('vis_hidden', !visible)
    prospect_pie_canvas.classList.toggle('vis_hidden', visible)
}

function postProspectLineCallback(datasetIdx : number, pointIdx : number)
{
    const progress = getPostProspectProgress(postProspectSources[datasetIdx], pointIdx)
    const yscale = postProspectBarScales[datasetIdx]

    if (prospect_bar_graph === null)
        prospect_bar_graph = new WarProgressGraph(prospect_bar_canvas, progress, yscale)
    else
        prospect_bar_graph.update(progress, yscale)
    setProspectBarVisible(true)
}