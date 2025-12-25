function getJsonString(obj : JsonObject, key: string) : string
{
    const value = obj[key];
    if (value === undefined) {
        throw new Error(`Property "${key}" is missing`);
    }
    if (typeof value === "string")
    {
        return value;
    }
    throw new Error(`Property "${key}" is type ${typeof value}, string expected`);
}

function getJsonNumber(obj : JsonObject, key: string) : number
{
    const value = obj[key];
    if (value === undefined) {
        throw new Error(`Property "${key}" is missing`);
    }
    if (typeof value === "number")
    {
        return value;
    }
    throw new Error(`Property "${key}" is type ${typeof value}, number expected`);
}

function getJsonArray(obj : JsonObject, key : string) : JsonArray
{
    const value = obj[key];
    if (value === undefined) {
        throw new Error(`Property "${key}" is missing`);
    }
    if (Array.isArray(value))
    {
        return value;
    }
    throw new Error(`Property "${key}" is type ${typeof value}, number expected`);
}

function getJsonNumberNullable(obj : JsonObject, key: string) : number | null
{
    const value = obj[key];
    if (value === undefined) {
        return null;
    }
    if (typeof value === "number")
    {
        return value;
    }
    throw new Error(`Property "${key}" is type ${typeof value}, number expected`);
}

function getElementByIdStrict(id : string) : HTMLElement
{
    const element = document.getElementById(id)
    if (element !== null)
        return element;

    throw new Error(`HTMLElement ${id} not found`)
}

function updateElementText(id: string, text : string) : void
{
    const element = getElementByIdStrict(id)
    element.innerText = text;
}

function getDateDelta(start : Date, end : Date) : number[]
{
    let years = end.getFullYear() - start.getFullYear()
    let months = end.getMonth() - start.getMonth()
    let days = end.getDate() - start.getDate()

    if (days < 0)
    {
        months--;
        const tmp = new Date(end.getFullYear(), end.getMonth(), 0).getDate()
        days += tmp
    }
    if (months < 0)
    {
        months += 12
        years--;
    }

    return [years, months, days]
}

function getQueryParam(name : string) : number
{
    const params = new URLSearchParams(window.location.search);
    const value = params.get(name)
    if (value === null)
        throw new Error(`Unable to get query parameter ${name}`);
        
    return Number(value);
}

function getQueryParamNullable(name : string) : number | null
{
    const params = new URLSearchParams(window.location.search);
    const value = params.get(name)
    if (value === null)
        return null
        
    return Number(value);
}

function getQueryParamBackup(name : string, backup : number) : number
{
    try {
        return getQueryParam(name)
    } catch (_)
    {
        return backup
    }
}

function getQueryParamBackupStr(name : string, backup : string) : string
{
    const params = new URLSearchParams(window.location.search);
    const value = params.get(name)
    if (value === null)
        return backup

    return value
}

async function retrieveJsonNullable(filename : string) : Promise<JsonObject | null>
{
    const response = await fetch(filename)
    if (response.status !== 200)
    {
        return null;
    }
    
    const compressedData = response.body
    const stream = new DecompressionStream('gzip')
    const data = compressedData?.pipeThrough(stream)

    const text = await new Response(data).text()
    const json = JSON.parse(text)

    return json as JsonObject;
}

async function retrieveJson(filename : string) : Promise<JsonObject>
{
    const json = await retrieveJsonNullable(filename)
    if (json !== null)
        return json;

    throw new Error(`Failed to retrieve Json ${filename}`)
}

function getParentId(id : number, year : number) : number
{
    if (org_map === null)
        throw new Error("Org map null accessing getParentId")

    // Check parents
    const parents = org_map["parents"] as JsonObject
    if (id in parents)
        return id

    // Parse through children
    const children = org_map["children"] as JsonObject
    const child = children[id] as JsonObject
    const parentArray = child["parents"] as JsonArray
    for (var parent of parentArray)
    {
        parent = parent as JsonObject
        const y = parent["year"] as number
        if (y == year)
            return parent["parent"] as number
    }

    throw new Error(`No parentId for id=${id} year=${year}`)
}

function getTeamAbbr(id : number, year : number) : string
{
    if (org_map === null)
        throw new Error("Org map null accessing getTeamAbbr")
    const parentId = getParentId(id, year)

    const parents = org_map["parents"] as JsonObject
    const parent = parents[parentId] as JsonObject
    return parent["abbr"] as string
}

function getParentAbbr(id: number) : string
{
    if (org_map === null)
        throw new Error("Org map null accessing getParentAbbr")

    if (id === 0)
        return "FA"

    const parents = org_map["parents"] as JsonObject
    const parent = parents[id] as JsonObject
    return parent["abbr"] as string
}

function getParentAbbrFallback(id : number, fallback : string) : string
{
    try {
        return getParentAbbr(id)
    } catch (_)
    {
        return fallback
    }
}

function getParentName(id : number) : string
{
    if (org_map === null)
        throw new Error("Org map null accessing getParentAbbr")

    const parents = org_map["parents"] as JsonObject
    const parent = parents[id] as JsonObject
    return parent["name"] as string
}

function getLeagueAbbr(id : number) : string
{
    if (org_map === null)
        throw new Error("Org map null accessing getLeagueAbbr")
    
    const leagues = org_map["leagues"] as JsonObject
    if (id in leagues)
    {
        const league = leagues[id] as JsonObject
        return league["abbr"] as string
    }

    throw new Error(`No League found for ${id}`)
}

function getOrdinalNumber(num : number) : string
{
    let lastDigit = num % 10
    let last2Digits = num % 100
    
    if (lastDigit === 1 && last2Digits !== 11)
        return num + "st"
    if (lastDigit === 2 && last2Digits !== 12)
        return num + "nd"
    if (lastDigit === 3 && last2Digits !== 13)
        return num + "rd"
    return num + "th"
}

function formatOutsToIP(outs : number) : string
{
    const ip = outs / 3
    const full_ip = Math.trunc(ip)
    const partial_ip = Math.trunc((ip - full_ip) * 3)
    return full_ip.toFixed(0) + "." + partial_ip.toFixed(0)
}

function formatModelString(val : number) : string
{
    return `${val.toFixed(1)}`
}
const MODEL_VALUES = [1,2,3]
const MODEL_STRINGS = ["Base","Stats Only", "Experimental"]

var org_map : JsonObject | null = null
const level_map : JsonObject = {1:"MLB",11:"AAA",12:"AA",13:"A+",14:"A",15:"A-",16:"Rk",17:"DSL",20:""}
const level_map2 : string[] = ["MLB", "AAA", "AA", "A+", "A", "A-", "Rk", "DSL"]
const MONTH_CODES : string[] = ["", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"]