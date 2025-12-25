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
var month;
var year;
var modelId;
var teamId = null;
var levelId;
var statsTables = getElementByIdStrict('stats_table_holder');
var stat_hitter_btn = getElementByIdStrict('stat_hitter_btn');
var stat_pitcher_btn = getElementByIdStrict('stat_pitcher_btn');
function GetHitterPromise() {
    if (teamId !== null) {
        return fetch("/stats_hitter?year=".concat(year, "&month=").concat(month, "&model=").concat(modelId, "&level=").concat(levelId, "&team=").concat(teamId));
    }
    else {
        return fetch("/stats_hitter?year=".concat(year, "&month=").concat(month, "&model=").concat(modelId, "&level=").concat(levelId));
    }
}
function GetPitcherPromise() {
    if (teamId !== null) {
        return fetch("/stats_pitcher?year=".concat(year, "&month=").concat(month, "&model=").concat(modelId, "&level=").concat(levelId, "&team=").concat(teamId));
    }
    else {
        return fetch("/stats_pitcher?year=".concat(year, "&month=").concat(month, "&model=").concat(modelId, "&level=").concat(levelId));
    }
}
function main() {
    return __awaiter(this, void 0, void 0, function () {
        var datesJsonPromise, player_search_data, org_map_promise, datesJson, statsHitterPromise, statsPitcherPromise, statsHitter, statsPitcher, lnk, al_r, al_l, br, bl, hitterStatsViewer, pitcherStatsViewer, _a;
        return __generator(this, function (_b) {
            switch (_b.label) {
                case 0:
                    datesJsonPromise = retrieveJson('../../assets/dates.json.gz');
                    player_search_data = retrieveJson('../../assets/player_search.json.gz');
                    org_map_promise = retrieveJson("../../assets/map.json.gz");
                    return [4, datesJsonPromise];
                case 1:
                    datesJson = _b.sent();
                    endYear = datesJson["endYear"];
                    endMonth = datesJson["endMonth"];
                    month = getQueryParamBackup('month', endMonth);
                    year = getQueryParamBackup('year', endYear);
                    modelId = getQueryParamBackup("model", 1);
                    teamId = getQueryParamNullable('team');
                    levelId = getQueryParamBackup('level', 0);
                    statsHitterPromise = GetHitterPromise();
                    statsPitcherPromise = GetPitcherPromise();
                    return [4, org_map_promise];
                case 2:
                    org_map = _b.sent();
                    setupSelector({
                        month: month,
                        year: year,
                        modelId: modelId,
                        endYear: endYear,
                        endMonth: endMonth,
                        startYear: datesJson["startYear"],
                        startTeam: teamId,
                        level: levelId
                    });
                    return [4, statsHitterPromise];
                case 3: return [4, (_b.sent()).json()];
                case 4:
                    statsHitter = _b.sent();
                    return [4, statsPitcherPromise];
                case 5: return [4, (_b.sent()).json()];
                case 6:
                    statsPitcher = _b.sent();
                    lnk = 'link';
                    al_r = 'align_right';
                    al_l = 'align_left';
                    br = 'border_right';
                    bl = 'border_left';
                    hitterStatsViewer = new SortableStatsViewer(statsHitter, DB_Prediction_HitterStats, ["Name", "Org", "Age", "Position", "PA", "1B", "2B", "3B", "HR", "BB%", "K%", "SB", "CS", "AVG", "OBP", "SLG", "ISO", "wRC+", "OFF", "BSR", "DEF", "WAR"], function (f) { return [["<a href='player?id=".concat(f.player.mlbId, "'>").concat(f.player.firstName + ' ' + f.player.lastName, "</a>"), [al_l, lnk]],
                        [getParentAbbr(f.player.orgId), [al_l, lnk]],
                        [getDateDelta(new Date(f.player.birthYear, f.player.birthMonth, f.player.birthDate), new Date())[0].toString(), [al_r]],
                        [f.player.position, [al_l, br]],
                        [f.obj.Pa.toFixed(0), [al_r]], [f.obj.Hit1B.toFixed(1), [al_r]], [f.obj.Hit2B.toFixed(1), [al_r]], [f.obj.Hit3B.toFixed(1), [al_r]], [f.obj.HitHR.toFixed(1), [al_r, br]],
                        [(f.obj.BB / f.obj.Pa * 100).toFixed(1), [al_r]], [(f.obj.K / f.obj.Pa * 100).toFixed(1), [al_r, br]],
                        [f.obj.SB.toFixed(1), [al_r]], [f.obj.CS.toFixed(1), [al_r, br]],
                        [f.obj.AVG.toFixed(3), [al_r]], [f.obj.OBP.toFixed(3), [al_r]], [f.obj.SLG.toFixed(3), [al_r]], [f.obj.ISO.toFixed(3), [al_r, br]],
                        [f.obj.wRC.toFixed(0), [al_r]], [f.obj.crOFF.toFixed(1), [al_r]], [f.obj.crBSR.toFixed(1), [al_r]], [f.obj.crDEF.toFixed(1), [al_r]], [f.obj.crWAR.toFixed(1), [al_r]]]; });
                    statsTables.appendChild(hitterStatsViewer.baseElement);
                    pitcherStatsViewer = new SortableStatsViewer(statsPitcher, DB_Prediction_PitcherStats, ["Name", "Org", "Age", "IP", "G", "GS", "ERA", "FIP", "K/9", "BB/9", "HR/9", "RAA", "WAR"], function (f) { return [["<a href='.player?id=".concat(f.player.mlbId, "'>").concat(f.player.firstName + ' ' + f.player.lastName, "</a>"), [al_l, lnk]], [getParentAbbr(f.player.orgId), [al_l, lnk]],
                        [getDateDelta(new Date(f.player.birthYear, f.player.birthMonth, f.player.birthDate), new Date())[0].toString(), [al_r, br]],
                        [formatOutsToIP(f.obj.Outs_RP + f.obj.Outs_SP), [al_r]], [(f.obj.GS + f.obj.GR).toFixed(1), [al_r]], [f.obj.GS.toFixed(1), [al_r, br]],
                        [f.obj.ERA.toFixed(2), [al_r]], [f.obj.FIP.toFixed(2), [al_r, br]],
                        [(f.obj.K / (f.obj.Outs_RP + f.obj.Outs_SP) * 27).toFixed(1), [al_r]], [(f.obj.BB / (f.obj.Outs_RP + f.obj.Outs_SP) * 27).toFixed(1), [al_r]], [(f.obj.HR / (f.obj.Outs_RP + f.obj.Outs_SP) * 27).toFixed(1), [al_r, br]],
                        [f.obj.crRAA.toFixed(1), [al_r]], [f.obj.crWAR.toFixed(1), [al_r]]]; });
                    statsTables.appendChild(pitcherStatsViewer.baseElement);
                    stat_hitter_btn.addEventListener('click', function () {
                        stat_hitter_btn.classList.add('selected');
                        stat_pitcher_btn.classList.remove('selected');
                        hitterStatsViewer.baseElement.classList.remove('hidden');
                        pitcherStatsViewer.baseElement.classList.add('hidden');
                    });
                    stat_pitcher_btn.addEventListener('click', function () {
                        stat_hitter_btn.classList.remove('selected');
                        stat_pitcher_btn.classList.add('selected');
                        hitterStatsViewer.baseElement.classList.add('hidden');
                        pitcherStatsViewer.baseElement.classList.remove('hidden');
                    });
                    stat_hitter_btn.click();
                    _a = SearchBar.bind;
                    return [4, player_search_data];
                case 7:
                    searchBar = new (_a.apply(SearchBar, [void 0, _b.sent()]))();
                    getElementByIdStrict('nav_stats').classList.add('selected');
                    rankings_button.addEventListener('click', function (event) {
                        var mnth = month_select.value;
                        var yr = year_select.value;
                        var model = model_select.value;
                        var level = level_select === null || level_select === void 0 ? void 0 : level_select.value;
                        var team = team_select === null || team_select === void 0 ? void 0 : team_select.value;
                        if (team !== "0")
                            window.location.href = "./stats?year=".concat(yr, "&month=").concat(mnth, "&model=").concat(model, "&level=").concat(level, "&team=").concat(team);
                        else
                            window.location.href = "./stats?year=".concat(yr, "&month=").concat(mnth, "&model=").concat(model, "&level=").concat(level);
                    });
                    return [2];
            }
        });
    });
}
main();
var PageSelector = (function () {
    function PageSelector(numPages, callback) {
        var _this = this;
        this.callback = callback;
        this.baseElement = document.createElement('div');
        this.baseElement.classList.add('page_selector');
        var pageInput = document.createElement('input');
        pageInput.type = "number";
        pageInput.min = "1";
        pageInput.value = pageInput.min;
        pageInput.max = numPages.toString();
        pageInput.addEventListener('change', function (f) { return _this.callback(Number(pageInput.value) - 1); });
        var minPageButton = document.createElement('button');
        minPageButton.innerText = "<<";
        minPageButton.addEventListener('click', function (f) {
            pageInput.value = pageInput.min;
            _this.callback(Number(pageInput.value) - 1);
        });
        var maxPageButton = document.createElement('button');
        maxPageButton.innerText = ">>";
        maxPageButton.addEventListener('click', function (f) {
            pageInput.value = pageInput.max;
            _this.callback(Number(pageInput.value) - 1);
        });
        this.baseElement.appendChild(minPageButton);
        this.baseElement.appendChild(pageInput);
        this.baseElement.appendChild(maxPageButton);
    }
    return PageSelector;
}());
var SortableStatsViewer = (function () {
    function SortableStatsViewer(vars, itemConstructor, titles, fRow, startIdx, numEntriesVisible) {
        if (startIdx === void 0) { startIdx = 1; }
        if (numEntriesVisible === void 0) { numEntriesVisible = 30; }
        var _this = this;
        this.itemConstructor = itemConstructor;
        this.data = vars.map(function (f) {
            f = f;
            return {
                player: new DB_Player(f),
                obj: new itemConstructor(f)
            };
        });
        this.fRow = fRow;
        this.startIdx = startIdx;
        this.numEntriesVisible = numEntriesVisible;
        this.tableElement = document.createElement('table');
        this.tableElement.classList.add('stats_table');
        var tableHeaderText = titles.map(function (f) { return "<td>".concat(f, "</td>"); }).reduce(function (a, b) { return a + b; });
        this.tableElement.innerHTML = "<thead><tr>".concat(tableHeaderText, "</tr></thead>");
        this.tableBody = document.createElement('tbody');
        this.tableElement.appendChild(this.tableBody);
        this.pageSelector = new PageSelector(Math.ceil(this.data.length / this.numEntriesVisible), function (page) { return _this.ChangePage(page); });
        this.baseElement = document.createElement('div');
        this.baseElement.classList.add('sortableStatsViewer');
        this.baseElement.appendChild(this.tableElement);
        this.baseElement.appendChild(this.pageSelector.baseElement);
        this.ChangePage(0);
    }
    SortableStatsViewer.prototype.ChangePage = function (pageIdx) {
        var _a;
        var _this = this;
        if (this.data.length == 0)
            return;
        this.startIdx = pageIdx * this.numEntriesVisible;
        if (this.startIdx >= this.data.length)
            return this.ChangePage(pageIdx - 1);
        var tableRows = this.data.slice(this.startIdx, this.startIdx + this.numEntriesVisible)
            .map(function (f) {
            var tr = document.createElement('tr');
            var row_values = _this.fRow(f);
            row_values.forEach(function (g) {
                var td = document.createElement('td');
                td.innerHTML = g[0];
                g[1].forEach(function (h) { return td.classList.add(h); });
                tr.appendChild(td);
            });
            return tr;
        });
        (_a = this.tableBody).replaceChildren.apply(_a, tableRows);
    };
    return SortableStatsViewer;
}());
var DB_Player = (function () {
    function DB_Player(data) {
        this.mlbId = data['mlbId'];
        this.firstName = data['firstName'];
        this.lastName = data['lastName'];
        this.birthYear = data['birthYear'];
        this.birthMonth = data['birthMonth'];
        this.birthDate = data['birthDate'];
        this.startYear = data['startYear'];
        this.position = data['position'];
        this.status = data['status'];
        this.orgId = data['orgId'];
        this.draftPick = data['draftPick'];
        this.draftRound = data['draftRound'];
        this.draftBonus = data['draftBonus'];
        this.isHitter = data['isHitter'];
        this.isPitcher = data['isPitcher'];
        this.inTraining = data['inTraining'];
    }
    return DB_Player;
}());
var DB_HitterYearStats = (function () {
    function DB_HitterYearStats(data) {
        this.mlbId = data['mlbId'];
        this.levelId = data['levelId'];
        this.year = data['year'];
        this.teamId = data['teamId'];
        this.leagueId = data['leagueId'];
        this.PA = data['PA'];
        this.AVG = data['AVG'];
        this.OBP = data['OBP'];
        this.SLG = data['SLG'];
        this.ISO = data['ISO'];
        this.WRC = data['WRC'];
        this.HR = data['HR'];
        this.BBPerc = data['BBPerc'];
        this.KPerc = data['KPerc'];
        this.SB = data['SB'];
        this.CS = data['CS'];
    }
    return DB_HitterYearStats;
}());
var DB_HitterMonthStats = (function () {
    function DB_HitterMonthStats(data) {
        this.mlbId = data['mlbId'];
        this.levelId = data['levelId'];
        this.year = data['year'];
        this.month = data['month'];
        this.teamId = data['teamId'];
        this.leagueId = data['leagueId'];
        this.PA = data['PA'];
        this.AVG = data['AVG'];
        this.OBP = data['OBP'];
        this.SLG = data['SLG'];
        this.ISO = data['ISO'];
        this.WRC = data['WRC'];
        this.HR = data['HR'];
        this.BBPerc = data['BBPerc'];
        this.KPerc = data['KPerc'];
        this.SB = data['SB'];
        this.CS = data['CS'];
    }
    return DB_HitterMonthStats;
}());
var DB_PitcherYearStats = (function () {
    function DB_PitcherYearStats(data) {
        this.mlbId = data['mlbId'];
        this.levelId = data['levelId'];
        this.year = data['year'];
        this.teamId = data['teamId'];
        this.leagueId = data['leagueId'];
        this.IP = data['IP'];
        this.ERA = data['ERA'];
        this.FIP = data['FIP'];
        this.HR9 = data['HR9'];
        this.BBPerc = data['BBPerc'];
        this.KPerc = data['KPerc'];
        this.GOPerc = data['GOPerc'];
    }
    return DB_PitcherYearStats;
}());
var DB_PitcherMonthStats = (function () {
    function DB_PitcherMonthStats(data) {
        this.mlbId = data['mlbId'];
        this.levelId = data['levelId'];
        this.year = data['year'];
        this.month = data['month'];
        this.teamId = data['teamId'];
        this.leagueId = data['leagueId'];
        this.IP = data['IP'];
        this.ERA = data['ERA'];
        this.FIP = data['FIP'];
        this.HR9 = data['HR9'];
        this.BBPerc = data['BBPerc'];
        this.KPerc = data['KPerc'];
        this.GOPerc = data['GOPerc'];
    }
    return DB_PitcherMonthStats;
}());
var DB_Prediction_HitterStats = (function () {
    function DB_Prediction_HitterStats(data) {
        this.MlbId = data['MlbId'];
        this.Model = data['Model'];
        this.Year = data['Year'];
        this.Month = data['Month'];
        this.LevelId = data['LevelId'];
        this.Pa = data['Pa'];
        this.Hit1B = data['Hit1B'];
        this.Hit2B = data['Hit2B'];
        this.Hit3B = data['Hit3B'];
        this.HitHR = data['HitHR'];
        this.BB = data['BB'];
        this.HBP = data['HBP'];
        this.K = data['K'];
        this.SB = data['SB'];
        this.CS = data['CS'];
        this.ParkRunFactor = data['ParkRunFactor'];
        this.PercC = data['PercC'];
        this.Perc1B = data['Perc1B'];
        this.Perc2B = data['Perc2B'];
        this.Perc3B = data['Perc3B'];
        this.PercSS = data['PercSS'];
        this.PercLF = data['PercLF'];
        this.PercCF = data['PercCF'];
        this.PercRF = data['PercRF'];
        this.PercDH = data['PercDH'];
        this.AVG = data['AVG'];
        this.OBP = data['OBP'];
        this.SLG = data['SLG'];
        this.ISO = data['ISO'];
        this.wRC = data['wRC'];
        this.crOFF = data['crOFF'];
        this.crBSR = data['crBSR'];
        this.crDEF = data['crDEF'];
        this.crWAR = data['crWAR'];
    }
    return DB_Prediction_HitterStats;
}());
var DB_Prediction_PitcherStats = (function () {
    function DB_Prediction_PitcherStats(data) {
        this.mlbId = data['mlbId'];
        this.Model = data['Model'];
        this.Year = data['Year'];
        this.Month = data['Month'];
        this.levelId = data['levelId'];
        this.Outs_SP = data['Outs_SP'];
        this.Outs_RP = data['Outs_RP'];
        this.GS = data['GS'];
        this.GR = data['GR'];
        this.ERA = data['ERA'];
        this.FIP = data['FIP'];
        this.HR = data['HR'];
        this.BB = data['BB'];
        this.HBP = data['HBP'];
        this.K = data['K'];
        this.ParkRunFactor = data['ParkRunFactor'];
        this.SP_Perc = data['SP_Perc'];
        this.RP_Perc = data['RP_Perc'];
        this.crRAA = data['crRAA'];
        this.crWAR = data['crWAR'];
    }
    return DB_Prediction_PitcherStats;
}());
var DB_PlayerModel = (function () {
    function DB_PlayerModel(data) {
        this.mlbId = data['mlbId'];
        this.year = data['year'];
        this.month = data['month'];
        this.modelId = data['modelId'];
        this.isHitter = data['isHitter'];
        this.probsWar = data['probsWar'];
        this.rankWar = data['rankWar'];
    }
    return DB_PlayerModel;
}());
var DB_PlayerRank = (function () {
    function DB_PlayerRank(data) {
        this.mlbId = data['mlbId'];
        this.modelId = data['modelId'];
        this.isHitter = data['isHitter'];
        this.year = data['year'];
        this.month = data['month'];
        this.teamId = data['teamId'];
        this.position = data['position'];
        this.war = data['war'];
        this.rankWar = data['rankWar'];
        this.teamRankWar = data['teamRankWar'];
        this.highestLevel = data['highestLevel'];
    }
    return DB_PlayerRank;
}());
var DB_HitterWarRank = (function () {
    function DB_HitterWarRank(data) {
        this.mlbId = data['mlbId'];
        this.modelId = data['modelId'];
        this.year = data['year'];
        this.month = data['month'];
        this.teamId = data['teamId'];
        this.position = data['position'];
        this.war = data['war'];
        this.rankWar = data['rankWar'];
        this.pa = data['pa'];
    }
    return DB_HitterWarRank;
}());
var DB_PitcherWarRank = (function () {
    function DB_PitcherWarRank(data) {
        this.mlbId = data['mlbId'];
        this.modelId = data['modelId'];
        this.year = data['year'];
        this.month = data['month'];
        this.teamId = data['teamId'];
        this.spWar = data['spWar'];
        this.spIP = data['spIP'];
        this.rpWar = data['rpWar'];
        this.rpIP = data['rpIP'];
        this.spRank = data['spRank'];
        this.rpRank = data['rpRank'];
    }
    return DB_PitcherWarRank;
}());
var DB_TeamRank = (function () {
    function DB_TeamRank(data) {
        this.teamId = data['teamId'];
        this.modelId = data['modelId'];
        this.year = data['year'];
        this.month = data['month'];
        this.highestRank = data['highestRank'];
        this.top10 = data['top10'];
        this.top50 = data['top50'];
        this.top100 = data['top100'];
        this.top200 = data['top200'];
        this.top500 = data['top500'];
        this.rank = data['rank'];
        this.war = data['war'];
    }
    return DB_TeamRank;
}());
var DB_Models = (function () {
    function DB_Models(data) {
        this.modelId = data['modelId'];
        this.name = data['name'];
    }
    return DB_Models;
}());
var DB_PlayerYearPositions = (function () {
    function DB_PlayerYearPositions(data) {
        this.mlbId = data['mlbId'];
        this.year = data['year'];
        this.isHitter = data['isHitter'];
        this.position = data['position'];
    }
    return DB_PlayerYearPositions;
}());
var DB_HomeData = (function () {
    function DB_HomeData(data) {
        this.year = data['year'];
        this.month = data['month'];
        this.rankType = data['rankType'];
        this.modelId = data['modelId'];
        this.isWar = data['isWar'];
        this.mlbId = data['mlbId'];
        this.data = data['data'];
        this.rank = data['rank'];
    }
    return DB_HomeData;
}());
var DB_HomeDataType = (function () {
    function DB_HomeDataType(data) {
        this.type = data['type'];
        this.name = data['name'];
    }
    return DB_HomeDataType;
}());
var rankings_selector = getElementByIdStrict('rankings_selector');
var year_select = getElementByIdStrict('year_select');
var month_select = getElementByIdStrict('month_select');
var model_select = getElementByIdStrict('model_select');
var team_select = document.getElementById('team_select');
var level_select = document.getElementById('level_select');
var rankings_button = getElementByIdStrict('rankings_button');
var rankings_error = getElementByIdStrict('rankings_error');
var endYear = 0;
var endMonth = 0;
function setupSelector(args) {
    endYear = args.endYear;
    endMonth = args.endMonth;
    for (var i = args.startYear; i <= endYear; i++) {
        var opt = document.createElement('option');
        opt.value = i.toString();
        opt.innerText = i.toString();
        year_select.appendChild(opt);
    }
    for (var i = 4; i <= 9; i++) {
        var opt = document.createElement('option');
        opt.value = i.toString();
        opt.innerText = MONTH_CODES[i];
        month_select.appendChild(opt);
    }
    year_select.value = args.year.toString();
    month_select.value = args.month.toString();
    model_select.value = args.modelId.toString();
    year_select.addEventListener('change', selectorEventHandler);
    month_select.addEventListener('change', selectorEventHandler);
    if (team_select !== null) {
        setupTeamSelector(args.startTeam);
        team_select.addEventListener('change', selectorEventHandler);
    }
    if (level_select !== null && args.level !== null) {
        level_select.value = args.level.toString();
        level_select.addEventListener('change', selectorEventHandler);
    }
}
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
    var teams = [{ id: 0, abbr: 'All' }];
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
    if (teamId !== null)
        team_select.value = teamId.toString();
    else
        team_select.value = "0";
}
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
    if (id === 0)
        return "FA";
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
function formatOutsToIP(outs) {
    var ip = outs / 3;
    var full_ip = Math.trunc(ip);
    var partial_ip = Math.trunc((ip - full_ip) * 3);
    return full_ip.toFixed(0) + "." + partial_ip.toFixed(0);
}
function formatModelString(val) {
    return "".concat(val.toFixed(1));
}
var MODEL_VALUES = [1, 2, 3];
var MODEL_STRINGS = ["Base", "Stats Only", "Experimental"];
var org_map = null;
var level_map = { 1: "MLB", 11: "AAA", 12: "AA", 13: "A+", 14: "A", 15: "A-", 16: "Rk", 17: "DSL", 20: "" };
var MONTH_CODES = ["", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
