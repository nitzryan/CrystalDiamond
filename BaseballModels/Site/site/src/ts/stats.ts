let month : number
let year : number
let modelId : number
let teamId : number | null = null
let levelId : number

const statsTables = getElementByIdStrict('stats_table_holder')
const stat_hitter_btn = getElementByIdStrict('stat_hitter_btn')
const stat_pitcher_btn = getElementByIdStrict('stat_pitcher_btn')

function GetHitterPromise()
{
    if (teamId !== null)
    {
        return fetch(`/stats_hitter?year=${year}&month=${month}&model=${modelId}&level=${levelId}&team=${teamId}`)
    }
    else
    {
        return fetch(`/stats_hitter?year=${year}&month=${month}&model=${modelId}&level=${levelId}`)
    }
}

function GetPitcherPromise()
{
    if (teamId !== null)
    {
        return fetch(`/stats_pitcher?year=${year}&month=${month}&model=${modelId}&level=${levelId}&team=${teamId}`)
    }
    else
    {
        return fetch(`/stats_pitcher?year=${year}&month=${month}&model=${modelId}&level=${levelId}`)
    }
}

async function main()
{
    const datesJsonPromise = retrieveJson('../../assets/dates.json.gz')
    const player_search_data = retrieveJson('../../assets/player_search.json.gz')
    const org_map_promise = retrieveJson("../../assets/map.json.gz")
    
    const datesJson = await datesJsonPromise

    endYear = datesJson["endYear"] as number
    endMonth = datesJson["endMonth"] as number
    month = getQueryParamBackup('month', endMonth)
    year = getQueryParamBackup('year', endYear)
    modelId = getQueryParamBackup("model", 1)
    teamId = getQueryParamNullable('team')
    levelId = getQueryParamBackup('level', 0)

    const statsHitterPromise = GetHitterPromise()
    const statsPitcherPromise = GetPitcherPromise()
    org_map = await org_map_promise

    setupSelector({
        month : month,
        year : year,
        modelId : modelId,
        endYear : endYear,
        endMonth : endMonth,
        startYear : datesJson["startYear"] as number,
        startTeam : teamId,
        level : levelId
    })


    const statsHitter = await (await statsHitterPromise).json()
    const statsPitcher = await (await statsPitcherPromise).json()

    const lnk = 'link'
    const al_r = 'align_right'
    const al_l = 'align_left'
    const br = 'border_right'
    const bl = 'border_left'

    const hitterStatsViewer = new SortableStatsViewer(
        statsHitter,
        DB_Prediction_HitterStats,
        ["Name", "Org", "Age", "Position", "PA", "1B", "2B", "3B", "HR", "BB%", "K%", "SB", "CS", "AVG", "OBP", "SLG", "ISO", "wRC+", "OFF", "BSR", "DEF", "WAR"],
        f => [[`<a href='.player?id=${f.player.mlbId}'>${f.player.firstName + ' ' + f.player.lastName}</a>`, [al_l, lnk]], 
            [getParentAbbr(f.player.orgId), [al_l, lnk]], 
            [getDateDelta(new Date(f.player.birthYear, f.player.birthMonth, f.player.birthDate), new Date())[0].toString(), [al_r]], 
            [f.player.position, [al_l, br]],
            [f.obj.Pa.toFixed(0), [al_r]], [f.obj.Hit1B.toFixed(1), [al_r]], [f.obj.Hit2B.toFixed(1), [al_r]], [f.obj.Hit3B.toFixed(1), [al_r]], [f.obj.HitHR.toFixed(1), [al_r, br]], 
            [(f.obj.BB / f.obj.Pa * 100).toFixed(1), [al_r]], [(f.obj.K / f.obj.Pa * 100).toFixed(1), [al_r, br]], 
            [f.obj.SB.toFixed(1), [al_r]], [f.obj.CS.toFixed(1), [al_r, br]],
            [f.obj.AVG.toFixed(3), [al_r]], [f.obj.OBP.toFixed(3), [al_r]], [f.obj.SLG.toFixed(3), [al_r]], [f.obj.ISO.toFixed(3), [al_r, br]], 
            [f.obj.wRC.toFixed(0), [al_r]], [f.obj.crOFF.toFixed(1), [al_r]], [f.obj.crBSR.toFixed(1), [al_r]], [f.obj.crDEF.toFixed(1), [al_r]], [f.obj.crWAR.toFixed(1), [al_r]]]
    )
    statsTables.appendChild(hitterStatsViewer.baseElement)

    const pitcherStatsViewer = new SortableStatsViewer(
        statsPitcher,
        DB_Prediction_PitcherStats,
        ["Name", "Org", "Age", "IP", "G", "GS", "ERA", "FIP", "K/9", "BB/9", "HR/9", "RAA", "WAR"],
        f => [[`<a href='.player?id=${f.player.mlbId}'>${f.player.firstName + ' ' + f.player.lastName}</a>`, [al_l, lnk]], [getParentAbbr(f.player.orgId), [al_l, lnk]], 
            [getDateDelta(new Date(f.player.birthYear, f.player.birthMonth, f.player.birthDate), new Date())[0].toString(), [al_r, br]], 
            [formatOutsToIP(f.obj.Outs_RP + f.obj.Outs_SP),  [al_r]], [(f.obj.GS + f.obj.GR).toFixed(1), [al_r]], [f.obj.GS.toFixed(1), [al_r, br]],
            [f.obj.ERA.toFixed(2), [al_r]], [f.obj.FIP.toFixed(2), [al_r, br]],
            [(f.obj.K / (f.obj.Outs_RP + f.obj.Outs_SP) * 27).toFixed(1), [al_r]], [(f.obj.BB / (f.obj.Outs_RP + f.obj.Outs_SP) * 27).toFixed(1), [al_r]], [(f.obj.HR / (f.obj.Outs_RP + f.obj.Outs_SP) * 27).toFixed(1), [al_r, br]],
            [f.obj.crRAA.toFixed(1), [al_r]], [f.obj.crWAR.toFixed(1), [al_r]]
        ]
    )
    statsTables.appendChild(pitcherStatsViewer.baseElement)

    stat_hitter_btn.addEventListener('click', () => {
        stat_hitter_btn.classList.add('selected')
        stat_pitcher_btn.classList.remove('selected')
        hitterStatsViewer.baseElement.classList.remove('hidden')
        pitcherStatsViewer.baseElement.classList.add('hidden')
    })
    stat_pitcher_btn.addEventListener('click', () => {
        stat_hitter_btn.classList.remove('selected')
        stat_pitcher_btn.classList.add('selected')
        hitterStatsViewer.baseElement.classList.add('hidden')
        pitcherStatsViewer.baseElement.classList.remove('hidden')
    })
    stat_hitter_btn.click()

    searchBar = new SearchBar(await player_search_data)
    getElementByIdStrict('nav_stats').classList.add('selected')

    rankings_button.addEventListener('click', (event) => {
        const mnth = month_select.value
        const yr = year_select.value
        const model = model_select.value
        const level = level_select?.value as string
        window.location.href = `./stats?year=${yr}&month=${mnth}&model=${model}&level=${level}`
    })
}

main()