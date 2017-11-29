#!/bin/bash

if [ $# -ne 1 ];
then
echo "uso: csv2insert <nomefile.csv>"
exit 1;
fi

FNAME=$(basename "$1")
FNAME="${FNAME%.*}"
#FNAME=`echo "$1" | cut -d'.' -f1`
FOUT="$FNAME"_insert.sql

# legge il file csv, e genera insert statements per il database
NUML=`wc -l $1 | awk '{print $1}'`
#NUML=$(($NUML-1))  #prende tutte le righe eccetto l'ultima
NUML2=$(($NUML-1)) #di quello che ha, prende tutte le righe tranne la prima

PRE='USE SDG_Consulting

     INSERT INTO [dbo].[LoadLevelling]
	([PRODUCTION_CATEGORY],
	[IND_SEASONAL_STATUS],
	[TCH_WEEK],
	[PLANNING_LEVEL],
	[EVENT],
	[WEEK],
	[F1],[F2],[F3],
	[Ahead],
	[Late],
	[Priority],
	[Capacity],
	[Required],
	[PLAN_BU],
	[FLAG_HR],
	[ALLOCATED])
	VALUES'

# inizia a inserire dal posizionale $2 PRODUCTION_CATEGORY, (il primo e' ID, e viene inserito in automatico)

#echo "$PRE">$FOUT

#
#cat $1 | nawk 'BEGIN{FS=";"}
#n=split($5,arr," ");
#{print "("q substr($2,1,2) q","q$3q","q"000000"q","q arr[1] q","q$6q","q$7q","q$9q","q$10q","q$11q","q$12q","q$13q","q$14q","q$15q","q"0"q"),"
#}'>>$FOUT

tail -"$NUML2" $1 | nawk -v q="'" 'BEGIN{FS=";"}
{
	#[PRODUCTION_CATEGORY],[IND_SEASONAL_STATUS],[TCH_WEEK]
	printf("(\x27%s\x27,\x27%s\x27,\x27%s\x27,",substr($2,1,2),$3,"000000")

	split($5,plan_lev," ")
	split($6,event," ")

	#[PLANNING_LEVEL],[EVENT],[WEEK]
	printf("\x27%s\x27,\x27%s %s %s\x27,\x27%s\x27,",plan_lev[1],event[1],event[2],event[3],$7)

	#[F1],[F2],[F3]
	printf("\x27%s\x27,\x27%s\x27,\x27%s\x27,",0,0,0)

	#[Ahead],[Late],[Priority],[Capacity],[Required]
	printf("\x27%s\x27,\x27%s\x27,\x27%s\x27,\x27%s\x27,\x27%s\x27,",$9,$10,$11,$12,$13)

	#[PLAN_BU],[FLAG_HR],[ALLOCATED]
	PLAN_BU=$14
	if(PLAN_BU=="")
	{
		PLAN_BU="58_LG"
	}
	FLAG_HR=$15
	if(FLAG_HR=="")
	{
		# se il campo è vuoto, lo forza a "NOT DEFINED"
		FLAG_HR="N"
	}
	printf("\x27%s\x27,\x27%s\x27,\x27%s\x27)\n",PLAN_BU,FLAG_HR,"0")
}'>>$FOUT
