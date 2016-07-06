#!/usr/bin/python

import sqlite3

dbName = "gameData"

db = sqlite3.connect('/srv/sqlite/data/' + dbName)

cursor = db.cursor()
db.commit()

cursor.execute("DROP TABLE IF EXISTS LevelData")
cursor.execute('''
            CREATE TABLE LevelData(id INTEGER PRIMARY KEY, userName TEXT,        
            currentLevel INTEGER, crashCount INTEGER, stepCount INTEGER,        
            timeElapsed FLOAT, startTime TEXT, endTime TEXT, asciiLevelRep TEXT,
            levelRecord TEXT, dateTimeStamp TIMESTAMP)''')

cursor.execute("DROP TABLE IF EXISTS EchoData")
cursor.execute('''
            CREATE TABLE EchoData(id INTEGER PRIMARY KEY, userName TEXT,
            currentLevel INTEGER, echo TEXT, echoLocation TEXT,
            dateTimeStamp TEXT)''')

cursor.execute("DROP TABLE IF EXISTS CrashData")
cursor.execute('''
            CREATE TABLE CrashData(id INTEGER PRIMARY KEY, userName TEXT,
            currentLevel INTEGER, crashNumber INTEGER, crashLocation TEXT,
            dateTimeStamp TEXT)''')

print "Creation successful!"
