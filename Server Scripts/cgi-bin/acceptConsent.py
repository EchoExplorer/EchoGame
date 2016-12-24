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
consentForm = cgi.FieldStorage()

userName = decrypt(consentForm.getvalue('userName'))
consentID = int(decrypt(consentForm.getvalue('consentID')))
dateTimeStamp = decrypt(consentForm.getvalue('dateTimeStamp'))

cursor.execute('''INSERT INTO ConsentIDData(userName, consentID, dateTimeStamp)
VALUES(?,?,?)''', (userName, consentID, dateTimeStamp))

db.commit() #changes are committed to database
db.close()

print "Content-type:text/json\r\n\r\n"
print "{",
keys = consentForm.keys()
for key in keys:
    print key, ":\"", consentForm[key].value, "\"", ",",
print "}"
