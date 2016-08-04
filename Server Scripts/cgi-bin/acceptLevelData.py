#!/usr/bin/python                                                                                               

import sqlite3, cgi, cgitb

from Crypto.PublicKey import RSA
from base64 import b64decode

fOpen = open('/srv/maze/key.txt', 'r')
longkey = bytes(fOpen.read())

rsakey = RSA.importKey(b64decode(longkey))

def decrypt(decryptThis):
    raw_cipher_data = b64decode(decryptThis)
    return unicode(rsakey.decrypt(raw_cipher_data))

dbName = "gameData"

db = sqlite3.connect('/srv/sqlite/data/' + dbName)
cursor = db.cursor()

# Create instance of FieldStorage                                                                               
levelForm = cgi.FieldStorage()

userName = decrypt(levelForm.getvalue('userName'))
currentLevel = int(decrypt(levelForm.getvalue('currentLevel')))
trackCount = int(decrypt(levelForm.getvalue('trackCount')))
crashCount = int(decrypt(levelForm.getvalue('crashCount')))
stepCount = int(decrypt(levelForm.getvalue('stepCount')))
timeElapsed = float(decrypt(levelForm.getvalue('timeElapsed')))
startTime = decrypt(levelForm.getvalue('startTime'))
endTime = decrypt(levelForm.getvalue('endTime'))
asciiLevelRep = decrypt(levelForm.getvalue('asciiLevelRep'))
levelRecord = levelForm.getvalue('levelRecord')

cursor.execute('''INSERT INTO LevelData(userName, currentLevel, trackCount, crashCount,
stepCount, timeElapsed, startTime, endTime, asciiLevelRep, levelRecord, dateTimeStamp)                           
VALUES(?,?,?,?,?,?,?,?,?,?,CURRENT_TIMESTAMP)''', (userName,
currentLevel, trackCount, crashCount, stepCount, timeElapsed, startTime, endTime, asciiLevelRep, levelRecord))

db.commit() #changes are committed to database                                                                  
db.close()

print "Content-type:text/json\r\n\r\n"
print "{",
keys = levelForm.keys()
for key in keys:
    print key, ":\"", levelForm[key].value, "\"", ",",
print "}"
