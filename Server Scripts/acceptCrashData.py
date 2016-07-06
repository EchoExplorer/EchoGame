#!/usr/bin/python

import sqlite3, cgi, cgitb

dbName = "gameData"

db = sqlite3.connect('/srv/sqlite/data/' + dbName)
cursor = db.cursor()

# Create instance of FieldStorage
crashForm = cgi.FieldStorage()

userName = crashForm.getvalue('userName')
currentLevel = int(crashForm.getvalue('currentLevel'))
crashNumber = crashForm.getvalue('crashNumber')
crashLocation = crashForm.getvalue('crashLocation')
dateTimeStamp = crashForm.getvalue('dateTimeStamp')

cursor.execute('''INSERT INTO CrashData(userName, currentLevel, crashNumber,
crashLocation, dateTimeStamp) 
VALUES(?,?,?,?,?)''', (userName, currentLevel,
    crashNumber, crashLocation, dateTimeStamp))

db.commit() #changes are committed to database
db.close()

print "Content-type:text/json\r\n\r\n"
print "{",
keys = crashForm.keys()
for key in keys:
    print key, ":\"", crashForm[key].value, "\"", ",",
print "}"
