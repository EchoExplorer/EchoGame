#!/bin/bash
rm /srv/maze/dataOutput/last/* > /dev/null #delete the lasts
mv /srv/maze/dataOutput/recent/* /srv/maze/dataOutput/last/ > /dev/null #move the recents to last
rm /srv/maze/dataOutput/last/* > /dev/null #delete the recents
python /srv/maze/makeCSV.py > /dev/null #the newest csv's are in recent

echo "Content-type: text/html"
echo ""

echo '<html>'
echo '<head>'
echo '<title>Success!</title>'
echo '</head>'
echo '<body>'
echo 'Succeeded. Go scp the data. :)'
echo '</body>'
echo '</html>'

exit 0
