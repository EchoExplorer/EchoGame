#!/usr/bin/python

import sqlite3, cgi, cgitb, datetime

def main():

  head = """<html lang="en">
      <head>
          <meta charset="utf-8">
          <meta http-equiv="X-UA-Compatible" content="IE=edge">
          <meta name="viewport" content="width=device-width, initial-scale=1">
          <title>Echolocation Lab Survey</title>

          <!-- Bootstrap -->
          <link href="../bootstrap/css/bootstrap.min.css" rel="stylesheet">

          <!--[if lt IE 9]>
  <script src="https://oss.maxcdn.com/libs/html5shiv/3.7.0/html5shiv.js"></script>
  <script src="https://oss.maxcdn.com/libs/respond.js/1.4.2/respond.min.js"></script>
  <![endif]-->
      </head>
      <body>

          <div class="container">
          <h1>Echolocation Survey</h1>"""
  tail = """ </div> <!-- /container -->

          <!-- jQuery (necessary for Bootstrap's JavaScript plugins) -->
          <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.11.0/jquery.min.js"></script>
          <!-- Include all compiled plugins (below), or include individual files as needed -->
          <script src="../bootstrap/js/bootstrap.min.js"></script>
      </body>
  </html>"""

  print 'Content-Type: text/html\n'
  print head

  surveyForm = cgi.FieldStorage()

  if 'surveyID' not in surveyForm or len(surveyForm['surveyID'].value) != 6:
      print "<h3> Please go back and check your surveyID.</h3>"
      print tail
      exit()

  fields = {'controls', 'easy', 'echonavigate', 'enjoy',
              'frustrating', 'hearingimpaired', 'hints',
              'instructions', 'look', 'lost', 'playmore',
              'tutorial', 'tutorialhelp', 'understandecho',
              'visuallyimpaired'}

  if not fields.issubset(surveyForm.keys()):
      print "<h3> Please go back and fill out the survey completely. Only the last 4 text areas are optional.</h3>"
      print tail
      exit()

  dbName = "gameData"

  db = sqlite3.connect('/srv/sqlite/data/' + dbName)
  cursor = db.cursor()

  surveyID = surveyForm['surveyID'].value
  controls = surveyForm['controls'].value
  easy = surveyForm['easy'].value
  echonavigate = surveyForm['echonavigate'].value
  enjoy = surveyForm['enjoy'].value
  frustrating = surveyForm['frustrating'].value
  hearingimpaired = surveyForm['hearingimpaired'].value
  hints = surveyForm['hints'].value
  instructions = surveyForm['instructions'].value
  look = surveyForm['look'].value
  lost = surveyForm['lost'].value
  playmore = surveyForm['playmore'].value
  tutorial = surveyForm['tutorial'].value
  tutorialhelp = surveyForm['tutorialhelp'].value
  understandecho = surveyForm['understandecho'].value
  visuallyimpaired = surveyForm['visuallyimpaired'].value
  email = surveyForm['email'].value
  likes = surveyForm['likes'].value
  confusions = surveyForm['confusions'].value
  suggestions = surveyForm['suggestions'].value

  time = str(datetime.datetime.now())

  cursor.execute('''INSERT INTO SurveyData(surveyID,
            controls, easy, echonavigate, enjoy,
            frustrating, hearingimpaired, hints,
            instructions, look, lost, playmore,
            tutorial, tutorialhelp, understandecho,
            visuallyimpaired, email, likes, confusions,
            suggestions, dateTimeStamp)
            VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)''',
            (surveyID, controls, easy, echonavigate, enjoy, frustrating,
              hearingimpaired, hints, instructions, look, lost, playmore,
              tutorial, tutorialhelp, understandecho, visuallyimpaired, email,
              likes, confusions, suggestions, time))

  db.commit() #changes are committed to database
  db.close()

  print "<h3>Thank you for your time!</h3>"

  print tail

main()