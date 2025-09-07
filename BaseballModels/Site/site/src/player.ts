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

type Draft = {
    pick : number,
    round : string,
    bonus : number
}

type Person = {
    firstName : string;
    lastName : string;
    birthDate : Date;
    signYear : number;
    draft : Draft | null;
    position : string;
    status : string;
    parentId : number | null;
    isHitter : boolean;
    isPitcher : boolean;
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

function getHitterStats(hitterObject : JsonObject) : HitterStats[]
{
    let stats : HitterStats[] = []
    let statsArray : JsonArray = getJsonArray(hitterObject, "hit_stats")

    statsArray.forEach(f => {
        const fObj : JsonObject = f as JsonObject;
        const hs : HitterStats = {
            level : getJsonNumber(fObj, "levelId"),
            year : getJsonNumber(fObj, "year"),
            team : getJsonNumber(fObj, "teamId"),
            league : getJsonNumber(fObj, "leagueId"),
            pa : getJsonNumber(fObj, "PA"),
            avg : getJsonNumber(fObj, "AVG"),
            obp : getJsonNumber(fObj, "OBP"),
            slg : getJsonNumber(fObj, "SLG"),
            iso : getJsonNumber(fObj, "ISO"),
            wrc : getJsonNumber(fObj, "WRC"),
            hr : getJsonNumber(fObj, "HR"),
            bbPerc : getJsonNumber(fObj, "BBPerc"),
            kPerc : getJsonNumber(fObj, "KPerc"),
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
    let statsArray : JsonArray = getJsonArray(pitcherObject, "pit_stats")

    statsArray.forEach(f => {
        const fObj : JsonObject = f as JsonObject;
        const ps : PitcherStats = {
            level : getJsonNumber(fObj, "levelId"),
            year : getJsonNumber(fObj, "year"),
            team : getJsonNumber(fObj, "teamId"),
            league : getJsonNumber(fObj, "leagueId"),
            ip : getJsonString(fObj, "IP"),
            era : getJsonNumber(fObj, "ERA"),
            fip : getJsonNumber(fObj, "FIP"),
            hrrate : getJsonNumber(fObj, "HR9"),
            bbperc : getJsonNumber(fObj, "BBPerc"),
            kperc : getJsonNumber(fObj, "KPerc"),
            gorate : getJsonNumber(fObj, "GOPerc")
        }
        stats.push(ps)
    })

    return stats
}

function getModels(obj : JsonObject, name : string) : Model[]
{
    let models : Model[] = []
    let modelArray : JsonArray = getJsonArray(obj, name)

    modelArray.forEach(f => {
        const fObj : JsonObject = f as JsonObject;
        const probString = getJsonString(fObj, "probs");
        const probArray : number[] = probString.split(',').map(Number)

        const m : Model = {
            year : getJsonNumber(fObj, "year"),
            month : getJsonNumber(fObj, "month"),
            probs : probArray,
            rank : getJsonNumberNullable(fObj, "rank")
        }
        models.push(m);
    })

    return models;
}

function getPerson(obj : JsonObject)
{
    const draftPick : number | null = obj["draftPick"] as number | null
    let draft : Draft | null = null
    if (draftPick !== null)
        draft = {
            pick : draftPick,
            round : getJsonString(obj, "draftRound"),
            bonus : getJsonNumber(obj, "draftBonus")
        }
    
    const p : Person = {
        firstName : getJsonString(obj, "firstName"),
        lastName : getJsonString(obj, "lastName"),
        birthDate : new Date(
            getJsonNumber(obj, "birthYear"),
            getJsonNumber(obj, "birthMonth"),
            getJsonNumber(obj, "birthDate")
        ),
        signYear : getJsonNumber(obj, "startYear"),
        position : getJsonString(obj, "position"),
        status : getJsonString(obj, "status"),
        draft : draft,
        parentId : getJsonNumber(obj, "orgId"),
        isHitter : obj["isHitter"] as boolean,
        isPitcher : obj["isPitcher"] as boolean
    }

    return p
}

function updateHitterStats(hitterStats : HitterStats[])
{
    const stats_body = getElementByIdStrict('h_stats_body')
    hitterStats.forEach(f => {
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

function updatePitcherStats(pitcherStats : PitcherStats[])
{
    const stats_body = getElementByIdStrict('p_stats_body')
    pitcherStats.forEach(f => {
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
    let model : Model
    if (line_graph.getModelIsHitter())
    {
        model = hitterModels[index]
    } else
    {
        model = pitcherModels[index]
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

function setupModel(hitterModels : Model[], pitcherModels : Model[]) : void
{
    let war_map = (f: Model) => {
        let war = 0;
        for (let i = 0; i < f.probs.length; i++)
            war += f.probs[i] * HITTER_WAR_BUCKETS[i];

        const label : string = f.month == 0 ? 'Initial' : `${f.month}-${f.year}`
        const p : Point = {y: war, label : label}
        return p;
    }

    let rank_map = (f : Model) => {
        let month = f.month
        let year = f.year
        if (f.rank === null)
            throw new Error("No Rank")
        const p : Point = {y: f.rank, label: year == 0 ? "Initial" : `${month}-${year}`}
        return p
    }
    
    const hitter_war = hitterModels.map(war_map)
    const pitcher_war = pitcherModels.map(war_map)

    let hitter_ranks : Point[] = []
    let pitcher_ranks : Point[] = []
    try {
        hitter_ranks = hitterModels.map(rank_map)
        pitcher_ranks = pitcherModels.map(rank_map)
    } catch (e) {}

    line_graph = new LineGraph(model_graph, hitter_war, hitter_ranks, pitcher_war, pitcher_ranks, lineCallback)


    const pie_points = person.isHitter ? 
        piePointGenerator(hitterModels[hitterModels.length - 1]) :
        piePointGenerator(pitcherModels[pitcherModels.length - 1])
    pie_graph = new PieGraph(model_pie, pie_points, "Outcome Distribution")
}

const model_pie = getElementByIdStrict("projWarPie") as HTMLCanvasElement
const model_graph = getElementByIdStrict("projWarGraph") as HTMLCanvasElement
let line_graph : LineGraph
let pie_graph : PieGraph
let keyControls : KeyControls
let searchBar : SearchBar

let person : Person
let hitterModels : Model[]
let pitcherModels : Model[]

async function main()
{
    const id = getQueryParam("id")
    var player_data = fetch(`/player/${id}`)

    const player_search_data = retrieveJson('../../assets/player_search.json.gz')
    org_map = await retrieveJson("../../assets/map.json.gz")

    const pd = await (await player_data).json() as JsonObject
    console.log(pd)
    person = getPerson(pd)

    let hitterStats = person.isHitter ? getHitterStats(pd) : []
    let pitcherStats = person.isPitcher ? getPitcherStats(pd) : []

    if (hitterStats.length > 0)
    {
        updateHitterStats(hitterStats)
        getElementByIdStrict('hitter_stats').classList.remove('hidden')
    }
    if (pitcherStats.length > 0)
    {
        updatePitcherStats(pitcherStats)
        getElementByIdStrict('pitcher_stats').classList.remove('hidden')
    }
    
    hitterModels = person.isHitter ? getModels(pd, "hit_models") : []
    pitcherModels = person.isPitcher ? getModels(pd, "pit_models") : []

    setupModel(hitterModels, pitcherModels)

    // Set person
    updateElementText("player_name", `${person.firstName} ${person.lastName}`)
    updateElementText("player_position", person.position)
    updateElementText("player_status", person.status)

    if (person.parentId !== null)
    {
        const player_team = getElementByIdStrict("player_team") as HTMLLinkElement
        player_team.innerText = getParentName(person.parentId)
        player_team.href = `teams.html?team=${person.parentId}`
    } else 
    {
        updateElementText("player_team", "Free Agent")
    }
    const age = getDateDelta(person.birthDate, new Date())
    updateElementText("player_age", `${age[0]}y, ${age[1]}m, ${age[2]}d`)
    if (person.draft !== null)
    {
        const round : string = isNaN(parseFloat(person.draft.round)) ? person.draft.round : "Round " + person.draft.round
        updateElementText("player_draft", `${person.signYear} Draft, ${round} (${getOrdinalNumber(person.draft.pick)} Overall)\n$${person.draft.bonus.toLocaleString()} Bonus`)
    }
    document.title = person.firstName + " " + person.lastName

    keyControls = new KeyControls(document, (x_inc) => {
        if (line_graph !== null)
            line_graph.increment_index(x_inc)
    })

    searchBar = new SearchBar(await player_search_data)
}

main()