#!/usr/bin/python

import sqlite3, csv, datetime

rootPath = "http://merichar-dev.eberly.cmu.edu:81/"

time = str(datetime.datetime.now())

db = sqlite3.connect('/srv/sqlite/data/gameData')
cursor = db.cursor()
db.commit()

data = cursor.execute("SELECT * FROM LevelData")
levelOut = "levelOutput " + time + ".csv"
with open('/var/www/' + levelOut, 'wb') as f:
    writer = csv.writer(f)
    writer.writerow(['rowID' ,'userName', 'currentLevel','crashCount',
'stepCount', 'timeElapsed', 'startTime', 'endTime', 'asciiLevelRep', 
'serverDateTimeStamp'])
    writer.writerows(data)

data = cursor.execute("SELECT * FROM EchoData")
echoOut = "echoOutput " + time + ".csv"
with open('/var/www/' + echoOut, 'wb') as f:
    writer = csv.writer(f)
    writer.writerow(['rowID' ,'userName', 'currentLevel','echo',
'echoLocation', 'dateTimeStamp'])
    writer.writerows(data)

data = cursor.execute("SELECT * FROM CrashData")
crashOut = "crashOutput " + time + ".csv"
with open('/var/www/' + crashOut, 'wb') as f:
    writer = csv.writer(f)
    writer.writerow(['rowID' ,'userName', 'currentLevel', 'crashNumber',
'crashLocation', 'dateTimeStamp'])
    writer.writerows(data)

db.close()

print "Content-type:text/html\r\n\r\n"
print "<html>"
print "<head>"
print "<title>Generate CSVs</title>"
print "</head>"
print "<body>"
print "<h1> Success!</h1>" 
print "<a href=\"" + rootPath + levelOut + "\">" + levelOut + "</a><br>"
print "<a href=\"" + rootPath + echoOut + "\">" + echoOut + "</a><br>"
print "<a href=\"" + rootPath  + crashOut + "\">" + crashOut + "</a>"
print "</body>"
print "</html>"