const express = require('express')
const sqlite3 = require('sqlite3').verbose()
const path = require('path')
const fs = require('fs').promises;
const favicon = require('serve-favicon')
const app = express()
const port = process.env.PORT || 3000
const host = process.env.HOST || "0.0.0.0"

const db = new sqlite3.Database('assets/Site.db', (err) => {
    if (err) {
        console.error('Error opening database:', err.message);
    } else {
        console.log('Connected to the SQLite database.');
    }
})

app.use(express.json())
app.use('/css', express.static('src/css'))
app.use('/html', express.static('src/html'))
app.use('/js', express.static('src/js'))
app.use('/assets', express.static('assets'))

app.use(favicon(path.join(__dirname, 'assets', 'favicon.ico')))

app.get('/player', (req, res) => {
    res.sendFile(path.join(__dirname, "src/html/player.html"))
})

app.get("/teams/", (req, res) => {
    res.sendFile(path.join(__dirname, "src/html/teams.html"))
})

app.get("/rankings/", (req, res) => {
    res.sendFile(path.join(__dirname, "src/html/rankings.html"))
})

app.get("/methodology/", (req, res) => {
    res.sendFile(path.join(__dirname, "src/html/methodology.html"))
})

app.get('/mlbranks/', (req, res) => {
    res.sendFile(path.join(__dirname, "src/html/mlbrankings.html"))
})

app.get('/', (req, res) => {
    res.sendFile(path.join(__dirname, "src/html/home.html"))
})

const dbAll = (sql, params) => {
    return new Promise((resolve, reject) => {
        db.all(sql, params, (err, rows) => {
            if (err) reject(err);
            else resolve(rows);
        });
    });
};

app.get('/player/:id', async (req, res) => {
    try 
    {
        const rows = await dbAll("SELECT * FROM PLAYER WHERE mlbId=?", [req.params.id]);
        if (rows.length == 1)
        {
            let row = rows[0]
            let promiseHitter = null
            let promisePitcher = null

            if (row.isHitter == 1)
            {
                promiseHitter = Promise.all([
                    dbAll("SELECT * FROM HitterYearStats WHERE mlbId=? ORDER BY YEAR ASC, LevelId DESC, leagueId", [req.params.id]),
                    dbAll("SELECT * FROM HitterMonthStats WHERE mlbId=? ORDER BY YEAR ASC, MONTH ASC, LevelId DESC, leagueId", [req.params.id]),
                    dbAll("SELECT * FROM PlayerModel WHERE mlbId=? AND isHitter=1 ORDER BY YEAR ASC, Month ASC", [req.params.id])
                ]);
            }
            if (row.isPitcher == 1)
            {
                promisePitcher = Promise.all([
                    dbAll("SELECT * FROM PitcherYearStats WHERE mlbId=? ORDER BY YEAR ASC, LevelId DESC, leagueId", [req.params.id]),
                    dbAll("SELECT * FROM PitcherMonthStats WHERE mlbId=? ORDER BY YEAR ASC, MONTH ASC, LevelId DESC, leagueId", [req.params.id]),
                    dbAll("SELECT * FROM PlayerModel WHERE mlbId=? AND isHitter=0 ORDER BY YEAR ASC, Month ASC", [req.params.id])
                ]);
            }
            if (row.isHitter == 1)
            {
                const [hitStats, hitMonthStats, hitModels] = await promiseHitter

                row["hit_stats"] = await hitStats;
                row["hit_month_stats"] = await hitMonthStats;
                row["hit_models"] = await hitModels;
            }
            if (row.isPitcher == 1)
            {
                const [pitStats, pitMonthStats, pitModels] = await promisePitcher

                row["pit_stats"] = await pitStats;
                row["pit_month_stats"] = await pitMonthStats;
                row["pit_models"] = await pitModels;
            }

            res.json(row)
        }
        else
            res.redirect('/404')
    }
    catch (err)
    {
        console.log(err)
        res.redirect('/500')
    }
})

app.get('/rankingsRequest', (req, res) => {
    try {
        const year = req.query.year
        const month = req.query.month
        const startRank = req.query.startRank
        const endRank = req.query.endRank
        const teamId = req.query.teamId
        const model = req.query.model

        if (teamId !== undefined)
        {
            rank_string = "teamRankWar"

            db.all(`SELECT pr.*, firstName, lastName, birthYear, birthMonth
                FROM PlayerRank as pr
                INNER JOIN Player as p on pr.mlbId=p.mlbId
                WHERE year=? AND month=? AND teamId=? AND ${rank_string}>=? AND ${rank_string}<=? AND pr.modelId=?
                ORDER BY ${rank_string} ASC`, 
            [year,month,teamId,startRank,endRank,model], (err, rows) => {
                res.json(rows)
            })
        } else {
            rank_string = "rankWar"

            db.all(`SELECT pr.*, firstName, lastName, birthYear, birthMonth
                FROM PlayerRank as pr
                INNER JOIN Player as p on pr.mlbId=p.mlbId 
                WHERE year=? AND month=? AND ${rank_string}>=? AND ${rank_string}<=? AND pr.modelId=?
                ORDER BY ${rank_string} ASC`, 
            [year,month,startRank,endRank,model], (err, rows) => {
                res.json(rows)
            })
        }
    }
    catch (e)
    {
        res.status(500).send("Error in rankingsRequest: " + e)
    }
})

app.get('/mlbRank', (req, res) => {
    try {
        const year = req.query.year
        const month = req.query.month
        const startRank = req.query.startRank
        const endRank = req.query.endRank
        const teamId = req.query.teamId
        const model= req.query.model
        const reqType = req.query.reqType
        const table = reqType == 1 ? "HitterWarRank" : "PitcherWarRank"
        const rankType = reqType == 1 ? "rankWar" : 
            reqType == 2 ? "spRank" : "rpRank"

        if (teamId !== undefined)
        {
            db.all(`SELECT hwr.*, firstName, lastName, birthYear, birthMonth
                FROM ${table} AS hwr
                INNER JOIN Player AS p ON hwr.mlbId=p.mlbId
                WHERE year=? AND month=? AND teamId=? AND ${rankType}Team>=? AND ${rankType}Team<=? AND modelId=?
                ORDER BY ${rankType}Team ASC`, 
                [year, month, teamId, startRank, endRank, model], (err, rows) => {
                    res.json(rows)
            })
        }
        else
        {
            db.all(`SELECT hwr.*, firstName, lastName, birthYear, birthMonth
                FROM ${table} AS hwr
                INNER JOIN Player AS p ON hwr.mlbId=p.mlbId
                WHERE year=? AND month=? AND ${rankType}>=? AND ${rankType}<=? AND modelId=?
                ORDER BY ${rankType} ASC`, 
                [year, month, startRank, endRank, model], (err, rows) => {
                    res.json(rows)
            })
        }
    }
    catch (e)
    {
        res.status(500).send("Error in mlbRank: " + e)
    }
})

app.get('/teamRanks', (req, res) => {
    try {
        const year = req.query.year
        const month = req.query.month
        const model = req.query.model
        
        db.all(`
            SELECT * FROM TeamRank
            WHERE year=? AND month=? AND modelId=?
            ORDER BY rank ASC
            `,
        [year, month, model], (err, rows) => {
            res.json(rows)
        })
    } catch (e)
    {
        res.status(500).send("Error in teamRanks: " + e)
    }
})

app.get('/homedata', async (req, res) => {
    try {
        const year = req.query.year
        const month = req.query.month
        const model = req.query.model

        let dataPromise = Promise.all([
            dbAll(`SELECT hd.*, p.firstName, p.lastName, p.position, p.orgId FROM HomeData as hd
                INNER JOIN Player as p ON hd.mlbId = p.mlbId
                WHERE hd.year=? AND hd.month=? AND hd.modelId=?`, [year,month,model]),
            dbAll("SELECT * FROM HomeDataType", [])
        ])
        
        const [hd, hdt] = await dataPromise
        res.send({"types": hdt, "data": hd})
    }
    catch (e)
    {
        res.status(500).send("Error in rankingsRequest: " + e)
    }
})

app.get('/brew', async (req, res) => {
    let html = await fs.readFile(path.join(__dirname, "src/html/error.html"), "utf8")
    html = html.replace("<!-- ERROR TEXT -->", `<p class='center_text'>Short and stout</p>`)
    html = html.replace("<!-- ERROR CODE -->", "418")
    res.status(418).type('html').send(html)
})

app.use(async (req, res, next) => {
    let html = await fs.readFile(path.join(__dirname, "src/html/error.html"), "utf8")
    html = html.replace("<!-- ERROR TEXT -->", `<p class='center_text'>Unable to find ${req.originalUrl}</p>`)
    html = html.replace("<!-- ERROR CODE -->", "404")
    res.status(404).type('html').send(html)
})

app.listen(port, host, () => {
    console.log(`Server running on port=${port}`)
})