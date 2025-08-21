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
            <td>${f.pa}</td>
            <td>${f.avg.toFixed(3)}</td>
            <td>${f.obp.toFixed(3)}</td>
            <td>${f.slg.toFixed(3)}</td>
            <td>${f.iso.toFixed(3)}</td>
            <td>${f.wrc}</td>
            <td>${f.hr}</td>
            <td>${f.bbPerc.toFixed(1)}</td>
            <td>${f.kPerc.toFixed(1)}</td>
            <td>${f.sb}</td>
            <td>${f.cs}</td>
        `
        stats_body.appendChild(tr)
    })
}

const HITTER_WAR_BUCKETS = [0,0.5,2.5,7.5,15,25,35]

function setupModel(hitter : Hitter) : void
{
    const points  = hitter.models.map(f => {
        let war = 0;
        for (let i = 0; i < f.probs.length; i++)
            war += f.probs[i] * HITTER_WAR_BUCKETS[i];

        const label : string = f.month == 0 ? 'Initial' : `${f.month}-${f.year}`
        const p : Point = {y: war, label : label}
        return p;
    })
    console.log(hitter.models)
    const lineGraph = new LineGraph(model_graph as HTMLCanvasElement, points,null)
}

const model_pie = getElementByIdStrict("projWarPie")
const model_graph = getElementByIdStrict("projWarGraph")

async function main()
{
    org_map = await retrieveJson("../../assets/map.json.gz")
    const id = getQueryParam("id")
    const hitter = await loadHitter(id)
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
}

main()