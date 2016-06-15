#echoMazeGenerate.py
#agotsis

# a command line script to generate mazes for Echolocaiton Project

import random
import math
import argparse
import sys
import os

MAZESIZE = 9 # must be an odd number, in this implementation
ISLANDS = (MAZESIZE + 1)//2 #maze size is 2*ISLANDS -1
DEBUG = False
MINDIST = 8 #minimum distance between entry and exit

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('aLevels', 
        type=int, help="The number of A levels to generate.")
    parser.add_argument('bLevels', 
        type=int, help="The number of B levels to generate.")
    parser.add_argument('cLevels', 
        type=int, help="The number of C levels to generate.")
    parser.add_argument('filename', help="The name of the text file to create.")

    args = parser.parse_args()

    numALevels = args.aLevels
    numBLevels = args.bLevels
    numCLevels = args.cLevels
    filename = os.path.dirname(os.path.realpath(__file__))+os.sep+args.filename 
    filename += ".txt"

    contents = ""

    lvlCounter = 0

    for a in range(numALevels):
        lvlCounter += 1
        maze = makeLevelAMaze()
        contents += levelize(maze, lvlCounter)

    for b in range(numBLevels):
        lvlCounter += 1
        maze, discard = makeLevelBMaze()
        contents += levelize(maze, lvlCounter)

    for c in range(numALevels):
        lvlCounter += 1
        maze = makeLevelCMaze()
        contents += levelize(maze, lvlCounter)

    with open(filename, "wt") as f:
        f.write(contents)

class Struct(object): pass

def maxItemLength(a):
    maxLen = 0
    rows = len(a)
    cols = len(a[0])
    for row in range(rows):
        for col in range(cols):
            checkThis = a[row][col]
            if isinstance(checkThis, Struct): checkThis = checkThis.number
            maxLen = max(maxLen, len(str(checkThis)))
    return maxLen

def print2dList(a):
    if (a == []):
        # So we don't crash accessing a[0]
        print([])
        return
    rows = len(a)
    #print("a", a)
    cols = len(a[0])
    fieldWidth = maxItemLength(a)
    print("[ ", end="")
    for row in range(rows):
        if (row > 0): print("\n  ", end="")
        print("[ ", end="")
        for col in range(cols):
            if (col > 0): print(", ", end="")
            # The next 2 lines print a[row][col] with the given fieldWidth
            formatSpec = "%" + str(fieldWidth) + "s"
            printThis = a[row][col]
            if isinstance(printThis, Struct): printThis = printThis.number
            print(formatSpec % str(printThis), end="")
        print(" ]", end="")
    print("]")

def createBoard(islands):
    if DEBUG: print("HEY", len(islands), len(islands[0]))
    rows,cols = 2*len(islands) - 1, 2*len(islands[0]) - 1
    board = [['-']*cols for row in range(rows)]
    for row in range(len(islands)):
        for col in range(len(islands[0])):
            island = islands[row][col]
            board[2*row][2*col] = 'i' # represents the island
            if DEBUG: print("row", 2*row, "col", 2*col, island.east)
            if (island.east and (2*col + 1 < cols)):
                board[2*row][2*col + 1] = 1
            elif(not island.east and (2*col + 1 < cols)):
                board[2*row][2*col + 1] = 0
            if (island.south and (2*row + 1 < rows)):
                board[2*row + 1][2*col] = 1
            elif(not island.south and (2*row + 1 < rows)):
                board[2*row + 1][2*col] = 0

    if DEBUG: print2dList(board)
    return board

def makeIsland(number):
    island = Struct()
    island.east = island.south = False
    island.number = number
    return island

def makeBlankMaze(rows,cols):
    islands = [[0]*cols for row in range(rows)] #empty 2D list
    counter = 0
    for row in range(rows):
        for col in range(cols):
            islands[row][col] = makeIsland(counter)
            counter+=1
    return islands

def connectIslands(islands):
    rows,cols = len(islands),len(islands[0])
    for i in range(rows*cols-1):
        makeBridge(islands)

def makeBridge(islands):
    rows,cols = len(islands),len(islands[0])
    while True:
        row,col = random.randint(0,rows-1),random.randint(0,cols-1)
        start = islands[row][col]
        if flipCoin(): #try to go east
            if col==cols-1: continue
            target = islands[row][col+1]
            if start.number==target.number: continue
            #the bridge is valid, so 1. connect them and 2. rename them
            start.east = True
            renameIslands(start,target,islands)
        else: #try to go south
            if row==rows-1: continue
            target = islands[row+1][col]
            if start.number==target.number: continue
            #the bridge is valid, so 1. connect them and 2. rename them
            start.south = True
            renameIslands(start,target,islands)
        #only got here if a bridge was made
        return

def renameIslands(i1,i2,islands):
    n1,n2 = i1.number,i2.number
    lo,hi = min(n1,n2), max(n1,n2)
    for row in islands:
        for island in row:
            if island.number==hi: island.number=lo

def flipCoin():
    return random.choice([True, False])

def convertToPrintable(maze):
    rows,cols = len(maze),len(maze[0])
    result = ""
    for row in range(rows):
        curRow = ""
        for col in range(cols):
            tile = maze[row][col]
            symbol = '-' if tile == 0 or tile == '-' else 'w'
            if tile == 's': symbol = 's'
            elif tile == 'e': symbol = 'e'
            curRow += symbol
        result += curRow + '\n'
    return result

def levelize(maze, lvlNumber):
    result = "LEVEL_%d\n" % lvlNumber
    result += convertToPrintable(maze)
    result += "END\n"
    if DEBUG: print(result)
    return result

def standardizeMaze(maze):
    #makes all walls w, all openings -
    rows, cols = len(maze),len(maze[0])
    standard = [[0]*cols for row in range(rows)] #empty 2D list
    for row in range(rows):
        for col in range(cols):
            tile = maze[row][col]
            symbol = 'w' if tile == 0 or tile == '-' else '-'
            standard[row][col] = symbol
    return standard

def makeLevelCMaze():
    maze = makeBlankMaze(ISLANDS, ISLANDS)
    if DEBUG: 
        print("The maze at the beginning.")
        print2dList(maze)
    connectIslands(maze)
    maze = standardizeMaze(createBoard(maze))
    if DEBUG: print2dList(maze)
    return maze

def makeLevelBMaze():
    maze = makeLevelCMaze()
    deadEnds = findDeadEnds(maze)

    solution = None
    mazeComplete = False
    while(not mazeComplete):
        start = random.choice(deadEnds)
        deadEnds.remove(start)
        end = random.choice(deadEnds)
        deadEnds.remove(end)
        startRow, startCol = start
        endRow, endCol = end
        solution = findSolution(maze, startRow, startCol, endRow, endCol)
        if len(solution) >= MINDIST:
            mazeComplete = True
            maze[startRow][startCol] = 's'
            maze[endRow][endCol] = 'e'
    if DEBUG: print2dList(maze)
    if DEBUG: print(solution)
    return maze, solution

def makeLevelAMaze():
    maze, solution = makeLevelBMaze()
    rows, cols = len(maze),len(maze[0])
    for row in range(rows):
        for col in range(cols):
            tile = maze[row][col]
            if tile == '-' and (row, col) not in solution:
                maze[row][col] = 'w'
    if DEBUG: print2dList(maze)
    return maze

def findDeadEnds(maze):
    #returns a list of tuples of dead ends
    rows, cols = len(maze),len(maze[0])
    dirs = [(-1, 0), ( 0, -1), ( 0, +1), (+1, 0)]
    deadEnds = []
    for row in range(rows):
        for col in range(cols):
            tile = maze[row][col]
            if tile == 'w': continue #walls aren't dead ends
            deadEnd = True
            numPaths = 0
            for drow, dcol in dirs:
                checkRow = row + drow
                checkCol = col + dcol
                if ((checkRow < 0) or (checkRow >= rows) or (checkCol < 0) or 
                    (checkCol >= cols) or (maze[checkRow][checkCol] == 'w')):
                    continue #not a path
                else:
                    numPaths += 1
                    if numPaths > 1: #not a dead end
                        deadEnd = False
                        break
            if deadEnd:
                deadEnds.append((row, col))
    if DEBUG: print(deadEnds)
    return deadEnds

def findSolution(maze, startRow, startCol, endRow, endCol):
    rows, cols = len(maze),len(maze[0])
    visited = set()
    def solve(row,col): #adapted from 15-112 notes
        # base cases
        if (row, col) in visited: return False
        visited.add((row,col))
        if (row, col)==(endRow, endCol): return True
        # recursive case
        for drow, dcol in [(-1, 0), ( 0, -1), ( 0, +1), (+1, 0)]:
            if not ((row < 0) or (row >= rows) or (col < 0) or 
                (col >= cols) or (maze[row][col] == 'w')):
                if solve(row + drow, col + dcol): return True
        visited.remove((row,col))
        return False
    return visited if solve(startRow, startCol) else None

if __name__ == '__main__':
    if DEBUG: 
        makeLevelAMaze()
        # makeLevelBMaze()
        # makeLevelCMaze()
        # test = makeLevelCMaze()
        # findDeadEnds(test)
        # levelize(test, 1)
    else:
        main()
        sys.exit(0)