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
function retrieveJson(filename) {
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
        var hitterObject, hitter;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4, retrieveJson("../../assets/player/h".concat(id, ".json.gz"))];
                case 1:
                    hitterObject = _a.sent();
                    if (hitterObject !== null) {
                        hitter = {
                            firstName: getJsonString(hitterObject, "firstName"),
                            lastName: getJsonString(hitterObject, "lastName"),
                            birthDate: new Date(getJsonNumber(hitterObject, "birthYear"), getJsonNumber(hitterObject, "birthMonth"), getJsonNumber(hitterObject, "birthDate")),
                            signYear: getJsonNumber(hitterObject, "startYear"),
                            draftPick: getJsonNumberNullable(hitterObject, "draftPick"),
                            stats: getHitterStats(hitterObject),
                            models: getHitterModels(hitterObject)
                        };
                        return [2, hitter];
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
        tr.innerHTML = "\n            <td>".concat(f.year, "</td>\n            <td>").concat(f.level, "</td>\n            <td>").concat(f.team, "</td>\n            <td>").concat(f.league, "</td>\n            <td>").concat(f.pa, "</td>\n            <td>").concat(f.avg.toFixed(3), "</td>\n            <td>").concat(f.obp.toFixed(3), "</td>\n            <td>").concat(f.slg.toFixed(3), "</td>\n            <td>").concat(f.iso.toFixed(3), "</td>\n            <td>").concat(f.wrc, "</td>\n            <td>").concat(f.hr, "</td>\n            <td>").concat(f.bbPerc.toFixed(1), "</td>\n            <td>").concat(f.kPerc.toFixed(1), "</td>\n            <td>").concat(f.sb, "</td>\n            <td>").concat(f.cs, "</td>\n        ");
        stats_body.appendChild(tr);
    });
}
function main() {
    return __awaiter(this, void 0, void 0, function () {
        var id, hitter, age;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    id = getQueryParam("id");
                    return [4, loadHitter(id)];
                case 1:
                    hitter = _a.sent();
                    if (hitter !== null) {
                        updateElementText("player_name", "".concat(hitter.firstName, " ").concat(hitter.lastName));
                        age = getDateDelta(hitter.birthDate, new Date());
                        updateElementText("player_age", "".concat(age[0], " years, ").concat(age[1], " months, ").concat(age[2], " days"));
                        if (hitter.draftPick !== null) {
                            updateElementText("player_draft", "#".concat(hitter.draftPick, " Overall, ").concat(hitter.signYear));
                        }
                        updateHitterStats(hitter);
                    }
                    return [2];
            }
        });
    });
}
main();
