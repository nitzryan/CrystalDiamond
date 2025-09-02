const express = require('express')
const app = express()
const port = 3000

app.use(express.json())
app.use('/css', express.static('src/css'))
app.use('/html', express.static('src/html'))
app.use('/js', express.static('src/js'))
app.use('/assets', express.static('assets'))

app.get('/player/:id', (req, res) => {
    res.sendFile('/html/player.html')
})

app.get("/teams/", (req, res) => {
    res.redirect('/html/teams.html')
})

app.listen(port, () => {
    console.log(`Server running on port=${port}`)
})