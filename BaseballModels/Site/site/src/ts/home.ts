let homeDataContainer = getElementByIdStrict("homeDataContainer")

type HomeData = {
    data : DB_HomeData
    player : DB_Player
}

function getHomeData(hd : JsonArray) : Array<HomeData>
{
    return hd.map(f => {
        
        f = f as JsonObject
        return {
            data : new DB_HomeData(f),
            player : new DB_Player(f)
        } as HomeData
    }).sort((a,b) => a.data.rank - b.data.rank)
}

function createHomeDataElements(home_data : JsonObject)
{
    const hd = getHomeData(home_data["data"] as JsonArray)
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
        const hd_array = hd.filter(f => f.data.rankType == current_type)
        
        hd_array.forEach(f => {
            let li = document.createElement('li')
            li.innerHTML = 
            `
            <div class='rankings_item'>
                <div class='rankings_row'>
                    <div class='rankings_name'><a href='./player?id=${f.player.mlbId}'>${f.player.firstName + " " + f.player.lastName}</a></div>
                    <div class='rankings_rightrow'>
                        <div><a href='./teams?id=${f.player.orgId}'>${getParentAbbrFallback(f.player.orgId, "")}</a></div>
                    </div>
                </div>
                <div class='rankings_row'>
                    <div>${f.data.data}</div>
                    <div class='rankings_rightrow'>
                        <div>${f.player.position}</div>
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
    await assetLoader.ready;

    const dates = await assetLoader.dates();
    endYear = dates.endYear;
    endMonth = dates.endMonth;
    const year = getQueryParamBackup("year", endYear)
    const month = getQueryParamBackup("month", endMonth)
    const modelId = getQueryParamBackup("model", 1)

    const home_data_response = fetch(`/homedata?year=${year}&month=${month}&model=${modelId}`)
    const home_data = await(await home_data_response).json() as JsonObject
    createHomeDataElements(home_data)
    
    setupSelector({
        month : month,
        year : year,
        modelId : modelId,
        endYear : endYear,
        endMonth : endMonth,
        startYear : dates.startYear,
        startTeam : null,
        level : null
    })

    getElementByIdStrict('nav_home').classList.add('selected')

    rankings_button.addEventListener('click', (event) => {
        const mnth = month_select.value
        const yr = year_select.value
        const model = model_select.value
        
        window.location.href = `./?year=${yr}&month=${mnth}&model=${model}`
    })
}

main()