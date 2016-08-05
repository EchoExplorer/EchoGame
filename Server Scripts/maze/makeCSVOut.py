#!/usr/bin/python

import sqlite3, csv, datetime

rootPath = "http://echolock.andrew.cmu.edu/srvData/"
dataRoot = '/var/www/srvData/'

time = str(datetime.datetime.now())

db = sqlite3.connect('/srv/sqlite/data/gameData')
cursor = db.cursor()
db.commit()

data = cursor.execute("SELECT * FROM LevelData")
levelOut = "levelOutput " + time + ".csv"
with open(dataRoot + levelOut, 'wb') as f:
    writer = csv.writer(f)
    writer.writerow(['rowID' ,'userName', 'currentLevel', 'trackCount', 'crashCount',
                     'stepCount', 'timeElapsed', 'startTime', 'endTime', 'asciiLevelRep', 
                     'levelRecord', 'serverDateTimeStamp'])
    writer.writerows(data)

data = cursor.execute("SELECT * FROM EchoData")
echoOut = "echoOutput " + time + ".csv"
with open(dataRoot + echoOut, 'wb') as f:
    writer = csv.writer(f)
    writer.writerow(['rowID' ,'userName', 'currentLevel', 'trackCount', 'echo',
                     'echoLocation', 'postEchoAction', 'correctAction', 'dateTimeStamp'])
    writer.writerows(data)

data = cursor.execute("SELECT * FROM CrashData")
crashOut = "crashOutput " + time + ".csv"
with open(dataRoot + crashOut, 'wb') as f:
    writer = csv.writer(f)
    writer.writerow(['rowID' ,'userName', 'currentLevel', 'trackCount',  'crashNumber',
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
