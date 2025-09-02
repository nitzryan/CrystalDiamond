import sys

if __name__ == "__main__":
    filename = sys.argv[1]
    
    with open("src/htmlTemplates/banner.html", "r") as file:
        bannerHtml = file.read()
    
    with open(f"src/html/{filename}", "r") as file:
        contents = file.read()
        contents = contents.replace("<!-- BANNER -->", bannerHtml)
        
        with open(f"../server/src/html/{filename}", "w") as outFile:
            outFile.write(contents)