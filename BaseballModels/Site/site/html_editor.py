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
    
    with open(f"src/html/{filename}", "r") as file:
        contents = file.read()
        contents = contents.replace("<!-- BANNER -->", bannerHtml)
        contents = contents.replace("<!-- MODEL_OPTIONS -->", modelSelectOptionsHtml)
        contents = contents.replace("<!-- RANKINGS TABLE -->", rankingsTableHtml)
        contents = contents.replace("<!-- LEVEL OPTIONS -->", levelSelectOptionsHtml)
        with open(f"../server/src/html/{filename}", "w") as outFile:
            outFile.write(contents)