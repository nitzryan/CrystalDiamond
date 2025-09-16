"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g = Object.create((typeof Iterator === "function" ? Iterator : Object).prototype);
    return g.next = verb(0), g["throw"] = verb(1), g["return"] = verb(2), typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (g && (g = 0, op[0] && (_ = 0)), _) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
var homeDataContainer = getElementByIdStrict("homeDataContainer");
function getHomeData(hd) {
    return hd.map(function (f) {
        f = f;
        return {
            mlbId: f["mlbId"],
            data: f["data"],
            rank: f["rank"],
            name: f["firstName"] + " " + f["lastName"],
            position: f["position"],
            orgId: f["orgId"]
        };
    }).sort(function (a, b) { return a.rank - b.rank; });
}
function createHomeDataElements(home_data) {
    var hd = home_data["data"];
    var hdt = home_data["types"];
    var _loop_1 = function () {
        type = type;
        type_div = document.createElement('div');
        header = document.createElement('h2');
        header.innerText = type["name"];
        type_div.appendChild(header);
        list = document.createElement('ol');
        var current_type = type["type"];
        var current_hd = hd.filter(function (f) { return f["rankType"] == current_type; });
        var hd_array = getHomeData(current_hd);
        hd_array.forEach(function (f) {
            var li = document.createElement('li');
            li.innerHTML =
                "\n            <div class='rankings_item'>\n                <div class='rankings_row'>\n                    <div class='rankings_name'><a href='./player?id=".concat(f.mlbId, "'>").concat(f.name, "</a></div>\n                    <div class='rankings_rightrow'>\n                        <div><a href='./teams?id=").concat(f.orgId, "'>").concat(getParentAbbr(f.orgId), "</a></div>\n                    </div>\n                </div>\n                <div class='rankings_row'>\n                    <div>").concat(f.data, "</div>\n                    <div class='rankings_rightrow'>\n                        <div>").concat(f.position, "</div>\n                    </div>\n                </div>\n            </div>\n            ");
            list.appendChild(li);
        });
        type_div.append(list);
        homeDataContainer.appendChild(type_div);
    };
    var type_div, header, list;
    for (var _i = 0, hdt_1 = hdt; _i < hdt_1.length; _i++) {
        var type = hdt_1[_i];
        _loop_1();
    }
}
function main() {
    return __awaiter(this, void 0, void 0, function () {
        var datesJsonPromise, player_search_data, org_map_promise, datesJson, home_data_response, home_data, _a;
        return __generator(this, function (_b) {
            switch (_b.label) {
                case 0:
                    datesJsonPromise = retrieveJson('/assets/dates.json.gz');
                    player_search_data = retrieveJson('/assets/player_search.json.gz');
                    org_map_promise = retrieveJson("/assets/map.json.gz");
                    return [4, datesJsonPromise];
                case 1:
                    datesJson = _b.sent();
                    home_data_response = fetch("/homedata?year=".concat(datesJson["endYear"], "&month=").concat(datesJson["endMonth"]));
                    return [4, home_data_response];
                case 2: return [4, (_b.sent()).json()];
                case 3:
                    home_data = _b.sent();
                    return [4, org_map_promise];
                case 4:
                    org_map = _b.sent();
                    createHomeDataElements(home_data);
                    _a = SearchBar.bind;
                    return [4, player_search_data];
                case 5:
                    searchBar = new (_a.apply(SearchBar, [void 0, _b.sent()]))();
                    getElementByIdStrict('nav_home').classList.add('selected');
                    return [2];
            }
        });
    });
}
main();
var searchBar = null;
var SearchBar = (function () {
    function SearchBar(json) {
        var _this = this;
        this.searchBox = getElementByIdStrict('searchBar');
        this.majorsSearchResults = getElementByIdStrict('majorsSearchResults');
        this.minorsSearchResults = getElementByIdStrict('minorsSearchResults');
        this.searchResultsContainer = getElementByIdStrict('searchResultsContainer');
        function removeAccents(str) {
            return str.normalize("NFD").replace(/[\u0300-\u036f]/g, "");
        }
        var players = json["players"];
        players.forEach(function (f) {
            f["fstLoc"] = removeAccents(f["f"]).toLowerCase();
            f["lstLoc"] = removeAccents(f["l"]).toLowerCase();
        });
        this.mlbItems = players.filter(function (f) { return (f["s"] & 2) === 2; });
        this.milbItems = players.filter(function (f) { return (f["s"] & 2) === 0; });
        this.current_count = 0;
        this.searchBox.addEventListener('input', function (event) {
            _this.current_count++;
            var idx = _this.current_count;
            var search_str = _this.searchBox.value;
            var resultsPair = search_str == "" ? null : _this.getResults(search_str);
            if (idx != _this.current_count)
                return;
            if (resultsPair !== null) {
                _this.searchResultsContainer.classList.remove('hidden');
                _this.majorsSearchResults.innerHTML = resultsPair[0];
                _this.minorsSearchResults.innerHTML = resultsPair[1];
            }
            else {
                _this.searchResultsContainer.classList.add('hidden');
                _this.majorsSearchResults.innerHTML = '';
                _this.minorsSearchResults.innerHTML = '';
            }
        });
    }
    SearchBar.prototype.getResults = function (text) {
        text = text.toLowerCase();
        var filterFunction = function (f) {
            var first = f["fstLoc"];
            var last = f["lstLoc"];
            return first.startsWith(text)
                || last.startsWith(text)
                || (first + " " + last).startsWith(text)
                || (last + " " + first).startsWith(text);
        };
        var sortFunction = function (a, b) {
            var a_s = a["s"];
            var b_s = b["s"];
            if (a_s != b_s)
                return b_s - a_s;
            var r = a["f"].localeCompare(b["f"]);
            return r !== 0 ? r : a["l"].localeCompare(b["l"]);
        };
        var validMLB = this.mlbItems.filter(filterFunction).sort(sortFunction);
        var validMilb = this.milbItems.filter(filterFunction).sort(sortFunction);
        if (validMLB.length == 0 && validMilb.length == 0)
            return null;
        var elementMap = function (f) {
            var id = f["o"];
            var teamString = id > 0 ? getParentAbbr(id) : "";
            return "<li><a href=\"./player?id=".concat(f["id"], "\">").concat(f["f"][0].toUpperCase() + f["f"].substring(1), " ").concat(f["l"][0].toUpperCase() + f["l"].substring(1), "</a><div class=\"teamsearch team").concat(id, "\">").concat(teamString, "</div></li>");
        };
        var htmlMajorsStrings = validMLB.map(elementMap).join("\n");
        var htmlMinorsStrings = validMilb.map(elementMap).join("\n");
        return [htmlMajorsStrings, htmlMinorsStrings];
    };
    return SearchBar;
}());
var CSS_Style = (function () {
    function CSS_Style() {
        var style = window.getComputedStyle(document.body);
        this.background_high = style.getPropertyValue('--background-high');
        this.background_med = style.getPropertyValue('--background-med');
        this.background_low = style.getPropertyValue('--background-low');
        this.text_high = style.getPropertyValue('--text-high');
        this.text_low = style.getPropertyValue('--text-low');
    }
    return CSS_Style;
}());
var css = new CSS_Style();
function getJsonString(obj, key) {
    var value = obj[key];
    if (value === undefined) {
        throw new Error("Property \"".concat(key, "\" is missing"));
    }
    if (typeof value === "string") {
        return value;
    }
    throw new Error("Property \"".concat(key, "\" is type ").concat(typeof value, ", string expected"));
}
function getJsonNumber(obj, key) {
    var value = obj[key];
    if (value === undefined) {
        throw new Error("Property \"".concat(key, "\" is missing"));
    }
    if (typeof value === "number") {
        return value;
    }
    throw new Error("Property \"".concat(key, "\" is type ").concat(typeof value, ", number expected"));
}
function getJsonArray(obj, key) {
    var value = obj[key];
    if (value === undefined) {
        throw new Error("Property \"".concat(key, "\" is missing"));
    }
    if (Array.isArray(value)) {
        return value;
    }
    throw new Error("Property \"".concat(key, "\" is type ").concat(typeof value, ", number expected"));
}
function getJsonNumberNullable(obj, key) {
    var value = obj[key];
    if (value === undefined) {
        return null;
    }
    if (typeof value === "number") {
        return value;
    }
    throw new Error("Property \"".concat(key, "\" is type ").concat(typeof value, ", number expected"));
}
function getElementByIdStrict(id) {
    var element = document.getElementById(id);
    if (element !== null)
        return element;
    throw new Error("HTMLElement ".concat(id, " not found"));
}
function updateElementText(id, text) {
    var element = getElementByIdStrict(id);
    element.innerText = text;
}
function getDateDelta(start, end) {
    var years = end.getFullYear() - start.getFullYear();
    var months = end.getMonth() - start.getMonth();
    var days = end.getDate() - start.getDate();
    if (days < 0) {
        months--;
        var tmp = new Date(end.getFullYear(), end.getMonth(), 0).getDate();
        days += tmp;
    }
    if (months < 0) {
        months += 12;
        years--;
    }
    return [years, months, days];
}
function getQueryParam(name) {
    var params = new URLSearchParams(window.location.search);
    var value = params.get(name);
    if (value === null)
        throw new Error("Unable to get query parameter ".concat(name));
    return Number(value);
}
function retrieveJsonNullable(filename) {
    return __awaiter(this, void 0, void 0, function () {
        var response, compressedData, stream, data, text, json;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4, fetch(filename)];
                case 1:
                    response = _a.sent();
                    if (response.status !== 200) {
                        return [2, null];
                    }
                    compressedData = response.body;
                    stream = new DecompressionStream('gzip');
                    data = compressedData === null || compressedData === void 0 ? void 0 : compressedData.pipeThrough(stream);
                    return [4, new Response(data).text()];
                case 2:
                    text = _a.sent();
                    json = JSON.parse(text);
                    return [2, json];
            }
        });
    });
}
function retrieveJson(filename) {
    return __awaiter(this, void 0, void 0, function () {
        var json;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4, retrieveJsonNullable(filename)];
                case 1:
                    json = _a.sent();
                    if (json !== null)
                        return [2, json];
                    throw new Error("Failed to retrieve Json ".concat(filename));
            }
        });
    });
}
function getParentId(id, year) {
    if (org_map === null)
        throw new Error("Org map null accessing getParentId");
    var parents = org_map["parents"];
    if (id in parents)
        return id;
    var children = org_map["children"];
    var child = children[id];
    var parentArray = child["parents"];
    for (var _i = 0, parentArray_1 = parentArray; _i < parentArray_1.length; _i++) {
        var parent = parentArray_1[_i];
        parent = parent;
        var y = parent["year"];
        if (y == year)
            return parent["parent"];
    }
    throw new Error("No parentId for id=".concat(id, " year=").concat(year));
}
function getTeamAbbr(id, year) {
    if (org_map === null)
        throw new Error("Org map null accessing getTeamAbbr");
    var parentId = getParentId(id, year);
    var parents = org_map["parents"];
    var parent = parents[parentId];
    return parent["abbr"];
}
function getParentAbbr(id) {
    if (org_map === null)
        throw new Error("Org map null accessing getParentAbbr");
    var parents = org_map["parents"];
    var parent = parents[id];
    return parent["abbr"];
}
function getParentName(id) {
    if (org_map === null)
        throw new Error("Org map null accessing getParentAbbr");
    var parents = org_map["parents"];
    var parent = parents[id];
    return parent["name"];
}
function getLeagueAbbr(id) {
    if (org_map === null)
        throw new Error("Org map null accessing getLeagueAbbr");
    var leagues = org_map["leagues"];
    if (id in leagues) {
        var league = leagues[id];
        return league["abbr"];
    }
    throw new Error("No League found for ".concat(id));
}
function getOrdinalNumber(num) {
    var lastDigit = num % 10;
    var last2Digits = num % 100;
    if (lastDigit === 1 && last2Digits !== 11)
        return num + "st";
    if (lastDigit === 2 && last2Digits !== 12)
        return num + "nd";
    if (lastDigit === 3 && last2Digits !== 13)
        return num + "rd";
    return num + "th";
}
var org_map = null;
var level_map = { 1: "MLB", 11: "AAA", 12: "AA", 13: "A+", 14: "A", 15: "A-", 16: "Rk", 17: "DSL", 20: "" };
var MONTH_CODES = ["", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dev"];
