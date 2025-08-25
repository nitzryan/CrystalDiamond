type HitterStats = {
    level : number,
    year : number,
    team : number,
    league : number,
    pa : number,
    avg : number,
    obp : number,
    slg : number,
    iso : number,
    wrc : number,
    hr : number,
    bbPerc : number,
    kPerc : number,
    sb : number,
    cs : number
}

type HitterModel = {
    year : number,
    month : number,
    probs : number[]
}

type Hitter = {
    firstName : string;
    lastName : string;
    birthDate : Date;
    signYear : number;
    draftPick : number | null;

    stats : HitterStats[],
    models : HitterModel[],
}

type Pitcher = {

}

function getHitterStats(hitterObject : JsonObject) : HitterStats[]
{
    let stats : HitterStats[] = []
    let statsArray : JsonArray = getJsonArray(hitterObject, "stats")

    statsArray.forEach(f => {
        const fObj : JsonObject = f as JsonObject;
        const hs : HitterStats = {
            level : getJsonNumber(fObj, "level"),
            year : getJsonNumber(fObj, "year"),
            team : getJsonNumber(fObj, "team"),
            league : getJsonNumber(fObj, "league"),
            pa : getJsonNumber(fObj, "PA"),
            avg : getJsonNumber(fObj, "AVG"),
            obp : getJsonNumber(fObj, "OBP"),
            slg : getJsonNumber(fObj, "SLG"),
            iso : getJsonNumber(fObj, "ISO"),
            wrc : getJsonNumber(fObj, "wrc"),
            hr : getJsonNumber(fObj, "HR"),
            bbPerc : getJsonNumber(fObj, "BB%"),
            kPerc : getJsonNumber(fObj, "K%"),
            sb : getJsonNumber(fObj, "SB"),
            cs : getJsonNumber(fObj, "CS")
        }
        stats.push(hs)
    })

    return stats
}

function getHitterModels(hitterObject : JsonObject) : HitterModel[]
{
    let models : HitterModel[] = []
    let modelArray : JsonArray = getJsonArray(hitterObject, "model")
    modelArray.forEach(f => {
        const fObj : JsonObject = f as JsonObject;
        const probArray = getJsonArray(fObj, "probs");
        
        const m : HitterModel = {
            year : getJsonNumber(fObj, "year"),
            month : getJsonNumber(fObj, "month"),
            probs : probArray.map(f => {
                const num = f as number;
                return num;
            })
        }
        models.push(m);
    })

    return models;
}

async function loadHitter(id : number) : Promise<Hitter | null>
{
    let hitterObject = await retrieveJsonNullable(`../../assets/player/h${id}.json.gz`);
    if (hitterObject !== null)
    {
        const hitter : Hitter = {
            firstName : getJsonString(hitterObject, "firstName"),
            lastName : getJsonString(hitterObject, "lastName"),
            birthDate : new Date(
                getJsonNumber(hitterObject, "birthYear"),
                getJsonNumber(hitterObject, "birthMonth"),
                getJsonNumber(hitterObject, "birthDate")
            ),
            signYear : getJsonNumber(hitterObject, "startYear"),
            draftPick : getJsonNumberNullable(hitterObject, "draftPick"),
            stats : getHitterStats(hitterObject),
            models : getHitterModels(hitterObject)
        }

        return hitter;
    }
    return null;
}

function updateHitterStats(hitter : Hitter)
{
    const stats_body = getElementByIdStrict('tstats_body')
    hitter.stats.forEach(f => {
        const tr = document.createElement('tr')
        tr.innerHTML = `
            <td>${f.year}</td>
            <td>${level_map[f.level]}</td>
            <td>${getTeamAbbr(f.team, f.year)}</td>
            <td>${getLeagueAbbr(f.league)}</td>
            <td class="align_right">${f.pa}</td>
            <td class="align_right">${f.avg.toFixed(3)}</td>
            <td class="align_right">${f.obp.toFixed(3)}</td>
            <td class="align_right">${f.slg.toFixed(3)}</td>
            <td class="align_right">${f.iso.toFixed(3)}</td>
            <td class="align_right">${f.wrc}</td>
            <td class="align_right">${f.hr}</td>
            <td class="align_right">${f.bbPerc.toFixed(1)}</td>
            <td class="align_right">${f.kPerc.toFixed(1)}</td>
            <td class="align_right">${f.sb}</td>
            <td class="align_right">${f.cs}</td>
        `
        stats_body.appendChild(tr)
    })
}

const HITTER_WAR_BUCKETS = [0,0.5,2.5,7.5,15,25,35]
const HITTER_WAR_LABELS = ["<=0", "0-1", "1-5", "5-10", "10-20", "20-30", "30+"]

function piePointGenerator(model : HitterModel) : Point[]
{
    let points : Point[] = []
    for (let i = 0; i < HITTER_WAR_LABELS.length; i++)
    {
        points.push({y: model.probs[i], label:HITTER_WAR_LABELS[i]})
    }
    return points
}

function lineCallback(index : number)
{
    if (hitter !== null)
    {
        const model = hitter.models[index]
        if (pie_graph === null)
            throw new Error("Pie Graph null at lineCallback")

        const title_text : string = model.month == 0 ? 
            "Iniitial Outcome Distribution" :
            `${model.month}-${model.year} Outcome Distribution`
        pie_graph.updateChart(model.probs, title_text)
    } else if (pitcher !== null)
    {

    } else {
        throw new Error("Both pitcher and hitter null at lineCallback")
    }
}

function setupModel(hitter : Hitter) : void
{
    const line_points  = hitter.models.map(f => {
        let war = 0;
        for (let i = 0; i < f.probs.length; i++)
            war += f.probs[i] * HITTER_WAR_BUCKETS[i];

        const label : string = f.month == 0 ? 'Initial' : `${f.month}-${f.year}`
        const p : Point = {y: war, label : label}
        return p;
    })
    line_graph = new LineGraph(model_graph, line_points, lineCallback)

    const pie_points = piePointGenerator(hitter.models[hitter.models.length - 1])
    pie_graph = new PieGraph(model_pie, pie_points, "Outcome Distribution")
}

const model_pie = getElementByIdStrict("projWarPie") as HTMLCanvasElement
const model_graph = getElementByIdStrict("projWarGraph") as HTMLCanvasElement
let line_graph : LineGraph | null = null
let pie_graph : PieGraph | null = null
let hitter : Hitter | null = null
let pitcher : Pitcher | null = null
let keyControls : KeyControls | null = null
let searchBar : SearchBar | null = null

async function main()
{
    const player_search_data = retrieveJson('../../assets/player_search.json.gz')
    org_map = await retrieveJson("../../assets/map.json.gz")
    const id = getQueryParam("id")
    hitter = await loadHitter(id)
    if (hitter !== null)
    {
        updateElementText("player_name", `${hitter.firstName} ${hitter.lastName}`)
        const age = getDateDelta(hitter.birthDate, new Date())
        updateElementText("player_age", `${age[0]} years, ${age[1]} months, ${age[2]} days`)
        if (hitter.draftPick !== null)
        {
            updateElementText("player_draft", `#${hitter.draftPick} Overall, ${hitter.signYear}`)
        }

        updateHitterStats(hitter)
        setupModel(hitter)
    }

    keyControls = new KeyControls(document, (x_inc) => {
        if (line_graph !== null)
            line_graph.increment_index(x_inc)
    })

    searchBar = new SearchBar(await player_search_data)
}

main()