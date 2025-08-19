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

