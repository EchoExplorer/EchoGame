#!/usr/bin/python

import sqlite3, cgi, cgitb

from Crypto.PublicKey import RSA
from base64 import b64decode

fOpen = open('key.txt', 'r')
longkey = bytes(fOpen.read())

rsakey = RSA.importKey(b64decode(longkey))

def decrypt(decryptThis):
    raw_cipher_data = b64decode(decryptThis)
    return rsakey.decrypt(raw_cipher_data)

dbName = "gameData"

db = sqlite3.connect('/srv/sqlite/data/' + dbName)
cursor = db.cursor()

# Create instance of FieldStorage
echoForm = cgi.FieldStorage()

userName = decrypt(echoForm.getvalue('userName'))
currentLevel = int(decrypt(echoForm.getvalue('currentLevel')))
echo = decrypt(echoForm.getvalue('echo'))
echoLocation = decrypt(echoForm.getvalue('echoLocation'))
dateTimeStamp = decrypt(echoForm.getvalue('dateTimeStamp'))

cursor.execute('''INSERT INTO EchoData(userName, currentLevel, echo,
echoLocation, dateTimeStamp) 
VALUES(?,?,?,?,?)''', (userName, currentLevel, echo, echoLocation, dateTimeStamp))

db.commit() #changes are committed to database
db.close()

print "Content-type:text/json\r\n\r\n"
print "{",
keys = echoForm.keys()
for key in keys:
    print key, ":\"", echoForm[key].value, "\"", ",",
print "}"
