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