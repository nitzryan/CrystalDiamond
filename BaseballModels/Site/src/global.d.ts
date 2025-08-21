declare type JsonValue = string | number | boolean | null | JsonArray | JsonObject;
declare type JsonArray = JsonValue[];
declare type JsonObject = { [key: string]: JsonValue };
declare function getJsonString(obj : JsonObject, key: string) : string;
declare function getJsonNumber(obj : JsonObject, key: string) : number;
declare function getJsonArray(obj : JsonObject, key : string) : JsonArray;
declare function getJsonNumberNullable(obj : JsonObject, key: string) : number | null;
declare function getElementByIdStrict(id : string) : HTMLElement
declare function updateElementText(id: string, text : string) : void
declare function getDateDelta(start : Date, end : Date) : number[]
declare function getQueryParam(name : string) : number
declare function retrieveJsonNullable(filename : string) : Promise<JsonObject | null>
declare function retrieveJson(filename : string) : Promise<JsonObject>
declare function getTeamAbbr(id : number, year : number) : string
declare function getLeagueAbbr(id : number) : string 