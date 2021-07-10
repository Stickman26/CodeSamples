/*
Author:				Lansingh Freeman
Class :				CSI-281-03
Assignment :		Programming Assignment 2: Hash Table Karaoke
Date Assigned :		November 15, 2018
Due Date :			December 6, 2018 - 11:59 pm
Description :
Using a binary space partioning tree, a two dimensional array is subdivided
into two sections which each have a chance to be split as well, and they continue
recusivly until the minimum size is reached or a split doesn't ocure. It then will
create a room (repesented by Xs) with hallways (represented by 0s) that connect the
children of a parent node together.

Certification of Authenticity :
I  certify  that  this  is  entirely  my  own  work, except  where  I  have  given
fully - documented  references  to  the  work  of  others.I  understand  the
definition and consequences of plagiarism and acknowledge that the assessor
of this assignment may, for the purpose of assessing this assignment :
-Reproduce  this  assignment  and  provide  a  copy  to  another  member  of academic staff; and/or
-Communicate  a  copy  of  this  assignment  to  a  plagiarism  checking
service(which  may  then  retain  a  copy  of  this  assignment  on  its
database for the purpose of future plagiarism checking)
*/
#include "DungeonKeeper.h"

int main()
{
	//Seeds the randoms
	srand(time(0));

	//Varaibles for fstream output
	string doc = "Dungeon.txt";
	ofstream output;

	//Creates the Dungeon
	DungeonKeeper DKP;

	output.open(doc, fstream::trunc);
	output << DKP.printArray();
	output.close();

	//pause
	//string junk;
	//cin >> junk;
}