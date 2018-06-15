#!/usr/bin/python

#returns the parameters of an HTML POST request in JSON 
# Import modules for CGI handling

import cgi, cgitb

# Create instance of FieldStorage
  
form = cgi.FieldStorage()

# Get data from fields
print "Content-type:text/json\r\n\r\n"
print "{",
keys = form.keys()
for key in keys:
    print "", key, ":\"", form[key].value, "\"", ",",
print "}"