#!/usr/bin/python

import sqlite3, cgi, cgitb, logging

# Create instance of FieldStorage
form = cgi.FieldStorage()

db = sqlite3.connect('data/gameData')
cursor = db.cursor()
db.commit()

logging.basicConfig(filename='acceptLevelData.log')

try:
    userName = form.getvalue('userName')
    currentLevel = int(form.getvalue('currentLevel'))
    crashCount = int(form.getvalue('crashCount'))
    stepCount = int(form.getvalue('stepCount'))
    timeElapsed = float(form.getvalue('timeElapsed'))
    startTime = form.getvalue('startTime')
    endTime = form.getvalue('endTime')

    cursor.execute('''INSERT INTO GameData(userName, currentLevel,crashCount,
    stepCount, timeElapsed, startTime, endTime, dateTimeStamp) 
    VALUES(?,?,?,?,?,?,?,CURRENT_TIMESTAMP)''', (userName,
    currentLevel,crashCount, stepCount, timeElapsed, startTime, endTime))

    db.commit() #changes are committed to database
    db.close()
except Exception, e:
    logging.exception("message")

print "Content-type:text/json\r\n\r\n"
print "{",
keys = form.keys()
for key in keys:
    print key, ":\"", form[key].value, "\"", ",",
print "}"