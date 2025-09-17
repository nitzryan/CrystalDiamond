let homeDataContainer = getElementByIdStrict("homeDataContainer")

type HomeData = {
    mlbId : number,
    name : string,
    position : string,
    orgId : number,
    data : string,
    rank : number
}

function getHomeData(hd : JsonArray) : Array<HomeData>
{
    return hd.map(f => {
        f = f as JsonObject
        return {
            mlbId : f["mlbId"],
            data : f["data"],
            rank : f["rank"],
            name : f["firstName"] + " " + f["lastName"],
            position : f["position"],
            orgId : f["orgId"]
        } as HomeData
    }).sort((a,b) => a.rank - b.rank)
}

function createHomeDataElements(home_data : JsonObject)
{
    const hd = home_data["data"] as JsonArray
    const hdt = home_data["types"] as JsonArray

    for (var type of hdt)
    {
        type = type as JsonObject
        var type_div = document.createElement('div')
        var header = document.createElement('h2')
        header.innerText = type["name"] as string
        type_div.appendChild(header)
        
        var list = document.createElement('ol')
        const current_type = type["type"] as number
        const current_hd = hd.filter(f => (f as JsonObject)["rankType"] == current_type)
        const hd_array = getHomeData(current_hd)
        hd_array.forEach(f => {
            let li = document.createElement('li')
            li.innerHTML = 
            `
            <div class='rankings_item'>
                <div class='rankings_row'>
                    <div class='rankings_name'><a href='./player?id=${f.mlbId}'>${f.name}</a></div>
                    <div class='rankings_rightrow'>
                        <div><a href='./teams?id=${f.orgId}'>${getParentAbbrFallback(f.orgId, "")}</a></div>
                    </div>
                </div>
                <div class='rankings_row'>
                    <div>${f.data}</div>
                    <div class='rankings_rightrow'>
                        <div>${f.position}</div>
                    </div>
                </div>
            </div>
            `
            list.appendChild(li)
        })

        type_div.append(list)
        homeDataContainer.appendChild(type_div)
    }
}

async function main()
{
    const datesJsonPromise = retrieveJson('/assets/dates.json.gz')
    const player_search_data = retrieveJson('/assets/player_search.json.gz')
    const org_map_promise = retrieveJson("/assets/map.json.gz")
    
    const datesJson = await datesJsonPromise
    const endYear = datesJson["endYear"] as number
    const endMonth = datesJson["endMonth"] as number
    const year = getQueryParamBackup("year", endYear)
    const month = getQueryParamBackup("month", endMonth)

    const home_data_response = fetch(`/homedata?year=${year}&month=${month}`)
    const home_data = await(await home_data_response).json() as JsonObject
    org_map = await org_map_promise
    createHomeDataElements(home_data)
    
    
    setupSelector({
        month : month,
        year : year,
        endYear : endYear,
        endMonth : endMonth,
        startYear : datesJson["startYear"] as number,
        startTeam : null
    })

    searchBar = new SearchBar(await player_search_data)
    getElementByIdStrict('nav_home').classList.add('selected')

    rankings_button.addEventListener('click', (event) => {
        const mnth = month_select.value
        const yr = year_select.value
        
        window.location.href = `./?year=${yr}&month=${mnth}`
    })
}

main()