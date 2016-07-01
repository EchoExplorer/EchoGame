#!/usr/bin/python

import sqlite3, csv

db = sqlite3.connect('/srv/sqlite/data/gameData')
cursor = db.cursor()
db.commit()

data = cursor.execute("SELECT * FROM LevelData")

outputName = "levelOutput.csv"

with open('/var/www/' + outputName, 'wb') as f:
    writer = csv.writer(f)
    writer.writerow(['rowID' ,'userName', 'currentLevel','crashCount',
'stepCount', 'timeElapsed', 'startTime', 'endTime', 'dateTimeStamp'])
    writer.writerows(data)

db.close()

print "Content-type:text/html\r\n\r\n"
print "<html>"
print "<head>"
print "<title>Generate CSV</title>"
print "</head>"
print "<body>"
print "<h1> Success!</h1>" 
print "Click <a href=\"http://merichar-dev.eberly.cmu.edu:81/" + outputName + "\">here</a> to download."
print "</body>"
print "</html>"