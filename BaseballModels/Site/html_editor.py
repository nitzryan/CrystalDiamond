import sys

if __name__ == "__main__":
    filename = sys.argv[1]
    
    with open(f"src/html/{filename}", "r") as file:
        contents = file.read()
        contents = contents.replace("<!-- BANNER -->",
        "<div id=banner>Banner Banner Banner Banner Banner Banner</div>")
        
        with open(f"dist/html/{filename}", "w") as outFile:
            outFile.write(contents)