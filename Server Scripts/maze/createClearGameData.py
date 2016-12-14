#!/usr/bin/python

import sqlite3, pickle

dbName = "gameData"

db = sqlite3.connect('/srv/sqlite/data/' + dbName)

cursor = db.cursor()
db.commit()

cursor.execute("DROP TABLE IF EXISTS LevelData")
cursor.execute('''
            CREATE TABLE LevelData(id INTEGER PRIMARY KEY, userName TEXT,
            currentLevel INTEGER, trackCount INTEGER, crashCount INTEGER,
            stepCount INTEGER, timeElapsed FLOAT, startTime TEXT, endTime TEXT,
            asciiLevelRep TEXT, levelRecord TEXT, dateTimeStamp TIMESTAMP)''')

cursor.execute("DROP TABLE IF EXISTS EchoData")
cursor.execute('''
            CREATE TABLE EchoData(id INTEGER PRIMARY KEY, userName TEXT,
            currentLevel INTEGER, trackCount INTEGER,  echo TEXT,
            echoLocation TEXT, postEchoAction TEXT, correctAction TEXT,
            dateTimeStamp TEXT)''')

cursor.execute("DROP TABLE IF EXISTS CrashData")
cursor.execute('''
            CREATE TABLE CrashData(id INTEGER PRIMARY KEY, userName TEXT,
            currentLevel INTEGER, trackCount INTEGER, crashNumber INTEGER,
            crashLocation TEXT, dateTimeStamp TEXT)''')

cursor.execute("DROP TABLE IF EXISTS ConsentIDData")
cursor.execute('''
            CREATE TABLE ConsentIDData(id INTEGER PRIMARY KEY, userName TEXT,
            consentID TEXT, dateTimeStamp TEXT)''')

cursor.execute("DROP TABLE IF EXISTS ConsentData")
cursor.execute('''
            CREATE TABLE ConsentData(id INTEGER PRIMARY KEY, consentID TEXT,
            dateTimeStamp TEXT)''')

cursor.execute("DROP TABLE IF EXISTS SurveyIDData")
cursor.execute('''
            CREATE TABLE SurveyIDData(id INTEGER PRIMARY KEY, userName TEXT,
            surveyID TEXT, dateTimeStamp TEXT)''')

cursor.execute("DROP TABLE IF EXISTS SurveyData")
cursor.execute('''
            CREATE TABLE SurveyData(id INTEGER PRIMARY KEY, surveyID TEXT,
            controls TEXT, easy TEXT, echonavigate TEXT, enjoy TEXT,
            frustrating TEXT, hearingimpaired TEXT, hints TEXT,
            instructions TEXT, look TEXT, lost TEXT, playmore TEXT,
            tutorial TEXT, tutorialhelp TEXT, understandecho TEXT,
            visuallyimpaired TEXT, email TEXT, likes TEXT, confusions TEXT,
            suggestions TEXT, dateTimeStamp TEXT)''')

db.close()

consentIDS = set()
with open('/srv/maze/consent.set', 'wb') as f:
    pickle.dump(consentIDS, f)

print "Creation successful!"
