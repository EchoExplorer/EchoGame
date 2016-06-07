#echoMazeGenerate.py
#agotsis, adapted from code by rmorina

import random
import math
import argparse
import sys
import os

MAZESIZE = 9 # must be an odd number, in this implementation
ISLANDS = (MAZESIZE + 1)//2 #maze size is 2*ISLANDS -1
DEBUG = False

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('levels', help="The number of levels to generate.")
    parser.add_argument('filename', help="The name of the text file to create.")

    args = parser.parse_args()

    numLevels = int(args.levels)
    filename = os.path.dirname(os.path.realpath(__file__))+os.sep+args.filename 
    filename += ".txt"

    contents = ""

    for level in range(numLevels):
        contents += makeLevel(level + 1)

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
    islands = [[0]*cols for row in range(rows)]
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

def convertToPrintable(board):
    rows,cols = len(board),len(board[0])
    result = ""
    for row in range(rows):
        curRow = ""
        for col in range(cols):
            tile = board[row][col]
            symbol = 'w' if tile == 0 or tile == '-' else '-'
            if row == 0 and col == 0: symbol = 's'
            elif row == rows - 1 and col == cols - 1: symbol = 'e'
            curRow += symbol
        result += curRow + '\n'
    return result

def makeLevel(levelNum):
    result = "LEVEL_%d\n" % levelNum
    maze = makeBlankMaze(ISLANDS, ISLANDS)
    if DEBUG: 
        print("The maze at the beginning.")
        print2dList(maze)
    connectIslands(maze)
    board = createBoard(maze)
    result += convertToPrintable(board)
    result += "END\n"
    if DEBUG: print(result)
    return result

if __name__ == '__main__':
    if DEBUG: makeLevel(1)
    else:
        main()
        sys.exit(0)