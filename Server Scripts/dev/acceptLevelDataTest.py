#!/usr/bin/python

import sqlite3, cgi, cgitb, logging

# Create instance of FieldStorage
form = cgi.FieldStorage()

db = sqlite3.connect('data/gameData')
cursor = db.cursor()
db.commit()

logging.basicConfig(filename='acceptLevelDataTest.log')

try:
    userName = " 9984DA33-4B0C-5D1B-8E42-C2691DD06216 ".strip()
    currentLevel = int(" 11 ".strip())
    crashCount = int(" 0 ".strip())
    stepCount = int(" 18 ".strip())
    timeElapsed = float(" 9.076 ".strip())
    startTime = " 06/22/2016 12:41:40 ".strip()
    endTime = " 06/22/2016 12:41:49 ".strip()

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