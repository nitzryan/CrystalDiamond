async function main()
{
    const player_search_data = retrieveJson('/assets/player_search.json.gz')
    const org_map_promise = retrieveJson("/assets/map.json.gz")
    org_map = await org_map_promise
    searchBar = new SearchBar(await player_search_data)
}

main()