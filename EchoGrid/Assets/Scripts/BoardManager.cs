﻿using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Text;
using System.IO;

public class BoardManager : MonoBehaviour {

	[Serializable]
	public class Count{
		public int minimum;
		public int maximum;

		public Count (int min, int max) {
			minimum = min;
			maximum = max;
		}
	}

	public enum JunctionType{
		INVALID,
		DEADEND,
		T,
		LL,
		RL,
		CROSS
	}

	public enum Direction{
		FRONT,
		BACK,
		LEFT,
		RIGHT
	}

	public struct echoDistData{
		public int front, back, left, right;
		public JunctionType fType, bType, lType, rType;

		public string all_jun_to_string(){
			string juns;
			juns = jun_to_string (fType) + ", " + jun_to_string (bType) + ", " +
				jun_to_string (lType) + ", " + jun_to_string (rType);
			return juns;
		}

		public string jun_to_string(JunctionType jun){
			if (jun ==  JunctionType.INVALID)
				return "Invalid";
			else if (jun ==  JunctionType.DEADEND)
				return "Deadend";
			else if (jun ==  JunctionType.T)
				return "T-Junction";
			else if (jun ==  JunctionType.LL)
				return "Left L";
			else if (jun ==  JunctionType.RL)
				return "Right L";
			else if (jun ==  JunctionType.CROSS)
				return "Cross";

			return "opps an error";
		}
	}

	int columns = Utilities.MAZE_SIZE;
	int rows = Utilities.MAZE_SIZE;
	public int max_level;

	// number of walls and such
	public GameObject[] floorTiles;
	public GameObject[] wallTiles;
	public GameObject[] outerWallTiles;
	public Count wallCount = new Count (5, 9);	//Lower and upper limit for our random number of walls per level.
	public GameObject exit;

	private Transform boardHolder;
	private List <Vector3> gridPositions = new List<Vector3> ();
	private List <int> wallIdxes = new List<int> ();
	public List <Vector3> wallPositions = new List<Vector3> ();
	private List<Vector3> playerPositions = new List<Vector3> ();
	Vector3 exitPos;

	//Clears our list gridPositions and prepares it to generate a new board.
	void InitialiseList (){
		//Clear our list gridPositions.
		gridPositions.Clear ();

		float scale = (float)Utilities.SCALE_REF / (float)Utilities.MAZE_SIZE;
		
		//Loop through x axis (columns).
		for(int x = -1; x < rows + 1; x++){
			//Within each column, loop through y axis (rows).
			for(int y = -1; y < columns + 1; y++){
				//At each index add a new Vector3 to our list with the x and y coordinates of that position.
				gridPositions.Add (new Vector3((float)y*scale, (float)x*scale, 0f));
			}
		}
	}
		
	
	//Sets up the outer walls and floor (background) of the game board.
	void BoardSetup (){
		//Instantiate Board and set boardHolder to its transform.
		boardHolder = transform.FindChild("Board");
		for (int i = 0; i < boardHolder.childCount; ++i) {
			Destroy (boardHolder.GetChild (i).gameObject);
		}

		float scale = (float)Utilities.SCALE_REF / (float)Utilities.MAZE_SIZE;

		//Loop along x axis, starting from -1 (to fill corner) with floor or outerwall edge tiles.
		for(int x = 0; x <= columns + 1; x++){
			//Loop along y axis, starting from -1 to place floor or outerwall tiles.
			for(int y = 0; y <= rows + 1; y++){
				//Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
				GameObject toInstantiate = floorTiles[Random.Range (0,floorTiles.Length)];
				
				//Check if we current position is at board edge, if so choose a random outer wall prefab from our array of outer wall tiles.
				if(x == 0 || x == columns + 1 || y == 0 || y == rows + 1)
					toInstantiate = outerWallTiles [Random.Range (0, outerWallTiles.Length)];
				
				//Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
				GameObject instance =
					Instantiate (toInstantiate, gridPositions[y*(rows+2) + x], Quaternion.identity) as GameObject;
					//Instantiate (toInstantiate, new Vector3 (x*scale, y*scale, 0f), Quaternion.identity) as GameObject;
				
				//Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
				Vector3 new_scale = instance.transform.localScale;
				new_scale *= scale;
				instance.transform.localScale = new_scale;
				instance.transform.SetParent (boardHolder);
			}
		}
	}

	/* Returns a vector of the location where the exit tile will be placed.
	 * Determined randomly from path locations excluding starting position */
	private Vector3 getRandomVector(List<Vector3> positions) {
		int index = Random.Range (0, positions.Count);
		return positions [index];
	}

	/*TODO(agotsis/wenyuw1) I have a python script that generates random mazes. That's the idea you want to use 
	 * so that there's no hardcoding */
	void setup_level (int level){
		//Clear our list gridPositions.
		wallPositions.Clear ();
		wallIdxes.Clear ();
		playerPositions.Clear ();

		float scale = (float)Utilities.SCALE_REF / (float)Utilities.MAZE_SIZE;

		//clear existing walls and exit
		GameObject wall_parent = transform.FindChild ("Walls").gameObject;
		for (int i = 0; i < wall_parent.transform.childCount; ++i) {
			Destroy (wall_parent.transform.GetChild (i).gameObject);
		}
			
		//Determine a random position for the player on the path
		GameObject player = GameObject.Find("Player");

		//return to level 1 if the index is not correct
		if ((level <= 0) || (level > max_level))
			level = 1;
			
		load_level_from_file ("GameData/levels", level);
		int randomDelta = Random.Range (0, playerPositions.Count);
		if (randomDelta == playerPositions.Count)
			randomDelta = playerPositions.Count - 1;
			
		player.transform.position = playerPositions[randomDelta];

		for(int i = 0; i < wallPositions.Count; i++){
			Vector3 position = wallPositions[i];
			//Choose a random tile from tileArray and assign it to tileChoice
			GameObject tileChoice = wallTiles[0];	
			//Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
			GameObject new_wall = Instantiate(tileChoice, position, Quaternion.identity) as GameObject;
			Vector3 new_scale = new_wall.transform.localScale;
			new_scale *= scale;
			new_wall.transform.localScale = new_scale;
			new_wall.transform.SetParent (wall_parent.transform);
		}
		GameObject new_exit = Instantiate (exit, exitPos, Quaternion.identity) as GameObject;
		new_exit.transform.SetParent (wall_parent.transform);
	}

	//private help function to replace list.contain()
	bool _searchWallIdxes(Vector2 idx){
		//Debug.Log (idx);
		//Debug.Log ("[" + x.ToString() + " " + y.ToString() + "]");
		float threshhold = 0.1f;

		if( (Mathf.Abs(0 - idx.x)< threshhold) || (Mathf.Abs(0 - idx.y)< threshhold) ||
			(Mathf.Abs((columns+1) - idx.x)< threshhold) || (Mathf.Abs((rows+1) - idx.y)< threshhold) )
			return true;

		for (int i = 0; i < wallIdxes.Count; i += 2) {
			if ((Mathf.Abs(wallIdxes [i] - idx.x) <= threshhold) && (Mathf.Abs(wallIdxes [i + 1] - idx.y) <= threshhold)) {
				return true;
			}
		}
		return false;
	}

	//private helper to get the distance from  wall/edge
	Vector2 _getDist(Vector2 gridIdx, Vector2 dir){
		//Vector2 playerPos = gridIdx;
		//Debug.Log("dir is: " + dir.x.ToString() + ", "+ dir.y.ToString());
		while ((gridIdx.x > 0) && (gridIdx.x < columns+1) && (gridIdx.y > 0) && (gridIdx.y < rows+1)) {
			if (_searchWallIdxes (gridIdx))//the first one we met
				break;
			else
				gridIdx += new Vector2 (dir.x, dir.y); 	
		}
		//now gridIdx is either a wall or a border
		//print("result:");
		//print (gridIdx);
		return gridIdx; 
	}
		
	//determine which echo to call
	public echoDistData getEchoDistData(Vector3 playerPos, Vector3 playerFront, Vector3 playerLeft){
		float threshhold = 0.01f;
		int player_y_idx = 0, player_x_idx = 0;

		//get which index of gridPos() player is in
		for (int i = 1; i <= rows; i++) {
			for (int j = 1; j <= columns; ++j) {
				if ((gridPositions [i*(columns+2) + j] - playerPos).magnitude <= threshhold) {
					player_y_idx = i; player_x_idx = j;
					//UnityEngine.Debug.Log ("idx is");
					//UnityEngine.Debug.Log (new Vector2 (j, i));
					break;
				}
			}
		}

		//setup the return value
		echoDistData result = new echoDistData();
		Vector2 gridIdx = new Vector2 (player_x_idx, player_y_idx);
		Vector2 gridTemp;

		gridTemp = _getDist (gridIdx, playerFront);
		result.front = (int)(gridTemp - gridIdx).magnitude;
		result.fType = getJunctionType (gridTemp - new Vector2(playerFront.x, playerFront.y), gridIdx);

		gridTemp = _getDist (gridIdx, -playerFront);
		result.back = (int)(gridTemp - gridIdx).magnitude;
		result.bType = getJunctionType (gridTemp + new Vector2(playerFront.x, playerFront.y), gridIdx);

		//_debug_print_wallidx ();
		//print ("LEFT!");
		gridTemp = _getDist (gridIdx, playerLeft);
		result.left = (int)(gridTemp - gridIdx).magnitude;
		result.lType = getJunctionType (gridTemp - new Vector2(playerLeft.x, playerLeft.y), gridIdx);

		gridTemp = _getDist (gridIdx, -playerLeft);
		result.right = (int)(gridTemp - gridIdx).magnitude;
		result.rType = getJunctionType (gridTemp + new Vector2(playerLeft.x, playerLeft.y), gridIdx);

		return result;
	}

	public JunctionType getJunctionType(Vector2 pos, Vector2 entrance){
		if ( (pos.x <= 0) || (pos.x >= columns + 1) || (pos.y <= 0) || (pos.y >= rows + 1) )
			return JunctionType.INVALID;

		float threshhold = 0.1f;

		if ((pos - entrance).magnitude <= threshhold)
			return JunctionType.DEADEND;
			
		int path_count = 0;
		bool entranceX = true;
		if (Mathf.Abs(pos.x-entrance.x) < threshhold)
			entranceX = false;
		bool xrf = false, xlf = false, ytf = false, ydf = false; 

		if (!_searchWallIdxes (new Vector2 (pos.x + 1, pos.y))) {
			xrf = true;
			path_count += 1;
		}
		if (!_searchWallIdxes (new Vector2 (pos.x - 1, pos.y))) {
			xlf = true;
			path_count += 1;
		}
		if (!_searchWallIdxes (new Vector2 (pos.x, pos.y + 1))) {
			ytf = true;
			path_count += 1;
		}
		if (!_searchWallIdxes (new Vector2 (pos.x, pos.y - 1))) {
			ydf = true;
			path_count += 1;
		}

		Debug.Log (path_count);

		if (path_count <= 1)
			return JunctionType.DEADEND;
		else if (path_count == 3)
			return JunctionType.T;
		else if (path_count == 4)//not possible, but I leave it here
			return JunctionType.CROSS;
		else if (path_count == 2) {
			if ( (xrf == ytf) && entranceX)
				return JunctionType.RL;
			else if ( (xrf == ytf) && !entranceX)
				return JunctionType.LL;
			else if ( (xrf != ytf) && entranceX )
				return JunctionType.LL;
			else if ( (xrf != ytf) && !entranceX )
				return JunctionType.RL;
		}
		
		return JunctionType.INVALID;
	}

	void _debug_print_wallidx(){
		string toPrint = "";
		for (int i = 0; i < wallIdxes.Count; i += 2) {
			toPrint += "[" + (wallIdxes [i]).ToString();
			toPrint += " " + (wallIdxes [i + 1]).ToString();
			toPrint += "] ";
		}
		Debug.Log (toPrint);
	}

	//RandomPosition returns a random position from our list gridPositions.
	//WARNING: get rid of the position chosen from the list, be very careful when calling it
	Vector3 RandomPosition (){
		//Declare an integer randomIndex, set it's value to a random number between 0 and the count of items in our List gridPositions.
		int randomIndex = Random.Range (0, gridPositions.Count);
		
		//Declare a variable of type Vector3 called randomPosition, set it's value to the entry at randomIndex from our List gridPositions.
		Vector3 randomPosition = gridPositions[randomIndex];
		
		//Remove the entry at randomIndex from the list so that it can't be re-used.
		gridPositions.RemoveAt (randomIndex);
		
		//Return the randomly selected Vector3 position.
		return randomPosition;
	}
	
	
	//LayoutObjectAtRandom accepts an array of game objects to choose from along with a minimum and maximum range for the number of objects to create.
	void LayoutObjectAtRandom (GameObject[] tileArray, int minimum, int maximum){
		//Choose a random number of objects to instantiate within the minimum and maximum limits
		int objectCount = Random.Range (minimum, maximum+1);
		
		//Instantiate objects until the randomly chosen limit objectCount is reached
		for(int i = 0; i < objectCount; i++){
			//Choose a position for randomPosition by getting a random position from our list of available Vector3s stored in gridPosition
			Vector3 randomPosition = RandomPosition();
			
			//Choose a random tile from tileArray and assign it to tileChoice
			GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
			
			//Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
			Instantiate(tileChoice, randomPosition, Quaternion.identity);
		}
	}
	
	
	//SetupScene initializes our level and calls the previous functions to lay out the game board
	public void SetupScene (int level){
		//Reset our list of gridpositions.
		InitialiseList ();
		//Creates the outer walls and floor.
		BoardSetup ();
		float repeat = 1f;
		level = (int) Mathf.Floor(GameManager.instance.level / repeat);
		setup_level (level+1);
	}

	public bool load_level_from_file(string filename, int level_wanted = 1){
		TextAsset lvldata = Resources.Load (filename)as TextAsset;
		if (lvldata == null) {
			UnityEngine.Debug.Log ("Cannot open file at:");
			UnityEngine.Debug.Log (filename);
			return false;
		}
		string[] lvldata_split = lvldata.text.Split ('\n');
		bool reading_level = false;
		int cur_y = Utilities.MAZE_SIZE-1;//start from top left corner
		float scale = (float)Utilities.SCALE_REF / (float)Utilities.MAZE_SIZE;

		//read through the file until desired level is found
		foreach (string line in lvldata_split) {
			if (line == "END")//reach end of a level layout
				reading_level = false;

			if (reading_level) {//actually loading layout
				//check for valid index
				if (cur_y >= 0) {
					//do things
					for (int i = 0; i < line.Length; ++i) {
						if (line [i] == 'w') {//wall
							wallPositions.Add (gridPositions [(cur_y + 1) * (columns + 2) + (i + 1)]);
							wallIdxes.Add (i + 1);
							wallIdxes.Add (cur_y + 1);
						}
						else if (line [i] == 'e')//exit
							exitPos = gridPositions[(cur_y + 1)*(columns+2) + (i+1)];
						else if (line [i] == 's')//start positions
							playerPositions.Add (gridPositions[(cur_y + 1)*(columns+2) + (i+1)]);
					}
					cur_y -= 1;
				}
			}

			//flow control
			if (line.Length >= 7) {
				if (line.Substring (0, 6) == "LEVEL_") {
					//get the current level we are reading
					int level_reading = Int32.Parse (line.Substring (6, 1));
					if (level_reading == level_wanted)//we found the level we want
									reading_level = true;
				}
			}
		}

		return true;
	}

	public int get_level_count(string filename){
		TextAsset lvldata = Resources.Load (filename)as TextAsset;
		if (lvldata == null) {
			UnityEngine.Debug.Log ("Cannot open file at:");
			UnityEngine.Debug.Log (filename);
			return 0;
		}
		string[] lvldata_split = lvldata.text.Split ('\n');
		bool reading_level = false;
		int cur_y = 7;//start from top left corner

		//read through the file until desired level is found
		int level_count = 0;
		foreach (string line in lvldata_split) {
			//flow control
			if (line.Length >= 7) {
				if (line.Substring (0, 6) == "LEVEL_") {
					//get the current level we are reading
					level_count += 1;
				}
			}
		}
			
		return level_count;
	}

}

//archive
/*
switch(level){
case 1:
			wallPositions.Add (new Vector3 (0f, 1f, 0f));
			wallPositions.Add (new Vector3 (1f, 1f, 0f));
			wallPositions.Add (new Vector3 (2f, 1f, 0f));
			wallPositions.Add (new Vector3 (3f, 1f, 0f));
			wallPositions.Add (new Vector3 (4f, 1f, 0f));
			wallPositions.Add (new Vector3 (5f, 1f, 0f));
			wallPositions.Add (new Vector3 (6f, 1f, 0f));
			wallPositions.Add (new Vector3 (7f, 1f, 0f));
			wallPositions.Add (new Vector3 (7f, 0f, 0f));
			exitPos = new Vector3 (6f, 0f, 0f);
	player.transform.position = new Vector3(randomDelta, 0, 0);
	break;
case 2:

			wallPositions.Add (new Vector3 (0f, 1f, 0f));
			wallPositions.Add (new Vector3 (1f, 1f, 0f));
			wallPositions.Add (new Vector3 (2f, 1f, 0f));
			wallPositions.Add (new Vector3 (3f, 1f, 0f));
			wallPositions.Add (new Vector3 (4f, 1f, 0f));
			wallPositions.Add (new Vector3 (5f, 1f, 0f));
			wallPositions.Add (new Vector3 (6f, 1f, 0f));

			wallPositions.Add (new Vector3 (6f, 1f, 0f));
			wallPositions.Add (new Vector3 (6f, 2f, 0f));
			wallPositions.Add (new Vector3 (6f, 3f, 0f));
			wallPositions.Add (new Vector3 (6f, 4f, 0f));
			wallPositions.Add (new Vector3 (6f, 5f, 0f));
			wallPositions.Add (new Vector3 (6f, 6f, 0f));
			wallPositions.Add (new Vector3 (6f, 7f, 0f));
			exitPos = new Vector3 (7f, 7f, 0f);

	player.transform.position = new Vector3(randomDelta, 0, 0);
	break;
case 3:
	wallPositions.Add (new Vector3 (1f, 6f, 0f));
	wallPositions.Add (new Vector3 (2f, 6f, 0f));
	wallPositions.Add (new Vector3 (3f, 6f, 0f));
	wallPositions.Add (new Vector3 (4f, 6f, 0f));
	wallPositions.Add (new Vector3 (5f, 6f, 0f));
	wallPositions.Add (new Vector3 (6f, 6f, 0f));
	wallPositions.Add (new Vector3 (7f, 6f, 0f));

	wallPositions.Add (new Vector3 (1f, 0f, 0f));
	wallPositions.Add (new Vector3 (1f, 1f, 0f));
	wallPositions.Add (new Vector3 (1f, 2f, 0f));
	wallPositions.Add (new Vector3 (1f, 3f, 0f));
	wallPositions.Add (new Vector3 (1f, 4f, 0f));
	wallPositions.Add (new Vector3 (1f, 5f, 0f));
	wallPositions.Add (new Vector3 (1f, 6f, 0f));
	exitPos = new Vector3 (0f, 0f, 0f);
	player.transform.position = new Vector3(randomDelta, 7, 0);
	break;
default:
	wallPositions.Add (new Vector3 (0f, 1f, 0f));
	wallPositions.Add (new Vector3 (1f, 1f, 0f));
	wallPositions.Add (new Vector3 (2f, 1f, 0f));
	wallPositions.Add (new Vector3 (3f, 1f, 0f));
	wallPositions.Add (new Vector3 (4f, 1f, 0f));
	wallPositions.Add (new Vector3 (5f, 1f, 0f));
	wallPositions.Add (new Vector3 (6f, 1f, 0f));
	wallPositions.Add (new Vector3 (7f, 1f, 0f));
	wallPositions.Add (new Vector3 (7f, 0f, 0f));
	exitPos = new Vector3 (6f, 0f, 0f);
	player.transform.position = new Vector3(randomDelta, 0, 0);
	break;
}
*/
