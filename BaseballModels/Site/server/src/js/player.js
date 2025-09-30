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
    var statsArray = getJsonArray(hitterObject, "hit_stats");
    var statsMonthArray = getJsonArray(hitterObject, "hit_month_stats");
    statsArray = statsArray.concat(statsMonthArray);
    statsArray.forEach(function (f) {
        var fObj = f;
        var hs = {
            level: getJsonNumber(fObj, "levelId"),
            year: getJsonNumber(fObj, "year"),
            month: getJsonNumberNullable(fObj, "month"),
            team: getJsonNumber(fObj, "teamId"),
            league: getJsonNumber(fObj, "leagueId"),
            pa: getJsonNumber(fObj, "PA"),
            avg: getJsonNumber(fObj, "AVG"),
            obp: getJsonNumber(fObj, "OBP"),
            slg: getJsonNumber(fObj, "SLG"),
            iso: getJsonNumber(fObj, "ISO"),
            wrc: getJsonNumber(fObj, "WRC"),
            hr: getJsonNumber(fObj, "HR"),
            bbPerc: getJsonNumber(fObj, "BBPerc"),
            kPerc: getJsonNumber(fObj, "KPerc"),
            sb: getJsonNumber(fObj, "SB"),
            cs: getJsonNumber(fObj, "CS")
        };
        stats.push(hs);
    });
    stats.sort(function (a, b) {
        if (a.year < b.year)
            return -1;
        if (a.year > b.year)
            return 1;
        if (a.month === null && b.month === null)
            return b.level - a.level;
        if (a.month === null)
            return -1;
        if (b.month === null)
            return 1;
        if (a.month === b.month)
            return b.level - a.level;
        return a.month - b.month;
    });
    return stats;
}
function getPitcherStats(pitcherObject) {
    var stats = [];
    var statsArray = getJsonArray(pitcherObject, "pit_stats");
    var statsMonthArray = getJsonArray(pitcherObject, "pit_month_stats");
    statsArray = statsArray.concat(statsMonthArray);
    statsArray.forEach(function (f) {
        var fObj = f;
        var ps = {
            level: getJsonNumber(fObj, "levelId"),
            year: getJsonNumber(fObj, "year"),
            month: getJsonNumberNullable(fObj, "month"),
            team: getJsonNumber(fObj, "teamId"),
            league: getJsonNumber(fObj, "leagueId"),
            ip: getJsonString(fObj, "IP"),
            era: getJsonNumber(fObj, "ERA"),
            fip: getJsonNumber(fObj, "FIP"),
            hrrate: getJsonNumber(fObj, "HR9"),
            bbperc: getJsonNumber(fObj, "BBPerc"),
            kperc: getJsonNumber(fObj, "KPerc"),
            gorate: getJsonNumber(fObj, "GOPerc")
        };
        stats.push(ps);
    });
    stats.sort(function (a, b) {
        if (a.year < b.year)
            return -1;
        if (a.year > b.year)
            return 1;
        if (a.month === null && b.month === null)
            return b.level - a.level;
        if (a.month === null)
            return -1;
        if (b.month === null)
            return 1;
        if (a.month === b.month)
            return b.level - a.level;
        return a.month - b.month;
    });
    return stats;
}
function getModels(obj, name) {
    var models = [];
    var modelArray = getJsonArray(obj, name);
    modelArray.forEach(function (f) {
        var fObj = f;
        var probString = getJsonString(fObj, "probsWar");
        var probArray = probString.split(',').map(Number);
        var m = {
            year: getJsonNumber(fObj, "year"),
            month: getJsonNumber(fObj, "month"),
            probs: probArray,
            modelId: getJsonNumber(fObj, "modelId"),
            rank: fObj["rank"],
        };
        models.push(m);
    });
    var models_list = [];
    MODEL_VALUES.forEach(function (f) {
        models_list.push(models.filter(function (g) { return g.modelId === f; }));
    });
    return models_list;
}
function getPerson(obj) {
    var draftPick = obj["draftPick"];
    var draft = null;
    if (draftPick !== null)
        draft = {
            pick: draftPick,
            round: getJsonString(obj, "draftRound"),
            bonus: getJsonNumber(obj, "draftBonus")
        };
    var p = {
        firstName: getJsonString(obj, "firstName"),
        lastName: getJsonString(obj, "lastName"),
        birthDate: new Date(getJsonNumber(obj, "birthYear"), getJsonNumber(obj, "birthMonth"), getJsonNumber(obj, "birthDate")),
        signYear: getJsonNumber(obj, "startYear"),
        position: getJsonString(obj, "position"),
        status: getJsonString(obj, "status"),
        draft: draft,
        parentId: getJsonNumber(obj, "orgId"),
        isHitter: obj["isHitter"],
        isPitcher: obj["isPitcher"],
        inTraining: obj["inTraining"],
    };
    return p;
}
function tableUpdateCallback(tablebody, monthcol, year, type) {
    var rows = Array.from(tablebody.getElementsByTagName('tr'));
    rows.forEach(function (f) {
        var y = f.dataset.year;
        var t = f.dataset.type;
        if (year !== y)
            return;
        if (type === t)
            f.classList.add('hidden');
        else
            f.classList.remove('hidden');
    });
    var any_monthly = rows.reduce(function (a, b) {
        if (a)
            return true;
        var t = b.dataset.type;
        return (t === 'month') && (!b.classList.contains('hidden'));
    }, false);
    var monthly_elements = document.getElementsByClassName('table_month');
    if (any_monthly)
        for (var i = 0; i < monthly_elements.length; i++)
            monthly_elements[i].classList.remove('hidden');
    else
        for (var i = 0; i < monthly_elements.length; i++)
            monthly_elements[i].classList.add('hidden');
}
function updateHitterStats(hitterStats) {
    var stats_body = getElementByIdStrict('h_stats_body');
    var hcol_month = getElementByIdStrict('hcol_month');
    var prevYear = null;
    var prevYearMonthly = null;
    hitterStats.forEach(function (f) {
        var tr = document.createElement('tr');
        var isFirst = false;
        if (f.month !== null) {
            if (f.year != prevYearMonthly) {
                isFirst = true;
                prevYearMonthly = f.year;
            }
            tr.classList.add('hidden');
        }
        else {
            if (f.year != prevYear) {
                isFirst = true;
                prevYear = f.year;
            }
        }
        var teamAbbr = "";
        try {
            teamAbbr = getTeamAbbr(f.team, f.year);
        }
        catch (e) {
            if (f.league != 134 && f.league != 130)
                throw e;
        }
        tr.innerHTML = "\n            <td></td>\n            <td>".concat(f.year, "</td>\n            <td class='table_month hidden'>").concat(f.month !== null ? MONTH_CODES[f.month] : "", "</td>\n            <td>").concat(level_map[f.level], "</td>\n            <td>").concat(teamAbbr, "</td>\n            <td>").concat(getLeagueAbbr(f.league), "</td>\n            <td class=\"align_right\">").concat(f.pa, "</td>\n            <td class=\"align_right\">").concat(f.avg.toFixed(3), "</td>\n            <td class=\"align_right\">").concat(f.obp.toFixed(3), "</td>\n            <td class=\"align_right\">").concat(f.slg.toFixed(3), "</td>\n            <td class=\"align_right\">").concat(f.iso.toFixed(3), "</td>\n            <td class=\"align_right\">").concat(f.wrc, "</td>\n            <td class=\"align_right\">").concat(f.hr, "</td>\n            <td class=\"align_right\">").concat(f.bbPerc.toFixed(1), "</td>\n            <td class=\"align_right\">").concat(f.kPerc.toFixed(1), "</td>\n            <td class=\"align_right\">").concat(f.sb, "</td>\n            <td class=\"align_right\">").concat(f.cs, "</td>\n        ");
        tr.dataset.year = f.year.toString();
        tr.dataset.type = f.month === null ? "year" : "month";
        if (isFirst) {
            tr.classList.add('row_first');
            var button_td = tr.getElementsByTagName('td')[0];
            var button = document.createElement('button');
            button.classList.add('table_button');
            if (f.month === null) {
                button.innerText = '+';
                button.classList.add('table_expand');
                button.addEventListener('click', function () {
                    tableUpdateCallback(stats_body, hcol_month, f.year.toString(), 'year');
                });
            }
            else {
                button.innerText = '-';
                button.classList.add('table_retract');
                button.addEventListener('click', function () {
                    tableUpdateCallback(stats_body, hcol_month, f.year.toString(), 'month');
                });
            }
            button_td.appendChild(button);
        }
        stats_body.appendChild(tr);
    });
}
function updatePitcherStats(pitcherStats) {
    var stats_body = getElementByIdStrict('p_stats_body');
    var pcol_month = getElementByIdStrict('pcol_month');
    var prevYear = null;
    var prevYearMonthly = null;
    pitcherStats.forEach(function (f) {
        var tr = document.createElement('tr');
        var isFirst = false;
        if (f.month !== null) {
            if (f.year != prevYearMonthly) {
                isFirst = true;
                prevYearMonthly = f.year;
            }
            tr.classList.add('hidden');
        }
        else {
            if (f.year != prevYear) {
                isFirst = true;
                prevYear = f.year;
            }
        }
        var teamAbbr = "";
        try {
            teamAbbr = getTeamAbbr(f.team, f.year);
        }
        catch (e) {
            if (f.league != 134 && f.league != 130)
                throw e;
        }
        tr.innerHTML = "\n            <td></td>\n            <td>".concat(f.year, "</td>\n            <td class='table_month hidden'>").concat(f.month !== null ? MONTH_CODES[f.month] : "", "</td>\n            <td>").concat(level_map[f.level], "</td>\n            <td>").concat(teamAbbr, "</td>\n            <td>").concat(getLeagueAbbr(f.league), "</td>\n            <td class=\"align_right\">").concat(f.ip, "</td>\n            <td class=\"align_right\">").concat(f.era.toFixed(2), "</td>\n            <td class=\"align_right\">").concat(f.fip.toFixed(2), "</td>\n            <td class=\"align_right\">").concat(f.hrrate.toFixed(1), "</td>\n            <td class=\"align_right\">").concat(f.bbperc.toFixed(1), "</td>\n            <td class=\"align_right\">").concat(f.kperc.toFixed(1), "</td>\n            <td class=\"align_right\">").concat(f.gorate.toFixed(1), "</td>\n        ");
        tr.dataset.year = f.year.toString();
        tr.dataset.type = f.month === null ? "year" : "month";
        if (isFirst) {
            tr.classList.add('row_first');
            var button_td = tr.getElementsByTagName('td')[0];
            var button = document.createElement('button');
            button.classList.add('table_button');
            if (f.month === null) {
                button.innerText = '+';
                button.classList.add('table_expand');
                button.addEventListener('click', function () {
                    tableUpdateCallback(stats_body, pcol_month, f.year.toString(), 'year');
                });
            }
            else {
                button.innerText = '-';
                button.classList.add('table_retract');
                button.addEventListener('click', function () {
                    tableUpdateCallback(stats_body, pcol_month, f.year.toString(), 'month');
                });
            }
            button_td.appendChild(button);
        }
        stats_body.append(tr);
    });
}
var WAR_BUCKETS = [0, 0.5, 3, 7.5, 15, 25, 35];
var WAR_LABELS = ["<=0", "0-1", "1-5", "5-10", "10-20", "20-30", "30+"];
var VALUE_BUCKETS = [0, 2.5, 12.5, 35, 75, 150, 250];
var VALUE_LABELS = ["<=0", "0M-5M", "5M-20M", "20M-50M", "50M-100M", "100M-200M", "200M+"];
function piePointGenerator(model) {
    var points = [];
    for (var i = 0; i < WAR_LABELS.length; i++) {
        if (modelIsWar)
            points.push({ y: model.probs[i], label: WAR_LABELS[i] });
        else
            points.push({ y: model.probs[i], label: VALUE_LABELS[i] });
    }
    return points;
}
function lineCallback(index, modelId) {
    var model;
    if (line_graph.graphIsHitter()) {
        model = hitterModels[modelId - 1][index];
    }
    else {
        model = pitcherModels[modelId - 1][index];
    }
    if (model !== null) {
        if (pie_graph === null)
            throw new Error("Pie Graph null at lineCallback");
        var title_text = model.month == 0 ?
            "Iniitial Outcome Distribution" :
            "".concat(model.month, "-").concat(model.year, " Outcome Distribution");
        if (modelIsWar)
            pie_graph.updateChart(model.probs, title_text, WAR_LABELS);
        else
            pie_graph.updateChart(model.probs, title_text, VALUE_LABELS);
    }
    else {
        throw new Error("Model was not set for hitter or pitcher");
    }
}
function getDatasets(hitter_war_list, hitter_ranks_list, pitcher_war_list, pitcher_ranks_list) {
    var datasets = [];
    if (hitter_ranks_list.length !== hitter_war_list.length)
        throw new Error("getDatasets: Hitter War vs Ranks length mismatch");
    for (var i = 0; i < hitter_war_list.length; i++) {
        if (hitter_war_list[i].length > 0) {
            datasets.push({
                points: hitter_war_list[i],
                title: MODEL_STRINGS[i],
                modelId: MODEL_VALUES[i],
                isLog: false,
                isHitter: true
            });
        }
        if (hitter_ranks_list[i].length > 0) {
            datasets.push({
                points: hitter_ranks_list[i],
                title: MODEL_STRINGS[i] + " Rank",
                modelId: MODEL_VALUES[i],
                isLog: true,
                isHitter: true
            });
        }
    }
    if (pitcher_ranks_list.length !== pitcher_war_list.length)
        throw new Error("getDatasets: Pitcher War vs Ranks length mismatch");
    for (var i = 0; i < pitcher_war_list.length; i++) {
        if (pitcher_war_list[i].length > 0) {
            datasets.push({
                points: pitcher_war_list[i],
                title: MODEL_STRINGS[i],
                modelId: MODEL_VALUES[i],
                isLog: false,
                isHitter: false
            });
        }
        if (pitcher_ranks_list[i].length > 0) {
            datasets.push({
                points: pitcher_ranks_list[i],
                title: MODEL_STRINGS[i] + " Rank",
                modelId: MODEL_VALUES[i],
                isLog: true,
                isHitter: false
            });
        }
    }
    return datasets;
}
function setupSelector(hitter_war_list, hitter_ranks_list, pitcher_war_list, pitcher_ranks_list) {
    var graph_selector = getElementByIdStrict('graph_selector');
    for (var i = 0; i < hitter_war_list.length; i++) {
        if (hitter_war_list[i].length > 0) {
            var opt = document.createElement('option');
            opt.text = "Hitter " + MODEL_STRINGS[i];
            opt.value = graph_selector.children.length.toString();
            graph_selector.appendChild(opt);
        }
        if (hitter_ranks_list[i].length > 0) {
            var opt = document.createElement('option');
            opt.text = "Hitter " + MODEL_STRINGS[i] + " Rank";
            opt.value = graph_selector.children.length.toString();
            graph_selector.appendChild(opt);
        }
    }
    for (var i = 0; i < pitcher_war_list.length; i++) {
        if (pitcher_war_list[i].length > 0) {
            var opt = document.createElement('option');
            opt.text = "Pitcher " + MODEL_STRINGS[i];
            opt.value = graph_selector.children.length.toString();
            graph_selector.appendChild(opt);
        }
        if (pitcher_ranks_list[i].length > 0) {
            var opt = document.createElement('option');
            opt.text = "Pitcher " + MODEL_STRINGS[i] + " Rank";
            opt.value = graph_selector.children.length.toString();
            graph_selector.appendChild(opt);
        }
    }
    graph_selector.value = "0";
    graph_selector.addEventListener('change', function () {
        line_graph.setDataset(parseInt(graph_selector.value));
        line_graph.fireCallback();
    });
}
function setupModel(hitterModels, pitcherModels) {
    var war_map = function (f, buckets) {
        var war = 0;
        for (var i = 0; i < f.probs.length; i++)
            war += f.probs[i] * buckets[i];
        var label = f.month == 0 ? 'Initial' : "".concat(f.month, "-").concat(f.year);
        var p = { y: war, label: label };
        return p;
    };
    var rank_map = function (f) {
        var month = f.month;
        var year = f.year;
        if (f.rank === null)
            throw new Error("No Rank");
        var p = { y: f.rank, label: year == 0 ? "Initial" : "".concat(month, "-").concat(year) };
        return p;
    };
    var hitter_war_points = [];
    var pitcher_war_points = [];
    for (var _i = 0, MODEL_VALUES_1 = MODEL_VALUES; _i < MODEL_VALUES_1.length; _i++) {
        var idx = MODEL_VALUES_1[_i];
        if (modelIsWar) {
            if (hitterModels.length > 0)
                hitter_war_points.push(hitterModels[idx - 1].map(function (f) { return war_map(f, WAR_BUCKETS); }));
            if (pitcherModels.length > 0)
                pitcher_war_points.push(pitcherModels[idx - 1].map(function (f) { return war_map(f, WAR_BUCKETS); }));
        }
        else {
            if (hitterModels.length > 0)
                hitter_war_points.push(hitterModels[idx - 1].map(function (f) { return war_map(f, VALUE_BUCKETS); }));
            if (pitcherModels.length > 0)
                pitcher_war_points.push(pitcherModels[idx - 1].map(function (f) { return war_map(f, VALUE_BUCKETS); }));
        }
    }
    var hitter_rank_points = hitterModels.map(function (m) { return m.filter(function (f) { return f.rank !== null; }).map(rank_map); });
    var pitcher_rank_points = pitcherModels.map(function (m) { return m.filter(function (f) { return f.rank !== null; }).map(rank_map); });
    var datasets = getDatasets(hitter_war_points, hitter_rank_points, pitcher_war_points, pitcher_rank_points);
    line_graph = new LineGraph(model_graph, datasets, lineCallback);
    setupSelector(hitter_war_points, hitter_rank_points, pitcher_war_points, pitcher_rank_points);
    var pie_points = person.isHitter ?
        piePointGenerator(hitterModels[0][hitterModels[0].length - 1]) :
        piePointGenerator(pitcherModels[0][pitcherModels[0].length - 1]);
    pie_graph = new PieGraph(model_pie, pie_points, "Outcome Distribution");
}
var model_pie = getElementByIdStrict("projWarPie");
var model_graph = getElementByIdStrict("projWarGraph");
var line_graph;
var pie_graph;
var keyControls;
var person;
var hitterModels;
var pitcherModels;
var modelIsWar = true;
function main() {
    return __awaiter(this, void 0, void 0, function () {
        var datesJsonPromise, id, player_data, player_search_data, pd, hitterStats, pitcherStats, trainingWarning, player_team, age, round, _a, datesJson, endYear, endMonth, hitter_title_element, pitcher_title_element;
        return __generator(this, function (_b) {
            switch (_b.label) {
                case 0:
                    datesJsonPromise = retrieveJson('../../assets/dates.json.gz');
                    id = getQueryParam("id");
                    player_data = fetch("/player/".concat(id));
                    player_search_data = retrieveJson('../../assets/player_search.json.gz');
                    return [4, retrieveJson("../../assets/map.json.gz")];
                case 1:
                    org_map = _b.sent();
                    return [4, player_data];
                case 2: return [4, (_b.sent()).json()];
                case 3:
                    pd = _b.sent();
                    person = getPerson(pd);
                    hitterStats = person.isHitter ? getHitterStats(pd) : [];
                    pitcherStats = person.isPitcher ? getPitcherStats(pd) : [];
                    if (hitterStats.length > 0) {
                        updateHitterStats(hitterStats);
                        getElementByIdStrict('hitter_stats').classList.remove('hidden');
                    }
                    if (pitcherStats.length > 0) {
                        updatePitcherStats(pitcherStats);
                        getElementByIdStrict('pitcher_stats').classList.remove('hidden');
                    }
                    hitterModels = person.isHitter ? getModels(pd, "hit_models") : [];
                    pitcherModels = person.isPitcher ? getModels(pd, "pit_models") : [];
                    setupModel(hitterModels, pitcherModels);
                    line_graph.fireCallback();
                    updateElementText("player_name", "".concat(person.firstName, " ").concat(person.lastName));
                    updateElementText("player_position", person.position);
                    updateElementText("player_status", person.status);
                    if (!person.inTraining) {
                        trainingWarning = getElementByIdStrict('playerInTraining');
                        trainingWarning.classList.add('hidden');
                    }
                    if (person.parentId !== null && person.parentId !== 0) {
                        player_team = getElementByIdStrict("player_team");
                        player_team.innerText = getParentName(person.parentId);
                        player_team.href = "teams.html?team=".concat(person.parentId);
                    }
                    else {
                        updateElementText("player_team", "Free Agent");
                    }
                    age = getDateDelta(person.birthDate, new Date());
                    updateElementText("player_age", "".concat(age[0], "y, ").concat(age[1], "m, ").concat(age[2], "d"));
                    if (person.draft !== null) {
                        round = isNaN(parseFloat(person.draft.round)) ? person.draft.round : "Round " + person.draft.round;
                        updateElementText("player_draft", "".concat(person.signYear, " Draft, ").concat(round, " (").concat(getOrdinalNumber(person.draft.pick), " Overall)\n$").concat(person.draft.bonus.toLocaleString(), " Bonus"));
                    }
                    document.title = person.firstName + " " + person.lastName;
                    keyControls = new KeyControls(document, function (x_inc) {
                        if (line_graph !== null)
                            line_graph.increment_index(x_inc);
                    });
                    _a = SearchBar.bind;
                    return [4, player_search_data];
                case 4:
                    searchBar = new (_a.apply(SearchBar, [void 0, _b.sent()]))();
                    return [4, datesJsonPromise];
                case 5:
                    datesJson = _b.sent();
                    endYear = datesJson["endYear"];
                    endMonth = datesJson["endMonth"];
                    hitter_title_element = getElementByIdStrict('hitter_stats_title');
                    pitcher_title_element = getElementByIdStrict('pitcher_stats_title');
                    hitter_title_element.textContent = "Hitter Stats through ".concat(MONTH_CODES[endMonth], " ").concat(endYear);
                    pitcher_title_element.textContent = "Pitcher Stats through ".concat(MONTH_CODES[endMonth], " ").concat(endYear);
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
var KeyControls = (function () {
    function KeyControls(document, callback) {
        this.callback = callback;
        document.addEventListener('keydown', function (event) {
            if (event.key === "ArrowLeft") {
                callback(-1);
                event.preventDefault();
            }
            else if (event.key === "ArrowRight") {
                callback(1);
                event.preventDefault();
            }
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
function getQueryParamNullable(name) {
    var params = new URLSearchParams(window.location.search);
    var value = params.get(name);
    if (value === null)
        return null;
    return Number(value);
}
function getQueryParamBackup(name, backup) {
    try {
        return getQueryParam(name);
    }
    catch (_) {
        return backup;
    }
}
function getQueryParamBackupStr(name, backup) {
    var params = new URLSearchParams(window.location.search);
    var value = params.get(name);
    if (value === null)
        return backup;
    return value;
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
function getParentAbbrFallback(id, fallback) {
    try {
        return getParentAbbr(id);
    }
    catch (_) {
        return fallback;
    }
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
function formatModelString(val, isWar) {
    if (isWar === 1)
        return "".concat(val.toFixed(1), " WAR");
    else
        return "$".concat(val.toFixed(0), "M");
}
var MODEL_VALUES = [1, 2];
var MODEL_STRINGS = ["Base", "Stats Only"];
var org_map = null;
var level_map = { 1: "MLB", 11: "AAA", 12: "AA", 13: "A+", 14: "A", 15: "A-", 16: "Rk", 17: "DSL", 20: "" };
var MONTH_CODES = ["", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dev"];
var POINT_DEFAULT_SIZE = 3;
var POINT_HIGHLIGHT_SIZE = 9;
var LineGraph = (function () {
    function LineGraph(element, datasets, callback, colorscale) {
        if (colorscale === void 0) { colorscale = null; }
        var _this = this;
        this.datasets = datasets;
        this.datasetIdx = 0;
        this.callback = callback;
        this.pointIdx = 0;
        this.points = this.datasets[0].points;
        this.graphObjs = datasets.map(function (f) {
            var go = {
                dataset: {
                    label: f.title,
                    data: f.points.map(function (f) { return f.y; }),
                    pointRadius: new Array(f.points.length).fill(POINT_DEFAULT_SIZE),
                },
                yscale: {}
            };
            if (f.isLog) {
                go.yscale = {
                    type: 'logarithmic',
                    reverse: true,
                    min: 0.1,
                    max: 20000,
                    title: {
                        display: true,
                        text: 'Prospect Ranking'
                    },
                    grid: {
                        color: css.background_low
                    },
                    ticks: {
                        callback: function (value, index, ticks) {
                            if (value === 1 || value === 10 || value === 100 || value === 1000 || value === 10000)
                                return value.toLocaleString();
                            return null;
                        }
                    }
                };
            }
            else {
                go.yscale = {
                    min: 0,
                    max: Math.max(16, f.points.map(function (g) { return g.y; }).reduce(function (a, b) { return Math.max(a, b); }, 0) + 2),
                    grid: {
                        color: css.background_low
                    },
                    title: {
                        display: true,
                        text: 'Expected WAR Through Control'
                    },
                    position: 'left',
                    ticks: {
                        callback: function (value, index, ticks) {
                            if (value % 2 === 0)
                                return value.toLocaleString();
                            return null;
                        }
                    }
                };
            }
            return go;
        });
        this.chart = new Chart(element, {
            type: 'line',
            data: {
                labels: this.points.map(function (f) { return f.label; }),
                datasets: [this.graphObjs[0].dataset],
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: {
                    duration: 0,
                },
                plugins: {
                    legend: {
                        display: false,
                    },
                },
                onClick: function (e, elements) {
                    var points = _this.chart.getElementsAtEventForMode(e, 'nearest', { intesect: false, axis: 'x' }, true);
                    if (points.length > 0) {
                        var firstPoint = points[0];
                        var index = firstPoint.index;
                        _this.highlight_index(index);
                        _this.callback(index, _this.getSelectedModel());
                    }
                },
                scales: {
                    y: this.graphObjs[0].yscale,
                    x: {
                        grid: {
                            color: css.background_low
                        }
                    }
                },
            }
        });
        if (this.points.length > 0)
            this.highlight_index(this.points.length - 1);
    }
    LineGraph.prototype.highlight_index = function (index) {
        this.pointIdx = index;
        var pointRadius = new Array(this.points.length).fill(POINT_DEFAULT_SIZE);
        pointRadius[index] = POINT_HIGHLIGHT_SIZE;
        this.chart.data.datasets[0].pointRadius = pointRadius;
        this.chart.update();
    };
    LineGraph.prototype.increment_index = function (x_inc) {
        if ((x_inc === -1 && this.pointIdx > 0) || (x_inc === 1 && this.pointIdx < this.points.length - 1)) {
            this.pointIdx += x_inc;
            this.highlight_index(this.pointIdx);
            this.callback(this.getAdjustedPointIndex(), this.getSelectedModel());
        }
    };
    LineGraph.prototype.fireCallback = function () {
        this.callback(this.getAdjustedPointIndex(), this.getSelectedModel());
    };
    LineGraph.prototype.getAdjustedPointIndex = function () {
        var index = this.pointIdx;
        if (this.datasets[this.datasetIdx].isLog)
            index++;
        return index;
    };
    LineGraph.prototype.graphIsHitter = function () {
        return this.datasets[this.datasetIdx].isHitter;
    };
    LineGraph.prototype.setDataset = function (idx) {
        this.datasetIdx = idx;
        this.points = this.datasets[this.datasetIdx].points;
        this.chart.data.datasets[0] = this.graphObjs[this.datasetIdx].dataset;
        this.chart.data.labels = this.points.map(function (f) { return f.label; });
        this.chart.options.scales.y = this.graphObjs[this.datasetIdx].yscale;
        this.chart.update();
        if (this.points.length > 0)
            this.highlight_index(this.points.length - 1);
    };
    LineGraph.prototype.getSelectedModel = function () {
        return this.datasets[this.datasetIdx].modelId;
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
                responseive: true,
                maintainAspectRatio: false,
                animation: {
                    duration: 0,
                },
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
    PieGraph.prototype.updateChart = function (values, title_text, labels) {
        this.points.forEach(function (f, idx) { f.y = values[idx]; });
        this.chart.data.datasets[0].data = values;
        this.chart.data.labels = labels;
        this.chart.options.plugins.title.text = title_text;
        this.chart.update();
    };
    return PieGraph;
}());
