#!/usr/bin/python

import sqlite3, cgi, cgitb, pickle, random, datetime

def findCode():
  with open('/srv/maze/consent.set', 'rb') as f: #set of already used ID #'s
    consentIDS = pickle.load(f)
  while (True):
    code = random.randint(100000, 999999)
    if code not in consentIDS:
      consentIDS.add(code)
      return code


def storeConsent(code):
  dbName = "gameData"

  db = sqlite3.connect('/srv/sqlite/data/' + dbName)
  cursor = db.cursor()

  time = str(datetime.datetime.now())

  cursor.execute('''INSERT INTO ConsentData(surveyID, dateTimeStamp)
  VALUES(?,?)''', (code, time))

  db.commit() #changes are committed to database
  db.close()

def main():
  head = """<html lang="en">
      <head>
          <meta charset="utf-8">
          <meta http-equiv="X-UA-Compatible" content="IE=edge">
          <meta name="viewport" content="width=device-width, initial-scale=1">
          <title>Echolocation Lab Consent</title>

          <!-- Bootstrap -->
          <link href="../bootstrap/css/bootstrap.min.css" rel="stylesheet">

          <!--[if lt IE 9]>
  <script src="https://oss.maxcdn.com/libs/html5shiv/3.7.0/html5shiv.js"></script>
  <script src="https://oss.maxcdn.com/libs/respond.js/1.4.2/respond.min.js"></script>
  <![endif]-->
      </head>
      <body>

          <div class="container">
          <h1>Echolocation Consent</h1>"""
  tail = """ </div> <!-- /container -->

          <!-- jQuery (necessary for Bootstrap's JavaScript plugins) -->
          <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.11.0/jquery.min.js"></script>
          <!-- Include all compiled plugins (below), or include individual files as needed -->
          <script src="../bootstrap/js/bootstrap.min.js"></script>
      </body>
  </html>"""

  consentForm = cgi.FieldStorage()

  print 'Content-Type: text/html\n'
  print head

  fields = {'agecheck', 'researchcheck', 'understandcheck'}

  if not fields.issubset(consentForm.keys()):
    print "<h3> Please go back and fill out the form completely.</h3>"
    print tail
    exit()

  researchcheck = consentForm['researchcheck'].value
  agecheck = consentForm['agecheck'].value
  understandcheck = consentForm['understandcheck'].value

  if researchcheck == "yes" and researchcheck == agecheck and agecheck == understandcheck:
    code = findCode()
    print "<h3>Enter the following code on the app to proceed: </h3>"
    print "<h2><strong>", code, "<h2>"
    storeConsent(code)
  else:
    print "<h3>Thank you for your time.</h3>"
    print "<p><strong>You do not qualify to participate in this research.</strong></p>"

  print tail

main()