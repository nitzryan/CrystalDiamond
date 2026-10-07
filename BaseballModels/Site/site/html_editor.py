import sys

if __name__ == "__main__":
    filename = sys.argv[1]
    
    with open("src/htmlTemplates/banner.html", "r") as file:
        bannerHtml = file.read()
    
    with open("src/htmlTemplates/model_select_options.html", "r") as file:
        modelSelectOptionsHtml = file.read()
    
    with open("src/htmlTemplates/rankings_table.html", "r") as file:
        rankingsTableHtml = file.read()
    
    with open("src/htmlTemplates/level_select_options.html", "r") as file:
        levelSelectOptionsHtml = file.read()
    
    with open("src/htmlTemplates/Descriptions/draft_players.html", "r") as file:
        descDraftPlayersHtml = file.read()
    with open("src/htmlTemplates/Descriptions/draft_teamrank.html", "r") as file:
        descDraftTeamRankHtml = file.read()
    with open("src/htmlTemplates/Descriptions/draft_teamdraft.html", "r") as file:
        descDraftTeamDraftHtml = file.read()
    
    with open(f"src/html/{filename}", "r") as file:
        contents = file.read()
        contents = contents.replace("<!-- BANNER -->", bannerHtml)
        contents = contents.replace("<!-- MODEL_OPTIONS -->", modelSelectOptionsHtml)
        contents = contents.replace("<!-- RANKINGS TABLE -->", rankingsTableHtml)
        contents = contents.replace("<!-- LEVEL OPTIONS -->", levelSelectOptionsHtml)
        
        contents = contents.replace("<!-- DESC DRAFT PLAYERS -->", descDraftPlayersHtml)
        contents = contents.replace("<!-- DESC DRAFT TEAMRANK -->", descDraftTeamRankHtml)
        contents = contents.replace("<!-- DESC DRAFT TEAMDRAFT -->", descDraftTeamDraftHtml)
        with open(f"../server/src/html/{filename}", "w") as outFile:
            outFile.write(contents)