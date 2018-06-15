#!/usr/bin/python

import sqlite3, cgi, cgitb

from Crypto.PublicKey import RSA
from base64 import b64decode

fOpen = open('/srv/maze/private.pem', 'r')
privateKey = fOpen.read()

rsakey = RSA.importKey(privateKey)

def decrypt(decryptThis):
    raw_cipher_data = b64decode(decryptThis)
    return unicode(rsakey.decrypt(raw_cipher_data))

dbName = "gameData"

db = sqlite3.connect('/srv/sqlite/data/' + dbName)
cursor = db.cursor()

# Create instance of FieldStorage
crashForm = cgi.FieldStorage()

userName = decrypt(crashForm.getvalue('userName'))
currentLevel = int(decrypt(crashForm.getvalue('currentLevel')))
trackCount = int(decrypt(crashForm.getvalue('trackCount')))
crashNumber = decrypt(crashForm.getvalue('crashNumber'))
crashLocation = decrypt(crashForm.getvalue('crashLocation'))
dateTimeStamp = decrypt(crashForm.getvalue('dateTimeStamp'))

cursor.execute('''INSERT INTO CrashData(userName, currentLevel, trackCount, crashNumber,
crashLocation, dateTimeStamp) 
VALUES(?,?,?,?,?,?)''', (userName, currentLevel, trackCount,
    crashNumber, crashLocation, dateTimeStamp))

db.commit() #changes are committed to database
db.close()

print "Content-type:text/json\r\n\r\n"
print "{",
keys = crashForm.keys()
for key in keys:
    print key, ":\"", crashForm[key].value, "\"", ",",
print "}"
