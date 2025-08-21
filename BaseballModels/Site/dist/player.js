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
function getHitterStats(hitterObject) {
    var stats = [];
    var statsArray = getJsonArray(hitterObject, "stats");
    statsArray.forEach(function (f) {
        var fObj = f;
        var hs = {
            level: getJsonNumber(fObj, "level"),
            year: getJsonNumber(fObj, "year"),
            team: getJsonNumber(fObj, "team"),
            league: getJsonNumber(fObj, "league"),
            pa: getJsonNumber(fObj, "PA"),
            avg: getJsonNumber(fObj, "AVG"),
            obp: getJsonNumber(fObj, "OBP"),
            slg: getJsonNumber(fObj, "SLG"),
            iso: getJsonNumber(fObj, "ISO"),
            wrc: getJsonNumber(fObj, "wrc"),
            hr: getJsonNumber(fObj, "HR"),
            bbPerc: getJsonNumber(fObj, "BB%"),
            kPerc: getJsonNumber(fObj, "K%"),
            sb: getJsonNumber(fObj, "SB"),
            cs: getJsonNumber(fObj, "CS")
        };
        stats.push(hs);
    });
    return stats;
}
function getHitterModels(hitterObject) {
    var models = [];
    var modelArray = getJsonArray(hitterObject, "model");
    modelArray.forEach(function (f) {
        var fObj = f;
        var probArray = getJsonArray(fObj, "probs");
        var m = {
            year: getJsonNumber(fObj, "year"),
            month: getJsonNumber(fObj, "month"),
            probs: probArray.map(function (f) {
                var num = f;
                return num;
            })
        };
        models.push(m);
    });
    return models;
}
function loadHitter(id) {
    return __awaiter(this, void 0, void 0, function () {
        var hitterObject, hitter_1;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4, retrieveJsonNullable("../../assets/player/h".concat(id, ".json.gz"))];
                case 1:
                    hitterObject = _a.sent();
                    if (hitterObject !== null) {
                        hitter_1 = {
                            firstName: getJsonString(hitterObject, "firstName"),
                            lastName: getJsonString(hitterObject, "lastName"),
                            birthDate: new Date(getJsonNumber(hitterObject, "birthYear"), getJsonNumber(hitterObject, "birthMonth"), getJsonNumber(hitterObject, "birthDate")),
                            signYear: getJsonNumber(hitterObject, "startYear"),
                            draftPick: getJsonNumberNullable(hitterObject, "draftPick"),
                            stats: getHitterStats(hitterObject),
                            models: getHitterModels(hitterObject)
                        };
                        return [2, hitter_1];
                    }
                    return [2, null];
            }
        });
    });
}
function updateHitterStats(hitter) {
    var stats_body = getElementByIdStrict('tstats_body');
    hitter.stats.forEach(function (f) {
        var tr = document.createElement('tr');
        tr.innerHTML = "\n            <td>".concat(f.year, "</td>\n            <td>").concat(level_map[f.level], "</td>\n            <td>").concat(getTeamAbbr(f.team, f.year), "</td>\n            <td>").concat(getLeagueAbbr(f.league), "</td>\n            <td>").concat(f.pa, "</td>\n            <td>").concat(f.avg.toFixed(3), "</td>\n            <td>").concat(f.obp.toFixed(3), "</td>\n            <td>").concat(f.slg.toFixed(3), "</td>\n            <td>").concat(f.iso.toFixed(3), "</td>\n            <td>").concat(f.wrc, "</td>\n            <td>").concat(f.hr, "</td>\n            <td>").concat(f.bbPerc.toFixed(1), "</td>\n            <td>").concat(f.kPerc.toFixed(1), "</td>\n            <td>").concat(f.sb, "</td>\n            <td>").concat(f.cs, "</td>\n        ");
        stats_body.appendChild(tr);
    });
}
var HITTER_WAR_BUCKETS = [0, 0.5, 2.5, 7.5, 15, 25, 35];
var HITTER_WAR_LABELS = ["<=0", "0-1", "1-5", "5-10", "10-20", "20-30", "30+"];
function piePointGenerator(model) {
    var points = [];
    for (var i = 0; i < HITTER_WAR_LABELS.length; i++) {
        points.push({ y: model.probs[i], label: HITTER_WAR_LABELS[i] });
    }
    return points;
}
function lineCallback(index) {
    if (hitter !== null) {
        var model = hitter.models[index];
        if (pie_graph === null)
            throw new Error("Pie Graph null at lineCallback");
        var title_text = model.month == 0 ?
            "Iniitial Outcome Distribution" :
            "".concat(model.month, "-").concat(model.year, " Outcome Distribution");
        pie_graph.updateChart(model.probs, title_text);
    }
    else if (pitcher !== null) {
    }
    else {
        throw new Error("Both pitcher and hitter null at lineCallback");
    }
}
function setupModel(hitter) {
    var line_points = hitter.models.map(function (f) {
        var war = 0;
        for (var i = 0; i < f.probs.length; i++)
            war += f.probs[i] * HITTER_WAR_BUCKETS[i];
        var label = f.month == 0 ? 'Initial' : "".concat(f.month, "-").concat(f.year);
        var p = { y: war, label: label };
        return p;
    });
    line_graph = new LineGraph(model_graph, line_points, lineCallback);
    var pie_points = piePointGenerator(hitter.models[hitter.models.length - 1]);
    pie_graph = new PieGraph(model_pie, pie_points, "Outcome Distribution");
}
var model_pie = getElementByIdStrict("projWarPie");
var model_graph = getElementByIdStrict("projWarGraph");
var line_graph = null;
var pie_graph = null;
var hitter = null;
var pitcher = null;
function main() {
    return __awaiter(this, void 0, void 0, function () {
        var id, age;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4, retrieveJson("../../assets/map.json.gz")];
                case 1:
                    org_map = _a.sent();
                    id = getQueryParam("id");
                    return [4, loadHitter(id)];
                case 2:
                    hitter = _a.sent();
                    if (hitter !== null) {
                        updateElementText("player_name", "".concat(hitter.firstName, " ").concat(hitter.lastName));
                        age = getDateDelta(hitter.birthDate, new Date());
                        updateElementText("player_age", "".concat(age[0], " years, ").concat(age[1], " months, ").concat(age[2], " days"));
                        if (hitter.draftPick !== null) {
                            updateElementText("player_draft", "#".concat(hitter.draftPick, " Overall, ").concat(hitter.signYear));
                        }
                        updateHitterStats(hitter);
                        setupModel(hitter);
                    }
                    return [2];
            }
        });
    });
}
main();
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
var POINT_DEFAULT_SIZE = 3;
var POINT_HIGHLIGHT_SIZE = 9;
var LineGraph = (function () {
    function LineGraph(element, points, callback, colorscale) {
        if (colorscale === void 0) { colorscale = null; }
        var _this = this;
        this.points = points;
        this.callback = callback;
        if (colorscale !== null)
            this.colorscale = colorscale;
        else
            this.colorscale = [
                '#d43d51',
                '#df797d',
                '#e2acab',
                '#dddddd',
                '#a1c0b6',
                '#63a490',
                '#00876c'
            ];
        this.chart = new Chart(element, {
            type: 'line',
            data: {
                labels: points.map(function (f) { return f.label; }),
                datasets: [{
                        label: 'Model 1',
                        data: points.map(function (f) { return f.y; }),
                        pointRadius: new Array(points.length).fill(POINT_DEFAULT_SIZE),
                    }],
            },
            options: {
                onClick: function (e, elements) {
                    var points = _this.chart.getElementsAtEventForMode(e, 'nearest', { intesect: false, axis: 'x' }, true);
                    if (points.length > 0) {
                        var firstPoint = points[0];
                        var index = firstPoint.index;
                        _this.highlight_index(index);
                        _this.callback(index);
                    }
                },
                scales: {
                    y: {
                        min: 0
                    }
                }
            }
        });
        if (points.length > 0)
            this.highlight_index(points.length - 1);
    }
    LineGraph.prototype.highlight_index = function (index) {
        var pointRadius = new Array(this.points.length).fill(POINT_DEFAULT_SIZE);
        pointRadius[index] = POINT_HIGHLIGHT_SIZE;
        this.chart.data.datasets[0].pointRadius = pointRadius;
        this.chart.update();
    };
    return LineGraph;
}());
var PieGraph = (function () {
    function PieGraph(element, points, title_text, colorscale) {
        if (colorscale === void 0) { colorscale = null; }
        var _this = this;
        this.points = points;
        if (colorscale !== null)
            this.colorscale = colorscale;
        else
            this.colorscale = [
                '#d43d51',
                '#df797d',
                '#e2acab',
                '#dddddd',
                '#a1c0b6',
                '#63a490',
                '#00876c'
            ];
        this.chart = new Chart(element, {
            type: 'pie',
            data: {
                labels: points.map(function (f) { return f.label; }),
                datasets: [{
                        label: '',
                        data: points.map(function (f) { return f.y; }),
                        backgroundColor: this.colorscale
                    }]
            },
            options: {
                plugins: {
                    legend: {
                        display: false,
                    },
                    title: {
                        display: true,
                        text: title_text,
                    },
                    tooltip: {
                        callbacks: {
                            label: function (item) {
                                var point = _this.points[item.dataIndex];
                                return "\t".concat((point.y * 100).toFixed(1), "%");
                            }
                        },
                    }
                },
                hover: {
                    mode: null,
                }
            }
        });
    }
    PieGraph.prototype.updateChart = function (values, title_text) {
        this.points.forEach(function (f, idx) { f.y = values[idx]; });
        this.chart.data.datasets[0].data = values;
        this.chart.options.plugins.title.text = title_text;
        this.chart.update();
    };
    return PieGraph;
}());
