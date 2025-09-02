import sys

def removeImports (val : str) -> str :
    lines = val.split('\n')
    output = ""
    for line in lines:
        if "@import" not in line:
            output += line + "\n"
            
    return output

def getFileContents(filename : str) -> str:
    with open(filename, "r") as file:
        return removeImports(file.read())

if __name__ == "__main__":
    outfilename = sys.argv[1]
    infiles = sys.argv[2].split(",")
    
    with open(f"dist/css/{outfilename}", "w") as outfile:
        outfile.write(getFileContents("src/css/main.css"))
        outfile.write(getFileContents("src/css/banner.css"))
        for infile in infiles:
            outfile.write(getFileContents(f"src/css/{infile}"))