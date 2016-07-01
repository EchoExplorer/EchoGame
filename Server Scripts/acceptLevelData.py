#!/usr/bin/python

import sqlite3, cgi, cgitb

dbName = "gameData"

db = sqlite3.connect('/srv/sqlite/data/' + dbName)
cursor = db.cursor()

# Create instance of FieldStorage
levelForm = cgi.FieldStorage()

userName = levelForm.getvalue('userName')
currentLevel = int(levelForm.getvalue('currentLevel'))
crashCount = int(levelForm.getvalue('crashCount'))
stepCount = int(levelForm.getvalue('stepCount'))
timeElapsed = float(levelForm.getvalue('timeElapsed'))
startTime = levelForm.getvalue('startTime')
endTime = levelForm.getvalue('endTime')
asciiLevelRep = levelForm.getvalue('asciiLevelRep')

cursor.execute('''INSERT INTO LevelData(userName, currentLevel, crashCount,
stepCount, timeElapsed,startTime, endTime, asciiLevelRep, dateTimeStamp) 
VALUES(?,?,?,?,?,?,?,?,CURRENT_TIMESTAMP)''', (userName,
currentLevel, crashCount, stepCount, timeElapsed, startTime, endTime, 
asciiLevelRep))

db.commit() #changes are committed to database
db.close()

print "Content-type:text/json\r\n\r\n"
print "{",
keys = levelForm.keys()
for key in keys:
    print key, ":\"", levelForm[key].value, "\"", ",",
print "}"