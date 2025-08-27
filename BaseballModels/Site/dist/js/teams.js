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
var searchBar = null;
var month = null;
var year = null;
var teamId = null;
function main() {
    return __awaiter(this, void 0, void 0, function () {
        var datesJsonPromise, player_search_data, datesJson, selector, rankingJson, _a;
        return __generator(this, function (_b) {
            switch (_b.label) {
                case 0:
                    datesJsonPromise = retrieveJson('../../assets/ranking/dates.json.gz');
                    try {
                        month = getQueryParam('month');
                        year = getQueryParam('year');
                        teamId = getQueryParam('team');
                    }
                    catch (error) { }
                    player_search_data = retrieveJson('../../assets/player_search.json.gz');
                    return [4, datesJsonPromise];
                case 1:
                    datesJson = _b.sent();
                    endYear = datesJson["endYear"];
                    endMonth = datesJson["endMonth"];
                    if (month === null || year === null) {
                        month = endMonth;
                        year = endYear;
                    }
                    if (teamId === null)
                        teamId = 142;
                    return [4, retrieveJson("../../assets/map.json.gz")];
                case 2:
                    org_map = _b.sent();
                    selector = setupSelector({
                        month: month,
                        year: year,
                        endYear: endYear,
                        endMonth: endMonth,
                        startYear: datesJson["startYear"],
                        startTeam: teamId
                    });
                    return [4, retrieveJson("../../assets/ranking/teams/".concat(teamId, "-").concat(month, "-").concat(year, ".json.gz"))];
                case 3:
                    rankingJson = _b.sent();
                    setupRankings(rankingJson, month, year, teamId);
                    _a = SearchBar.bind;
                    return [4, player_search_data];
                case 4:
                    searchBar = new (_a.apply(SearchBar, [void 0, _b.sent()]))();
                    return [4, selector];
                case 5:
                    _b.sent();
                    rankings_button.addEventListener('click', function (event) {
                        var mnth = month_select.value;
                        var yr = year_select.value;
                        var tm = team_select === null || team_select === void 0 ? void 0 : team_select.value;
                        window.location.href = "./teams.html?year=".concat(yr, "&month=").concat(mnth, "&team=").concat(tm);
                    });
                    return [2];
            }
        });
    });
}
main();
var rankings_selector = getElementByIdStrict('ranking_selector');
var year_select = getElementByIdStrict('year_select');
var month_select = getElementByIdStrict('month_select');
var team_select = document.getElementById('team_select');
var rankings_button = getElementByIdStrict('rankings_button');
var rankings_error = getElementByIdStrict('rankings_error');
var rankings_header = getElementByIdStrict('rankings_header');
var rankings_list = getElementByIdStrict('rankings_list');
var rankings_load = getElementByIdStrict('rankings_load');
var endYear = 0;
var endMonth = 0;
var PlayerLoader = (function () {
    function PlayerLoader(ps) {
        this.numElements = 100;
        this.players = ps;
        this.index = 0;
    }
    PlayerLoader.prototype.getListElements = function () {
        var elements = [];
        for (var i = this.index; i < this.index + this.numElements; i++) {
            if (i >= this.players.length) {
                rankings_load.classList.add('hidden');
                break;
            }
            var player = this.players[i];
            var element = document.createElement('li');
            var teamAbbr = player.team == 0 ? "" : getParentAbbr(player.team);
            element.innerHTML = "<div><a href='./player.html?id=".concat(player.id, "'>").concat(player.name, "</a><div><div class='war'>").concat(player.war.toFixed(1), " WAR</div><div class='team").concat(player.team, "'>").concat(teamAbbr, "</span></div></div>");
            elements.push(element);
        }
        this.index += this.numElements;
        return elements;
    };
    return PlayerLoader;
}());
function selectorEventHandler(ev) {
    var selectedMonth = parseInt(month_select.value);
    var selectedYear = parseInt(year_select.value);
    if (endYear == selectedYear && endMonth < selectedMonth) {
        rankings_button.classList.add('hidden');
        rankings_error.classList.remove('hidden');
    }
    else {
        rankings_error.classList.add('hidden');
        rankings_button.classList.remove('hidden');
    }
}
function setupTeamSelector(teamId) {
    if (org_map === null)
        throw new Error("org_map null at setupSelector");
    if (team_select === null)
        throw new Error('team_select null in setupTeamSelector');
    var parents = org_map["parents"];
    var teams = [];
    for (var id in parents) {
        teams.push({
            id: parseInt(id),
            abbr: parents[id]['abbr']
        });
    }
    teams.sort(function (a, b) {
        return a.abbr.localeCompare(b.abbr);
    });
    var elements = teams.map(function (f) {
        var el = document.createElement('option');
        el.value = f.id.toString();
        el.innerText = f.abbr;
        return el;
    });
    for (var _i = 0, elements_1 = elements; _i < elements_1.length; _i++) {
        var el = elements_1[_i];
        team_select.appendChild(el);
    }
    team_select.value = teamId.toString();
}
function setupSelector(args) {
    return __awaiter(this, void 0, void 0, function () {
        var i, opt, i, opt;
        return __generator(this, function (_a) {
            for (i = args.startYear; i <= endYear; i++) {
                opt = document.createElement('option');
                opt.value = i.toString();
                opt.innerText = i.toString();
                year_select.appendChild(opt);
            }
            for (i = 4; i <= 9; i++) {
                opt = document.createElement('option');
                opt.value = i.toString();
                opt.innerText = MONTH_CODES[i];
                month_select.appendChild(opt);
            }
            year_select.value = args.year.toString();
            month_select.value = args.month.toString();
            year_select.addEventListener('change', selectorEventHandler);
            month_select.addEventListener('change', selectorEventHandler);
            if (team_select !== null && args.startTeam !== null) {
                setupTeamSelector(args.startTeam);
                team_select.addEventListener('change', selectorEventHandler);
            }
            return [2];
        });
    });
}
function getPlayers(rankingJson) {
    var jsonArray = rankingJson["players"];
    return jsonArray.map(function (f) {
        f = f;
        var player = {
            name: getJsonString(f, "name"),
            war: getJsonNumber(f, "war"),
            id: getJsonNumber(f, "id"),
            model: getJsonString(f, "model"),
            team: getJsonNumber(f, "team")
        };
        return player;
    });
}
function setupRankings(obj, month, year, team) {
    var players = getPlayers(obj);
    var playerLoader = new PlayerLoader(players);
    if (team === null)
        rankings_header.innerText = "Rankings for ".concat(MONTH_CODES[month], " ").concat(year);
    else
        rankings_header.innerText = "Rankings for ".concat(org_map["parents"][team]["name"], " ").concat(MONTH_CODES[month], " ").concat(year);
    rankings_load.addEventListener('click', function (event) {
        var elements = playerLoader.getListElements();
        for (var _i = 0, elements_2 = elements; _i < elements_2.length; _i++) {
            var el = elements_2[_i];
            rankings_list.appendChild(el);
        }
    });
    rankings_load.dispatchEvent(new Event('click'));
}
var SearchBar = (function () {
    function SearchBar(json) {
        var _this = this;
        this.searchBox = getElementByIdStrict('searchBar');
        this.searchResults = getElementByIdStrict('searchResults');
        this.items = json["players"];
        this.current_count = 0;
        this.searchBox.addEventListener('input', function (event) {
            _this.current_count++;
            var idx = _this.current_count;
            var search_str = _this.searchBox.value;
            var results = search_str == "" ? "" : _this.getResults(search_str);
            if (idx != _this.current_count)
                return;
            _this.searchResults.innerHTML = results;
            if (results.length > 0)
                _this.searchResults.classList.remove('hidden');
            else
                _this.searchResults.classList.add('hidden');
        });
    }
    SearchBar.prototype.getResults = function (text) {
        text = text.toLowerCase();
        var valid = this.items.filter(function (f) {
            var first = f["f"].toLowerCase();
            var last = f["l"].toLowerCase();
            return first.includes(text)
                || last.includes(text)
                || (first + " " + last).includes(text)
                || (last + " " + first).includes(text);
        }).sort(function (a, b) {
            var r = a["f"].localeCompare(b["f"]);
            return r !== 0 ? r : a["l"].localeCompare(b["l"]);
        });
        var htmlStrings = valid.map(function (f) {
            var id = f["o"];
            var teamString = id > 0 ? "  (" + getParentAbbr(id) + ")" : "";
            return "<li><a href=\"./player.html?id=".concat(f["id"], "\">").concat(f["f"][0].toUpperCase() + f["f"].substring(1), " ").concat(f["l"][0].toUpperCase() + f["l"].substring(1), "</a>").concat(teamString, "</li>");
        });
        return htmlStrings.join("\n");
    };
    return SearchBar;
}());
var KeyControls = (function () {
    function KeyControls(document, callback) {
        this.callback = callback;
        document.addEventListener('keydown', function (event) {
            if (event.key === "ArrowLeft")
                callback(-1);
            else if (event.key === "ArrowRight")
                callback(1);
        });
    }
    return KeyControls;
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
var org_map = null;
var level_map = { 1: "MLB", 11: "AAA", 12: "AA", 13: "A+", 14: "A", 15: "A-", 16: "Rk", 17: "DSL" };
var MONTH_CODES = ["", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dev"];
