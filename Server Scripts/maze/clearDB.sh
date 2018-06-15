#!/bin/bash
mv /srv/maze/dataOutput/recent/* /srv/maze/dataOutput/archive/
python /srv/maze/createClearGameData.py
