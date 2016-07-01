#!/usr/bin/python

import sqlite3, cgi, cgitb

dbName = "gameData"

db = sqlite3.connect('/srv/sqlite/data/' + dbName)
cursor = db.cursor()

# Create instance of FieldStorage
echoForm = cgi.FieldStorage()

userName = echoForm.getvalue('userName')
currentLevel = int(echoForm.getvalue('currentLevel'))
echo = echoForm.getvalue('echo')
location = echoForm.getvalue('location')

cursor.execute('''INSERT INTO EchoData(userName, currentLevel, echo,
echoLocation, dateTimeStamp) 
VALUES(?,?,?,?,CURRENT_TIMESTAMP)''', (userName, currentLevel, echo, location))

db.commit() #changes are committed to database
db.close()

print "Content-type:text/json\r\n\r\n"
print "{",
keys = echoForm.keys()
for key in keys:
    print key, ":\"", echoForm[key].value, "\"", ",",
print "}"