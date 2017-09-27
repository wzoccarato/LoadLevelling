#!/bin/bash

if [ $# -ne 1 ];
then
echo "uso: csv2insert <nomefile.csv>"
exit 1;
fi

FNAME=$(basename "$1")
FNAME="${FNAME%.*}"
#FNAME=`echo "$1" | cut -d'.' -f1`
FOUT="$FNAME"_insert.txt

# legge il file csv, e genera insert statements per il database
NUML=`wc -l $1 | awk '{print $1}'`
NUML=$(($NUML-1))

PRE='INSERT INTO [dbo].[LoadLevelling]
	([PRODUCTION_CATEGORY],
	[IND_SEASONAL_STATUS],
	[TCH_WEEK],
	[PLANNING_LEVEL],
	[EVENT],
	[WEEK_PLAN],
	[Ahead],
	[Late],
	[Priority],
	[Capacity],
	[Required],
	[1],[2],[3],[4],
	[PLAN_BU],
	[FLAG_HR],
	[ALLOCATED],
	[NOT_ALLOCATED])
	VALUES'

echo "$PRE">$FOUT
head -"$NUML" $1 | nawk -F";" -v q="'"  '{print "("q$1q","q$2q","q$3q","q"NULL"q","q$5q","q$6q","q$7q","q$9q","q$10q","q$11q","q$12q","q$13q","q$14q","q$15q","q"0"q"),"}'>>$FOUT
tail -1 $1 | nawk -F";" -v q="'"  '{print "("q$1q","q$2q","q$3q","q"NULL"q","q$5q","q$6q","q$7q","q$9q","q$10q","q$11q","q$12q","q$13q","q$14q","q$15q","q"0"q")"}'>>$FOUT
