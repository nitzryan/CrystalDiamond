function getProspectWarPoint(f : DB_PlayerModel, buckets : number[]) : Point
{
    const probs = getModelProbs(f)
    let war = 0;
    for (let i = 0; i < probs.length; i++)
        war += probs[i] * buckets[i];

    const label : string = f.month == 0 ? 'Initial' : `${f.month}-${f.year}`
    return {y: war, label : label}
}

function getProspectRankPoint(f : DB_PlayerModel) : Point
{
    if (f.rankWar === null)
        throw new Error("No Rank")
    return {y: f.rankWar, label: f.year == 0 ? "Initial" : `${f.month}-${f.year}`}
}

function addProspectDataset(optionText : string, title : string, points : Point[], yscale : any, models : DB_PlayerModel[], datasets : GraphDataset[]) : void
{
    let opt = document.createElement('option') as HTMLOptionElement
    opt.text = optionText
    opt.value = datasets.length.toString()
    prospect_graph_selector.appendChild(opt)

    datasets.push({points: points, title: title, yscale: yscale})
    prospectSources.push(models)
}

function addProspectDatasets(role : string, models : DB_PlayerModel[][], warBuckets : number[], datasets : GraphDataset[]) : void
{
    for (let i = 0; i < models.length; i++)
    {
        if (models[i].length > 0)
        {
            const points = models[i].map(f => getProspectWarPoint(f, warBuckets))
            addProspectDataset(`${role} ${MODEL_STRINGS[i]}`, MODEL_STRINGS[i], points,
                getWarScale('Expected WAR Through Control', points, 23), models[i], datasets)
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

function setupProspectModel(hitterModels : DB_PlayerModel[][], pitcherModels : DB_PlayerModel[][]) : boolean
{
    let datasets : GraphDataset[] = []
    addProspectDatasets("Hitter", hitterModels, assetLoader.war_buckets_hitter, datasets)
    addProspectDatasets("Pitcher", pitcherModels, assetLoader.war_buckets_pitcher, datasets)
    if (datasets.length === 0)
        return false

    const graph = new LineGraph(prospect_line_canvas, datasets, prospectLineCallback)
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
        "Iniitial Outcome Distribution" :
        `${model.month}-${model.year} Outcome Distribution`

    prospect_pie_graph.updateChart(getModelProbs(model), title_text, PROSPECT_WAR_LABELS)
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