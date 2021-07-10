/*
Lansingh Freeman
CSI-281-03
*/
#pragma once
#include <iostream>
#include <fstream>
#include <string>
#include <ctime>
//Put in algorithms / BSP tree stuff here

using namespace std;

class DungeonKeeper
{
private:
	
	const static int DEFAULT_DUNGEON_X = 64;
	const static int DEFAULT_DUNGEON_Y = 32;
	const int MIN_SPACE = 8;
	const int MIN_ROOM_DIM = 4;
	const int CHANCE_OF_SPLIT = 100;
	const int CHANCE_REDUCTION = 5;

	struct Zone
	{
		int minX;
		int maxX;
		int minY;
		int maxY;

		Zone(int miX, int maX, int miY, int maY)
		{
			minX = miX;
			maxX = maX;
			minY = miY;
			maxY = maY;
		}
	};

	struct Dode
	{
		Dode* left;
		Dode* right;
		
		Zone* area;
		Zone* room;

		Dode(int miX, int maX, int miY, int maY)
		{
			area = new Zone(miX, maX, miY, maY);
			room = nullptr;

			left = nullptr;
			right = nullptr;
		}
	};

	int sizeOfDungeon;
	char DungeonArray[DEFAULT_DUNGEON_X][DEFAULT_DUNGEON_Y];

	Dode* root;

	void rPartitionSpace(int splitChance, Dode* ptr);
	void deletePartion(Dode* ptr);
	void makeRooms(Dode* ptr);

public:
	DungeonKeeper();
	~DungeonKeeper();

	string printArray();

};