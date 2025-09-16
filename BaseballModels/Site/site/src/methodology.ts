getElementByIdStrict('nav_methodology').classList.add('selected')

async function main()
{
    const player_search_data = retrieveJson('../../assets/player_search.json.gz')
    searchBar = new SearchBar(await player_search_data)
}

main()