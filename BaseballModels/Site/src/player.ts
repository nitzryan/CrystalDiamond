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

type Model = {
    year : number,
    month : number,
    probs : number[],
    rank: number | null
}

type Person = {
    firstName : string;
    lastName : string;
    birthDate : Date;
    signYear : number;
    draftPick : number | null;
}

type Hitter = {
    person : Person,
    stats : HitterStats[],
    models : Model[],
}

type PitcherStats = {
    level : number,
    year : number,
    team : number,
    league : number,
    ip : string,
    era : number,
    fip : number,
    hrrate : number,
    bbperc : number,
    kperc : number,
    gorate : number,
}

type Pitcher = {
    person : Person,
    stats : PitcherStats[],
    models : Model[]
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

function getPitcherStats(pitcherObject : JsonObject) : PitcherStats[]
{
    let stats : PitcherStats[] = []
    let statsArray : JsonArray = getJsonArray(pitcherObject, "stats")

    statsArray.forEach(f => {
        const fObj : JsonObject = f as JsonObject;
        const ps : PitcherStats = {
            level : getJsonNumber(fObj, "level"),
            year : getJsonNumber(fObj, "year"),
            team : getJsonNumber(fObj, "team"),
            league : getJsonNumber(fObj, "league"),
            ip : getJsonString(fObj, "IP"),
            era : getJsonNumber(fObj, "ERA"),
            fip : getJsonNumber(fObj, "FIP"),
            hrrate : getJsonNumber(fObj, "HR9"),
            bbperc : getJsonNumber(fObj, "BB%"),
            kperc : getJsonNumber(fObj, "K%"),
            gorate : getJsonNumber(fObj, "GO%")
        }
        stats.push(ps)
    })

    return stats
}

function getModels(obj : JsonObject) : Model[]
{
    let models : Model[] = []
    let modelArray : JsonArray = getJsonArray(obj, "model")

    modelArray.forEach(f => {
        const fObj : JsonObject = f as JsonObject;
        const probArray = getJsonArray(fObj, "probs");

        const m : Model = {
            year : getJsonNumber(fObj, "year"),
            month : getJsonNumber(fObj, "month"),
            probs : probArray.map(f => {
                const num = f as number;
                return num;
            }),
            rank : getJsonNumberNullable(fObj, "rank")
        }
        models.push(m);
    })

    return models;
}

function getPerson(obj : JsonObject)
{
    const p : Person = {
        firstName : getJsonString(obj, "firstName"),
        lastName : getJsonString(obj, "lastName"),
        birthDate : new Date(
            getJsonNumber(obj, "birthYear"),
            getJsonNumber(obj, "birthMonth"),
            getJsonNumber(obj, "birthDate")
        ),
        signYear : getJsonNumber(obj, "startYear"),
        draftPick : getJsonNumberNullable(obj, "draftPick"),
    }

    return p
}

async function loadHitter(id : number) : Promise<Hitter | null>
{
    let hitterObject = await retrieveJsonNullable(`../../assets/player/h${id}.json.gz`);
    if (hitterObject !== null)
    {
        const hitter : Hitter = {
            person: getPerson(hitterObject),
            stats : getHitterStats(hitterObject),
            models : getModels(hitterObject)
        }

        return hitter;
    }
    return null;
}

async function loadPitcher(id : number) : Promise<Pitcher | null>
{
    let pitcherObject = await retrieveJsonNullable(`../../assets/player/p${id}.json.gz`);
    if (pitcherObject !== null)
    {
        const pitcher : Pitcher = {
            person: getPerson(pitcherObject),
            stats : getPitcherStats(pitcherObject),
            models : getModels(pitcherObject)
        }

        return pitcher;
    }
    return null;
}

function updateHitterStats(hitter : Hitter)
{
    const stats_body = getElementByIdStrict('h_stats_body')
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

function updatePitcherStats(pitcher : Pitcher)
{
    const stats_body = getElementByIdStrict('p_stats_body')
    pitcher.stats.forEach(f => {
        const tr = document.createElement('tr')
        tr.innerHTML = `
            <td>${f.year}</td>
            <td>${level_map[f.level]}</td>
            <td>${getTeamAbbr(f.team, f.year)}</td>
            <td>${getLeagueAbbr(f.league)}</td>
            <td class="align_right">${f.ip}</td>
            <td class="align_right">${f.era.toFixed(2)}</td>
            <td class="align_right">${f.fip.toFixed(2)}</td>
            <td class="align_right">${f.hrrate.toFixed(1)}</td>
            <td class="align_right">${f.bbperc.toFixed(1)}</td>
            <td class="align_right">${f.kperc.toFixed(1)}</td>
            <td class="align_right">${f.gorate.toFixed(1)}</td>
        `
        stats_body.append(tr)
    })
}

const HITTER_WAR_BUCKETS = [0,0.5,3,7.5,15,25,35]
const HITTER_WAR_LABELS = ["<=0", "0-1", "1-5", "5-10", "10-20", "20-30", "30+"]

function piePointGenerator(model : Model) : Point[]
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
    let model : Model | null = null
    if (hitter !== null)
    {
        model = hitter.models[index]
        
    } else if (pitcher !== null)
    {
        model = pitcher.models[index]
    } else {
        throw new Error("Both pitcher and hitter null at lineCallback")
    }

    if (model !== null)
    {
        if (pie_graph === null)
            throw new Error("Pie Graph null at lineCallback")

        const title_text : string = model.month == 0 ? 
            "Iniitial Outcome Distribution" :
            `${model.month}-${model.year} Outcome Distribution`
        pie_graph.updateChart(model.probs, title_text)
    } else {
        throw new Error("Model was not set for hitter or pitcher")
    }
}

function setupModel(models : Model[]) : void
{
    const line_points  = models.map(f => {
        let war = 0;
        for (let i = 0; i < f.probs.length; i++)
            war += f.probs[i] * HITTER_WAR_BUCKETS[i];

        const label : string = f.month == 0 ? 'Initial' : `${f.month}-${f.year}`
        const p : Point = {y: war, label : label}
        return p;
    })
    
    let ranks : Point[] = []
    for (const model of models)
    {
        let m = model.month
        let y = model.year
        if (model.rank !== null)
            ranks.push({y: model.rank, label: m == 0 ? 'Initial' : `${m}-${y}`})
    }

    line_graph = new LineGraph(model_graph, line_points, ranks, lineCallback)

    const pie_points = piePointGenerator(models[models.length - 1])
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

    const lh = loadHitter(id)
    const lp = loadPitcher(id)

    let person : Person | null = null
    hitter = await lh
    if (hitter !== null)
    {
        person = hitter.person
        updateHitterStats(hitter)
        setupModel(hitter.models)
    } else {
        const hitterStats = getElementByIdStrict('hitter_stats')
        hitterStats.classList.add('hidden')
    }

    pitcher = await lp
    if (pitcher !== null)
    {
        person = pitcher.person
        updatePitcherStats(pitcher)
        setupModel(pitcher.models)
    } else {
        const pitcherStats = getElementByIdStrict('pitcher_stats')
        pitcherStats.classList.add('hidden')
    }

    // Set person
    if (person !== null)
    {
        updateElementText("player_name", `${person.firstName} ${person.lastName}`)
        const age = getDateDelta(person.birthDate, new Date())
        updateElementText("player_age", `${age[0]} years, ${age[1]} months, ${age[2]} days`)
        if (person.draftPick !== null)
        {
            updateElementText("player_draft", `#${person.draftPick} Overall, ${person.signYear}`)
        }
        document.title = person.firstName + " " + person.lastName
    } else {
        throw Error("No person found")
    }

    keyControls = new KeyControls(document, (x_inc) => {
        if (line_graph !== null)
            line_graph.increment_index(x_inc)
    })

    searchBar = new SearchBar(await player_search_data)
}

main()