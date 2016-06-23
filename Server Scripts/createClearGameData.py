#!/usr/bin/python

import sqlite3

db = sqlite3.connect('data/gameData')
cursor = db.cursor()
db.commit()

cursor.execute("DROP TABLE IF EXISTS GameData")
cursor.execute('''
            CREATE TABLE GameData(id INTEGER PRIMARY KEY, userName TEXT,        
            currentLevel INTEGER, crashCount INTEGER, stepCount INTEGER,        
            timeElapsed FLOAT, startTime TEXT, endTime TEXT,                    
            dateTimeStamp TIMESTAMP)''')

print "Creation successful!"