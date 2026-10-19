const MLB_WAR_SERIES = 6

type MlbWarTimestep = {
    modelId : number,
    year : number,
    month : number,
    season : number,
    probs : number[],
    mean : number,
    rank : number,
}

type MlbDatasetSource = {
    timesteps : MlbWarTimestep[],
    labels : string[],
}

let mlbSources : MlbDatasetSource[] = []

// Gets data N years into future, handling where the year ticks over
function getMlbBucketAverages(averages : Map<number, number[]>, month : number, yearOffset : number) : number[]
{
    const bucketMonth = (yearOffset === 0 && month !== 9) ? month : 1
    const values = averages.get(bucketMonth)
    if (values === undefined)
        throw new Error(`No MLB WAR bucket averages for month ${bucketMonth}`)
    return values
}

// Series s projects the season (s + 1) years after the timestep year.
// Year offset 0 is the current year, unless month is 9 where it is already next year
function buildMlbWarTimestep(modelId : number, year : number, month : number, series : number, probs : number[][], ranks : number[][], averages : Map<number, number[]>) : MlbWarTimestep
{
    const yearOffset = month === 9 ? series : series + 1
    const bucketWar = getMlbBucketAverages(averages, month, yearOffset)

    let mean = 0
    for (let bkt = 0; bkt < probs[yearOffset].length; bkt++)
        mean += probs[yearOffset][bkt] * bucketWar[bkt]

    return {
        modelId: modelId,
        year: year,
        month: month,
        season: year + series + 1,
        probs: probs[yearOffset],
        mean: mean,
        rank: ranks[yearOffset][0],
    }
}

function getHitterMlbWar(hitterObject : JsonObject) : MlbWarTimestep[][]
{
    let averages = new Map<number, number[]>()
    getJsonArray(hitterObject, "hit_mlb_buckets").forEach(f => {
        const b = new DB_HitterMlbWarBucketAverages(f as JsonObject)
        averages.set(b.month, [b.war0, b.war1, b.war2, b.war3, b.war4, b.war5, b.war6, b.war7])
    })

    let series : MlbWarTimestep[][] = []
    for (let s = 0; s < MLB_WAR_SERIES; s++)
        series.push([])

    getJsonArray(hitterObject, "hit_mlb_models").forEach(f => {
        const m = new DB_HitterMlbWarModel(f as JsonObject)
        for (let s = 0; s < MLB_WAR_SERIES; s++)
            series[s].push(buildMlbWarTimestep(m.modelId, m.year, m.month, s, m.probsWar, m.ranksWar, averages))
    })

    return series
}

function getPitcherMlbWar(pitcherObject : JsonObject) : { starter : MlbWarTimestep[][], reliever : MlbWarTimestep[][] }
{
    let averagesSP = new Map<number, number[]>()
    let averagesRP = new Map<number, number[]>()
    getJsonArray(pitcherObject, "pit_mlb_buckets").forEach(f => {
        const b = new DB_PitcherMlbWarBucketAverages(f as JsonObject)
        averagesSP.set(b.month, [b.warSP0, b.warSP1, b.warSP2, b.warSP3, b.warSP4, b.warSP5, b.warSP6, b.warSP7])
        averagesRP.set(b.month, [b.warRP0, b.warRP1, b.warRP2, b.warRP3, b.warRP4, b.warRP5, b.warRP6, b.warRP7])
    })

    let starter : MlbWarTimestep[][] = []
    let reliever : MlbWarTimestep[][] = []
    for (let s = 0; s < MLB_WAR_SERIES; s++)
    {
        starter.push([])
        reliever.push([])
    }

    getJsonArray(pitcherObject, "pit_mlb_models").forEach(f => {
        const m = new DB_PitcherMlbWarModel(f as JsonObject)
        for (let s = 0; s < MLB_WAR_SERIES; s++)
        {
            starter[s].push(buildMlbWarTimestep(m.modelId, m.year, m.month, s, m.probsWarSP, m.ranksWarSP, averagesSP))
            reliever[s].push(buildMlbWarTimestep(m.modelId, m.year, m.month, s, m.probsWarRP, m.ranksWarRP, averagesRP))
        }
    })

    return { starter: starter, reliever: reliever }
}

function addMlbDatasets(role : string, series : MlbWarTimestep[][], labels : string[], datasets : GraphDataset[]) : void
{
    if (series.length === 0)
        return

    for (let i = 0; i < MODEL_VALUES.length; i++)
    {
        const modelSeries = series.map(f => f.filter(g => g.modelId === MODEL_VALUES[i]))
        if (modelSeries[0].length === 0)
            continue

        let warOpt = document.createElement('option') as HTMLOptionElement
        warOpt.text = `${role} ${MODEL_STRINGS[i]}`
        warOpt.value = datasets.length.toString()
        mlb_graph_selector.appendChild(warOpt)
        for (const timesteps of modelSeries)
        {
            const points : Point[] = timesteps.map(f => ({y: f.mean, label: `${f.month}-${f.year}`}))
            datasets.push({points: points, title: MODEL_STRINGS[i], yscale: getWarScale('Expected Season WAR', points, 8)})
            mlbSources.push({timesteps: timesteps, labels: labels})
        }

        let rankOpt = document.createElement('option') as HTMLOptionElement
        rankOpt.text = `${role} ${MODEL_STRINGS[i]} Rank`
        rankOpt.value = datasets.length.toString()
        mlb_graph_selector.appendChild(rankOpt)
        for (const timesteps of modelSeries)
        {
            const points : Point[] = timesteps.map(f => ({y: f.rank, label: `${f.month}-${f.year}`}))
            datasets.push({points: points, title: MODEL_STRINGS[i] + " Rank", yscale: getRankScale(`${role} Rank`)})
            mlbSources.push({timesteps: timesteps, labels: labels})
        }
    }
}

function mlbLineCallback(datasetIdx : number, pointIdx : number)
{
    if (mlb_pie_graph === null)
        throw new Error("MLB Pie Graph null at mlbLineCallback")

    const source = mlbSources[datasetIdx]
    const timestep = source.timesteps[pointIdx]
    const title_text = `${timestep.month}-${timestep.year} Projection for ${timestep.season}`
    mlb_pie_graph.updateChart(timestep.probs, title_text, source.labels)
}

function setupMlbModel(hitter : MlbWarTimestep[][], starter : MlbWarTimestep[][], reliever : MlbWarTimestep[][]) : boolean
{
    let datasets : GraphDataset[] = []
    addMlbDatasets("Hitter", hitter, MLB_HIT_WAR_LABELS, datasets)
    addMlbDatasets("SP", starter, MLB_SP_WAR_LABELS, datasets)
    addMlbDatasets("RP", reliever, MLB_RP_WAR_LABELS, datasets)
    if (datasets.length === 0)
        return false

    const graph = new LineGraph(mlb_graph, datasets, mlbLineCallback)
    mlb_line_graph = graph

    const source = mlbSources[0]
    const latest = source.timesteps[source.timesteps.length - 1]
    const pie_points : Point[] = latest.probs.map((f, idx) => ({y: f, label: source.labels[idx]}))
    mlb_pie_graph = new PieGraph(mlb_pie_canvas, pie_points, "Outcome Distribution", MLB_PIE_COLORS)

    // Dataset index is the block start from the graph selector plus the series from the year selector
    const onChange = () => {
        graph.setDataset(parseInt(mlb_graph_selector.value) + parseInt(mlb_year_selector.value))
        graph.fireCallback()
    }
    mlb_graph_selector.value = "0"
    mlb_year_selector.value = "0"
    mlb_graph_selector.addEventListener('change', onChange)
    mlb_year_selector.addEventListener('change', onChange)

    graph.fireCallback()
    return true
}