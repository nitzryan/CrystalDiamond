const express = require('express')
const sqlite3 = require('sqlite3').verbose()
const path = require('path');
const { start } = require('repl');
const app = express()
const port = 3000

const db = new sqlite3.Database('../../SiteDb/Site.db', (err) => {
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
                    dbAll("SELECT * FROM HitterStats WHERE mlbId=? ORDER BY YEAR ASC, LevelId DESC, leagueId", [req.params.id]),
                    dbAll("SELECT * FROM PlayerModel WHERE mlbId=? AND modelId=? ORDER BY YEAR ASC, Month ASC", [req.params.id, 0])
                ]);
            }
            if (row.isPitcher == 1)
            {
                promisePitcher = Promise.all([
                    dbAll("SELECT * FROM PitcherStats WHERE mlbId=? ORDER BY YEAR ASC, LevelId DESC, leagueId", [req.params.id]),
                    dbAll("SELECT * FROM PlayerModel WHERE mlbId=? AND modelId=? ORDER BY YEAR ASC, Month ASC", [req.params.id, 1])
                ]);
            }
            if (row.isHitter == 1)
            {
                const [hitStats, hitModels] = await promiseHitter

                row["hit_stats"] = await hitStats;
                row["hit_models"] = hitModels;
            }
            if (row.isPitcher == 1)
            {
                const [pitStats, pitModels] = await promisePitcher

                row["pit_stats"] = await pitStats;
                row["pit_models"] = pitModels;
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
            db.all("SELECT * FROM PlayerRank WHERE year=? AND month=? AND teamId=? AND teamRank>=? AND teamRank<=? ORDER BY teamRank ASC", [year,month,teamId,startRank,endRank], (err, rows) => {
                res.json(rows)
            })
        } else {
            db.all("SELECT * FROM PlayerRank WHERE year=? AND month=? AND rank>=? AND rank<=? ORDER BY rank ASC", [year,month,startRank,endRank], (err, rows) => {
                res.json(rows)
            })
        }
    }
    catch (e)
    {
        res.status(500).send("Error in rankingsRequest: " + e)
    }
})

app.listen(port, () => {
    console.log(`Server running on port=${port}`)
})