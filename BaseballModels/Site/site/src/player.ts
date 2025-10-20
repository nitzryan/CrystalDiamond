type HitterStats = {
    level : number,
    year : number,
    month : number | null,
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
    modelId : number,
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
    inTraining : boolean;
}

type PitcherStats = {
    level : number,
    year : number,
    month : number | null,
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
    
    let statsMonthArray : JsonArray = getJsonArray(hitterObject, "hit_month_stats")
    statsArray = statsArray.concat(statsMonthArray)

    statsArray.forEach(f => {
        const fObj : JsonObject = f as JsonObject;
        const hs : HitterStats = {
            level : getJsonNumber(fObj, "levelId"),
            year : getJsonNumber(fObj, "year"),
            month : getJsonNumberNullable(fObj, "month"),
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

    stats.sort((a,b) => {
        if (a.year < b.year)
            return -1
        if (a.year > b.year)
            return 1

        if (a.month === null && b.month === null)
            return b.level - a.level
        if (a.month === null)
            return -1
        if (b.month === null)
            return 1
        if (a.month === b.month)
            return b.level - a.level
        return a.month - b.month
    })

    return stats
}

function getPitcherStats(pitcherObject : JsonObject) : PitcherStats[]
{
    let stats : PitcherStats[] = []
    let statsArray : JsonArray = getJsonArray(pitcherObject, "pit_stats")

    let statsMonthArray : JsonArray = getJsonArray(pitcherObject, "pit_month_stats")
    statsArray = statsArray.concat(statsMonthArray)

    statsArray.forEach(f => {
        const fObj : JsonObject = f as JsonObject;
        const ps : PitcherStats = {
            level : getJsonNumber(fObj, "levelId"),
            year : getJsonNumber(fObj, "year"),
            month : getJsonNumberNullable(fObj, "month"),
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

    stats.sort((a,b) => {
        if (a.year < b.year)
            return -1
        if (a.year > b.year)
            return 1

        if (a.month === null && b.month === null)
            return b.level - a.level
        if (a.month === null)
            return -1
        if (b.month === null)
            return 1
        if (a.month === b.month)
            return b.level - a.level
        return a.month - b.month
    })

    return stats
}

function getModels(obj : JsonObject, name : string) : Model[][]
{
    let models : Model[] = []
    let modelArray : JsonArray = getJsonArray(obj, name)

    modelArray.forEach(f => {
        const fObj : JsonObject = f as JsonObject;
        const probString = getJsonString(fObj, "probsWar");
        const probArray : number[] = probString.split(',').map(Number)

        const m : Model = {
            year : getJsonNumber(fObj, "year"),
            month : getJsonNumber(fObj, "month"),
            probs : probArray,
            modelId : getJsonNumber(fObj, "modelId"),
            // @ts-ignore
            rank : fObj["rankWar"],
        }
        models.push(m);
    })

    let models_list : Model[][] = []
    MODEL_VALUES.forEach(f => {
        models_list.push(models.filter(g => g.modelId === f))
    })

    return models_list;
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
        isPitcher : obj["isPitcher"] as boolean,
        inTraining : obj["inTraining"] as boolean,
    }

    return p
}

function tableUpdateCallback(tablebody : HTMLElement, monthcol : HTMLElement, year : string, type : string)
{
    var rows = Array.from(tablebody.getElementsByTagName('tr'))
    rows.forEach(f => {
        const y = f.dataset.year
        const t = f.dataset.type

        if (year !== y)
            return

        if (type === t)
            f.classList.add('hidden')
        else
            f.classList.remove('hidden')
    })

    // Only show the month column if any monthly values are shown
    const any_monthly = rows.reduce((a,b) => {
        if (a)
            return true
        
        const t = b.dataset.type
        return (t === 'month') && (!b.classList.contains('hidden'))
    }, false)

    var monthly_elements = document.getElementsByClassName('table_month')
    if (any_monthly)
        for (let i = 0; i < monthly_elements.length; i++)
            monthly_elements[i].classList.remove('hidden')
    else
        for (let i = 0; i < monthly_elements.length; i++)
            monthly_elements[i].classList.add('hidden')
}

function updateHitterStats(hitterStats : HitterStats[])
{
    const stats_body = getElementByIdStrict('h_stats_body')
    const hcol_month = getElementByIdStrict('hcol_month')

    let prevYear : number | null = null
    let prevYearMonthly : number | null = null

    hitterStats.forEach(f => {
        const tr = document.createElement('tr')
        let isFirst = false
        if (f.month !== null)
        {
            if (f.year != prevYearMonthly)
            {
                isFirst = true
                prevYearMonthly = f.year
            }
            tr.classList.add('hidden')
        }
        else {
            if (f.year != prevYear)
            {
                isFirst = true
                prevYear = f.year
            }
        }
        
        let teamAbbr : string = ""
        // VSL/DSL has some teams that split orgs
        try {
            teamAbbr = getTeamAbbr(f.team, f.year)
        } catch(e)
        {
            if (f.league != 134 && f.league != 130) // If not VSL/DSL, it is an error
                throw e
        }

        tr.innerHTML = `
            <td></td>
            <td>${f.year}</td>
            <td class='table_month hidden'>${f.month !== null ? MONTH_CODES[f.month] : ""}</td>
            <td>${level_map[f.level]}</td>
            <td>${teamAbbr}</td>
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

        tr.dataset.year = f.year.toString()
        tr.dataset.type = f.month === null ? "year" : "month"

        if (isFirst)
        {
            tr.classList.add('row_first')
            
            let button_td = tr.getElementsByTagName('td')[0]
            let button = document.createElement('button')
            
            button.classList.add('table_button')
            
            if (f.month === null)
            {
                button.innerText = '+'
                button.classList.add('table_expand')
                
                button.addEventListener('click', () => {
                    tableUpdateCallback(stats_body, hcol_month, f.year.toString(), 'year')
                })
            } else {
                button.innerText = '-'
                button.classList.add('table_retract')
                
                button.addEventListener('click', () => {
                    tableUpdateCallback(stats_body, hcol_month, f.year.toString(), 'month')
                })
            }
            

            button_td.appendChild(button)
        }

        stats_body.appendChild(tr)
    })
}

function updatePitcherStats(pitcherStats : PitcherStats[])
{
    const stats_body = getElementByIdStrict('p_stats_body')
    const pcol_month = getElementByIdStrict('pcol_month')

    let prevYear : number | null = null
    let prevYearMonthly : number | null = null

    pitcherStats.forEach(f => {
        const tr = document.createElement('tr')
        let isFirst = false
        if (f.month !== null)
        {
            if (f.year != prevYearMonthly)
            {
                isFirst = true
                prevYearMonthly = f.year
            }
            tr.classList.add('hidden')
        }
        else {
            if (f.year != prevYear)
            {
                isFirst = true
                prevYear = f.year
            }
        }
        
        let teamAbbr : string = ""
        // VSL/DSL has some teams that split orgs
        try {
            teamAbbr = getTeamAbbr(f.team, f.year)
        } catch(e)
        {
            if (f.league != 134 && f.league != 130) // If not VSL/DSL, it is an error
                throw e
        }

        tr.innerHTML = `
            <td></td>
            <td>${f.year}</td>
            <td class='table_month hidden'>${f.month !== null ? MONTH_CODES[f.month] : ""}</td>
            <td>${level_map[f.level]}</td>
            <td>${teamAbbr}</td>
            <td>${getLeagueAbbr(f.league)}</td>
            <td class="align_right">${f.ip}</td>
            <td class="align_right">${f.era.toFixed(2)}</td>
            <td class="align_right">${f.fip.toFixed(2)}</td>
            <td class="align_right">${f.hrrate.toFixed(1)}</td>
            <td class="align_right">${f.bbperc.toFixed(1)}</td>
            <td class="align_right">${f.kperc.toFixed(1)}</td>
            <td class="align_right">${f.gorate.toFixed(1)}</td>
        `

        tr.dataset.year = f.year.toString()
        tr.dataset.type = f.month === null ? "year" : "month"

        if (isFirst)
        {
            tr.classList.add('row_first')
            
            let button_td = tr.getElementsByTagName('td')[0]
            let button = document.createElement('button')
            
            button.classList.add('table_button')
            
            if (f.month === null)
            {
                button.innerText = '+'
                button.classList.add('table_expand')
                
                button.addEventListener('click', () => {
                    tableUpdateCallback(stats_body, pcol_month, f.year.toString(), 'year')
                })
            } else {
                button.innerText = '-'
                button.classList.add('table_retract')
                
                button.addEventListener('click', () => {
                    tableUpdateCallback(stats_body, pcol_month, f.year.toString(), 'month')
                })
            }
            

            button_td.appendChild(button)
        }

        stats_body.append(tr)
    })
}

const WAR_BUCKETS = [0,0.5,3,7.5,15,25,35]
const WAR_LABELS = ["<=0", "0-1", "1-5", "5-10", "10-20", "20-30", "30+"]
const VALUE_BUCKETS = [0, 2.5, 12.5, 35, 75, 150, 250]
const VALUE_LABELS = ["<=0", "0M-5M", "5M-20M", "20M-50M", "50M-100M", "100M-200M", "200M+"]

function piePointGenerator(model : Model) : Point[]
{
    let points : Point[] = []
    for (let i = 0; i < WAR_LABELS.length; i++)
    {
        if (modelIsWar)
            points.push({y: model.probs[i], label:WAR_LABELS[i]})
        else
            points.push({y: model.probs[i], label:VALUE_LABELS[i]})
    }
    return points
}

function lineCallback(index : number, modelId : number)
{
    if (line_graph === null)
        return

    let model : Model
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
        
        if (modelIsWar)
            pie_graph.updateChart(model.probs, title_text, WAR_LABELS)
        else
            pie_graph.updateChart(model.probs, title_text, VALUE_LABELS)
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
            line_graph.setDataset(parseInt(graph_selector.value))
            line_graph.fireCallback()
        }
    })
}

function setupModel(hitterModels : Model[][], pitcherModels : Model[][]) : void
{
    let war_map = (f: Model, buckets : number[]) => {
        let war = 0;
        for (let i = 0; i < f.probs.length; i++)
            war += f.probs[i] * buckets[i];

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
    
    let hitter_war_points : Point[][] = []
    let pitcher_war_points : Point[][] = []
    
    for (var idx of MODEL_VALUES)
    {
        if (modelIsWar)
        {
            if (hitterModels.length > 0)
                hitter_war_points.push(hitterModels[idx - 1].map(f => war_map(f, WAR_BUCKETS)))
            if (pitcherModels.length > 0)
                pitcher_war_points.push(pitcherModels[idx - 1].map(f => war_map(f, WAR_BUCKETS)))
        }
            
        else
        {
            if (hitterModels.length > 0)
                hitter_war_points.push(hitterModels[idx - 1].map(f => war_map(f, VALUE_BUCKETS)))
            if (pitcherModels.length > 0)
                pitcher_war_points.push(pitcherModels[idx - 1].map(f => war_map(f, VALUE_BUCKETS)))
        }
    }

    let hitter_rank_points : Point[][] = hitterModels.map(m => m.filter(f => f.rank !== null).map(rank_map))
    let pitcher_rank_points : Point[][] = pitcherModels.map(m => m.filter(f => f.rank !== null).map(rank_map))

    
    const datasets = getDatasets(hitter_war_points, hitter_rank_points, pitcher_war_points, pitcher_rank_points)
    console.log(datasets)
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

let person : Person
let hitterModels : Model[][]
let pitcherModels : Model[][]

let modelIsWar : boolean = true

async function main()
{
    const datesJsonPromise = retrieveJson('../../assets/dates.json.gz')
    const id = getQueryParam("id")
    var player_data = fetch(`/player/${id}`)

    const player_search_data = retrieveJson('../../assets/player_search.json.gz')
    org_map = await retrieveJson("../../assets/map.json.gz")

    const pd = await (await player_data).json() as JsonObject
    person = getPerson(pd)

    // Include stats
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

    if (person.parentId !== null && person.parentId !== 0)
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

    // Update stats date
    const datesJson = await datesJsonPromise
    const endYear = datesJson["endYear"] as number
    const endMonth = datesJson["endMonth"] as number
    let hitter_title_element = getElementByIdStrict('hitter_stats_title')
    let pitcher_title_element = getElementByIdStrict('pitcher_stats_title')
    hitter_title_element.textContent = `Hitter Stats through ${MONTH_CODES[endMonth]} ${endYear}`
    pitcher_title_element.textContent = `Pitcher Stats through ${MONTH_CODES[endMonth]} ${endYear}`
}

main()