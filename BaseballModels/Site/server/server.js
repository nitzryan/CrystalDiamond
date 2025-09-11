const express = require('express')
const sqlite3 = require('sqlite3').verbose()
const path = require('path')
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
                    dbAll("SELECT * FROM PlayerModel WHERE mlbId=? AND modelName=? ORDER BY YEAR ASC, Month ASC", [req.params.id, "H"])
                ]);
            }
            if (row.isPitcher == 1)
            {
                promisePitcher = Promise.all([
                    dbAll("SELECT * FROM PitcherYearStats WHERE mlbId=? ORDER BY YEAR ASC, LevelId DESC, leagueId", [req.params.id]),
                    dbAll("SELECT * FROM PitcherMonthStats WHERE mlbId=? ORDER BY YEAR ASC, MONTH ASC, LevelId DESC, leagueId", [req.params.id]),
                    dbAll("SELECT * FROM PlayerModel WHERE mlbId=? AND modelName=? ORDER BY YEAR ASC, Month ASC", [req.params.id, "P"])
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

        if (teamId !== undefined)
        {
            db.all(`SELECT pr.*, firstName, lastName, birthYear, birthMonth
                FROM PlayerRank as pr
                INNER JOIN Player as p on pr.mlbId=p.mlbId
                WHERE year=? AND month=? AND teamId=? AND teamRank>=? AND teamRank<=? 
                ORDER BY teamRank ASC`, 
            [year,month,teamId,startRank,endRank], (err, rows) => {
            
                res.json(rows)
            })
        } else {
            db.all(`SELECT pr.*, firstName, lastName, birthYear, birthMonth
                FROM PlayerRank as pr
                INNER JOIN Player as p on pr.mlbId=p.mlbId 
                WHERE year=? AND month=? AND rank>=? AND rank<=? 
                ORDER BY rank ASC`, 
            [year,month,startRank,endRank], (err, rows) => {

                res.json(rows)
            })
        }
    }
    catch (e)
    {
        res.status(500).send("Error in rankingsRequest: " + e)
    }
})

app.listen(port, host, () => {
    console.log(`Server running on port=${port}`)
})