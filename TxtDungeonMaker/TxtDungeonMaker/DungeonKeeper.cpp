/*
Lansingh Freeman
CSI-281-03
*/
#include "DungeonKeeper.h"

//Default Constructor
DungeonKeeper::DungeonKeeper()
{
	root = new Dode(0,DEFAULT_DUNGEON_X,0,DEFAULT_DUNGEON_Y);

	//Initializing the Array
	for (int x = 0; x < DEFAULT_DUNGEON_X; x++)
		for (int y = 0; y < DEFAULT_DUNGEON_Y; y++)
			DungeonArray[x][y] = '_';

	rPartitionSpace(CHANCE_OF_SPLIT, root);
	makeRooms(root);
	printArray();
}

//Deconstructor
DungeonKeeper::~DungeonKeeper()
{
	deletePartion(root);
}

//Recusive Function to destroy the binary space partition tree
void DungeonKeeper::deletePartion(Dode* ptr)
{
	if (ptr == nullptr)
		return;
	deletePartion(ptr->left);
	deletePartion(ptr->right);
	delete ptr->area;
	delete ptr->room;
	delete ptr;
}

//Recusivly splits the space of the dungeon into smaller Dodes
void DungeonKeeper::rPartitionSpace(int splitChance, Dode* ptr)
{
	if ((rand() % 100) < splitChance)
	{
		int split;

		//Horizontal cut (divides the Y)
		if (rand() % 2 == 0)
		{
			
			//Catch if spliting the area is too small
			int yRange = (ptr->area->maxY - ptr->area->minY) / 2;

			//Catch for if the area is too small to split
			if (yRange < MIN_SPACE * 2)
				return;

			cout << "split Y" << endl;

			//Forms a split based on the size of the Y in this segement
			split = rand() % yRange + 1;

			//Makes sure the rooms don't get too small
			if (split < MIN_SPACE)
				split = MIN_SPACE;

			//Adds the two children of the split
			ptr->left = new Dode(ptr->area->minX, ptr->area->maxX, ptr->area->minY, (ptr->area->minY + split));
			ptr->right = new Dode(ptr->area->minX, ptr->area->maxX, (ptr->area->minY + split + 1), ptr->area->maxY);

		}
		//Vertical cut (divides the X)
		else
		{
			int xRange = (ptr->area->maxX - ptr->area->minX) / 2;

			//Catch for if the area is too small to split
			if (xRange < MIN_SPACE * 2)
				return;

			cout << "split X" << endl;

			//Forms a split based on the size of the X in this segement
			split = rand() % xRange + 1;

			//Makes sure the rooms don't get too small
			if (split < MIN_SPACE)
				split = MIN_SPACE;

			//Adds the two children of the split
			ptr->left = new Dode(ptr->area->minX, (ptr->area->minX + split), ptr->area->minY, ptr->area->maxY);
			ptr->right = new Dode((ptr->area->minX + split + 1), ptr->area->maxX, ptr->area->minY, ptr->area->maxY);
		}

		rPartitionSpace(splitChance - CHANCE_REDUCTION, ptr->left);
		rPartitionSpace(splitChance - CHANCE_REDUCTION, ptr->right);
	}
	//If the split chance doesn't trigger
	else
		return;
}

//Takes the leafs and makes rooms within the areas of the leafs
void DungeonKeeper::makeRooms(Dode* ptr)
{
	//catch for null pointer
	if (ptr == nullptr)
		return;

	makeRooms(ptr->left);
	makeRooms(ptr->right);

	//Check if it is a leaf
	if (ptr->left == nullptr && ptr->right == nullptr)
	{	
		//Ranges for x and y
		int xRange = ptr->area->maxX - ptr->area->minX;
		int yRange = ptr->area->maxY - ptr->area->minY;

		//Picks a top left and bottom right point
		int xMin = rand() % xRange + ptr->area->minX;
		int yMin = rand() % yRange + ptr->area->minY;
		int xMax = rand() % xRange + ptr->area->minX;
		int yMax = rand() % yRange + ptr->area->minY;

		//Catches if xMin is actually less than xMax
		if (xMin > xMax)
		{
			int temp = xMin;
			xMin = xMax;
			xMax = temp;
		}

		//Catches if yMin is actually less than yMax
		if (yMin > yMax)
		{
			int temp = yMin;
			yMin = yMax;
			yMax = temp;
		}

		//Checks if the rooms are too small
		if (xMax - xMin < MIN_ROOM_DIM)
		{
			//Checks where there is space to place a minimum sized room
			if (xMin + MIN_ROOM_DIM <= ptr->area->maxX)
				xMax = xMin + MIN_ROOM_DIM;
			else if (xMax - MIN_ROOM_DIM >= ptr->area->minX)
				xMin = xMax - MIN_ROOM_DIM;
			else
			{
				//Randomly assigns one away from the max or min of the space
				//0 means it will be placed one away from the min
				if (rand() % 2 == 0)
				{
					xMin = ptr->area->minX + 1;
					xMax = xMin + MIN_ROOM_DIM;
				}
				//1 means it will be placed one away from the max
				else
				{
					xMax = ptr->area->maxX - 1;
					xMin = xMax - MIN_ROOM_DIM;
				}
			}
		}

		while (yMax - yMin < MIN_ROOM_DIM)
		{
			//Checks where there is space to place a minimum sized room
			if (yMin + MIN_ROOM_DIM <= ptr->area->maxY)
				yMax = yMin + MIN_ROOM_DIM;
			else if (yMax - MIN_ROOM_DIM >= ptr->area->minY)
				yMin = yMax - MIN_ROOM_DIM;
			else
			{
				//Randomly assigns one away from the max or min of the space
				//0 means it will be placed one away from the min
				if (rand() % 2 == 0)
				{
					yMin = ptr->area->minY + 1;
					yMax = yMin + MIN_ROOM_DIM;
				}
				//1 means it will be placed one away from the max
				else
				{
					yMax = ptr->area->maxY - 1;
					yMin = yMax - MIN_ROOM_DIM;
				}
			}
		}

		//Stores Room information
		ptr->room = new Zone(xMin,xMax,yMin,yMax);


		//Runs a loop for adding a room based on the scale in the X and Y, as well as the position of the room (top left)
		for (int x = ptr->room->minX; x < ptr->room->maxX; x++)
			for (int y = ptr->room->minY; y < ptr->room->maxY; y++)
				DungeonArray[x][y] = 'X';
		
	}

	//If it is a parent node
	else
	{
		//Compare the left and right children to see if there is overlap between rooms
		Zone* leftChild = ptr->left->room;
		Zone* rightChild = ptr->right->room;

		int maxBound, minBound, startPoint, endPoint, hallPoint;

		/*
		//Check for x overlap
		if (rightChild->room->maxX >= leftChild->room->maxX >= rightChild->room->minX || rightChild->room->maxX >= leftChild->room->minX >= rightChild->room->minX)
		{
			cout << "you here?" << endl;
			//Select the range of the x overlap
			if (rightChild->room->maxX < leftChild->room->maxX)
				maxBound = rightChild->room->maxX;
			else
				maxBound = leftChild->room->maxX;

			if (rightChild->room->minX < leftChild->room->minX)
				minBound = rightChild->room->minX;
			else
				minBound = leftChild->room->minX;

			hallPoint = rand() % (maxBound - minBound) + minBound;

			//Determine the start and end points for the hall
			if (rightChild->room->maxY < leftChild->room->maxY)
			{
				startPoint = leftChild->room->maxY;
				endPoint = rightChild->room->maxY;
			}
			else
			{
				startPoint = rightChild->room->maxY;
				endPoint = leftChild->room->maxY;
			}

			for (int z = startPoint; z > endPoint; z--)
			{
				cout << "test";
				DungeonArray[hallPoint][z] = 'X';
			}
			cout << endl;
		}
		//Check for y overlap
		else if (rightChild->room->maxY >= leftChild->room->maxY >= rightChild->room->minY || rightChild->room->maxY >= leftChild->room->minY >= rightChild->room->minY)
		{
			//Select the range of the x overlap
			if (rightChild->room->maxY < leftChild->room->maxY)
				maxBound = rightChild->room->maxY;
			else
				maxBound = leftChild->room->maxY;

			if (rightChild->room->minY < leftChild->room->minY)
				minBound = rightChild->room->minY;
			else
				minBound = leftChild->room->minY;

			hallPoint = rand() % (maxBound - minBound) + minBound;

			//Determine the start and end points for the hall
			if (rightChild->room->maxX < leftChild->room->maxX)
			{
				startPoint = leftChild->room->maxX;
				endPoint = rightChild->room->maxX;
			}
			else
			{
				startPoint = rightChild->room->maxY;
				endPoint = leftChild->room->maxY;
			}

			for (int z = startPoint; z > endPoint; z--)
			{
				cout << "test";
				DungeonArray[z][hallPoint] = 'X';
			}
			cout << endl;
		}
		//Incase there is no overlap
		else
		{
			
		}

		*/

		//Checking for overlap in X
		if (rightChild->minX <= leftChild->maxX && rightChild->minX >= leftChild->minX ||
			leftChild->minX <= rightChild->maxX && leftChild->minX >= rightChild->minX)
		{
			cout << "X overlap found" << endl;

			if (rightChild->maxX < leftChild->maxX)
				maxBound = rightChild->maxX;
			else
				maxBound = leftChild->maxX;

			if (rightChild->minX > leftChild->minX)
				minBound = rightChild->minX;
			else
				minBound = leftChild->minX;

			if (minBound == maxBound)
				maxBound++;

			hallPoint = rand() % (maxBound - minBound) + minBound;
			
			int z = leftChild->maxY;

			//Makes sure the hallway starts on a hall or room
			while (DungeonArray[hallPoint][z] != 'X' && DungeonArray[hallPoint][z] != '0' && z < DEFAULT_DUNGEON_Y)
				z--;

			//Places the start one space from the wall
			z++;

			//Finishes Hallway
			while (DungeonArray[hallPoint][z] != 'X' && DungeonArray[hallPoint][z] != '0' && z < DEFAULT_DUNGEON_Y)
			{
				DungeonArray[hallPoint][z] = '0';
				if (DungeonArray[hallPoint + 1][z] == 'X' || DungeonArray[hallPoint + 1][z] == '0' || 
					DungeonArray[hallPoint - 1][z] == 'X' || DungeonArray[hallPoint - 1][z] == '0')
					break;
				z++;
			}
		}
		
		//Checking for overlap in Y
		else if (rightChild->minY <= leftChild->maxY && rightChild->minY >= leftChild->minY ||
			leftChild->minY <= rightChild->maxY && leftChild->minY >= rightChild->minY)
		{
			cout << "Y overlap found" << endl;

			if (rightChild->maxY < leftChild->maxY)
				maxBound = rightChild->maxY;
			else
				maxBound = leftChild->maxY;

			if (rightChild->minY > leftChild->minY)
				minBound = rightChild->minY;
			else
				minBound = leftChild->minY;

			if (minBound == maxBound)
				maxBound++;

			hallPoint = rand() % (maxBound - minBound) + minBound;

			int z = leftChild->maxX;

			//Makes sure the hallway starts on a hall or room
			while (DungeonArray[z][hallPoint] != 'X' && DungeonArray[z][hallPoint] != '0' && z > 0)
				z--;

			//Places the start one space from the wall
			z++;

			//Finishes Hallway
			while (DungeonArray[z][hallPoint] != 'X' && DungeonArray[z][hallPoint] != '0' && z < DEFAULT_DUNGEON_X)
			{
				DungeonArray[z][hallPoint] = '0';
				if (DungeonArray[z][hallPoint + 1] == 'X' || DungeonArray[z][hallPoint + 1] == '0' ||
					DungeonArray[z][hallPoint - 1] == 'X' || DungeonArray[z][hallPoint - 1] == '0')
					break;
				z++;
			}
		}

		//For when there is no obvious overlap
		else
		{
			cout << "No overlap found" << endl;

			/*
			Zone* top;
			Zone* bot;
			int horzMod, lowerRoomStart, upperRoomStart, lowerSide;

			//Checks if the right room is on top of the left room
			if (rightChild->maxY < leftChild->maxY)
			{
				top = leftChild;
				bot = rightChild;
			}
			else
			{
				top = rightChild;
				bot = leftChild;
			}

			//Checks which side the top room is on compared to the bottom room
			if (top->maxX < bot->maxX)
			{
				horzMod = 1;
				lowerSide = bot->maxY;
			}
			else
			{
				horzMod = -1;
				lowerSide = top->minY;
			}

			upperRoomStart = rand() % (top->maxY - top->minY) + top->minY;
			lowerRoomStart = rand() % (bot->maxX - bot->minX) + bot->minX;

			//Makes hallway for bottom room
			for (int z = lowerSide; z <= lowerRoomStart && z < DEFAULT_DUNGEON_X && z > 0; z += horzMod)
			{
				DungeonArray[z][upperRoomStart] = '1';
			}

			//Makes hallway for top room
			for (int i = bot->maxY; i <= upperRoomStart && i < DEFAULT_DUNGEON_Y; i++)
			{
				DungeonArray[lowerRoomStart][i] = '2';
			}
			*/

			//Pick an arbitratry point using the x of one room and the y of the other
			int xVal = rand() % (rightChild->maxX - rightChild->minX) + rightChild->minX;
			int yVal = rand() % (leftChild->maxY - leftChild->minY) + leftChild->minY;

			//Sets up y hall
			int z = yVal;
			while (DungeonArray[xVal][z] != 'X' && DungeonArray[xVal][z] != '0' && z < DEFAULT_DUNGEON_Y)
			{
				DungeonArray[xVal][z] = '0';
				if (z > rightChild->maxY)
					z--;
				else
					z++;
			}

			//Sets up x hall
			int i = xVal;
			if (xVal > leftChild->maxX)
				i--;
			else
				i++;
			while (DungeonArray[i][yVal] != 'X' && DungeonArray[i][yVal] != '0' && i < DEFAULT_DUNGEON_X && i > 0)
			{
				DungeonArray[i][yVal] = '0';
				if (i > leftChild->maxX)
					i--;
				else
					i++;
			}

		}

		
		//Creates the bounds of the two children rooms and the new hall
		int rMinX, rMaxX, rMinY, rMaxY;
		
		//X-Max
		if (rightChild->maxX < leftChild->maxX)
			rMaxX = leftChild->maxX;
		else
			rMaxX = rightChild->maxX;

		//X-Min
		if (rightChild->minX < leftChild->minX)
			rMinX = rightChild->minX;
		else
			rMinX = leftChild->minX;

		//Y-Max
		if (rightChild->maxY < leftChild->maxY)
			rMaxY = leftChild->maxY;
		else
			rMaxY = rightChild->maxY;

		//Y-Min
		if (rightChild->minY < leftChild->minY)
			rMinY = rightChild->minY;
		else
			rMinY = leftChild->minY;

		ptr->room = new Zone(rMinX, rMaxX, rMinY, rMaxY);
	}
}

//Loops through the array and prints to a string which is outputted
string DungeonKeeper::printArray()
{
	string temp;

	for (int y = 0; y < DEFAULT_DUNGEON_Y; y++)
	{
		for (int x = 0; x < DEFAULT_DUNGEON_X; x++)
			temp += DungeonArray[x][y];
		temp += '\n';
	}

	return temp;
}