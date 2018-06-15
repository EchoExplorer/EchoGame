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
surveyForm = cgi.FieldStorage()

userName = decrypt(surveyForm.getvalue('userName'))
surveyID = int(decrypt(surveyForm.getvalue('surveyID')))
dateTimeStamp = decrypt(surveyForm.getvalue('dateTimeStamp'))

cursor.execute('''INSERT INTO SurveyIDData(userName, surveyID, dateTimeStamp)
VALUES(?,?,?)''', (userName, surveyID, dateTimeStamp))

db.commit() #changes are committed to database
db.close()

print "Content-type:text/json\r\n\r\n"
print "{",
keys = surveyForm.keys()
for key in keys:
    print key, ":\"", surveyForm[key].value, "\"", ",",
print "}"
