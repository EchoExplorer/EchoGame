using UnityEngine;
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

	public int columns = 8;
	public int rows = 8;
	public int max_level;

	// number of walls and such
	public GameObject[] floorTiles;
	public GameObject[] wallTiles;
	public GameObject[] outerWallTiles;
	public Count wallCount = new Count (5, 9);	//Lower and upper limit for our random number of walls per level.
	public GameObject exit;

	private Transform boardHolder;
	private List <Vector3> gridPositions = new List<Vector3>();
	public List <Vector3> wallPositions = new List<Vector3>();
	private List<Vector3> playerPositions = new List<Vector3>();
	Vector3 exitPos;

	//Clears our list gridPositions and prepares it to generate a new board.
	void InitialiseList (){
		//Clear our list gridPositions.
		gridPositions.Clear ();
		
		//Loop through x axis (columns).
		for(int x = 1; x < columns-1; x++){
			//Within each column, loop through y axis (rows).
			for(int y = 1; y < rows-1; y++){
				//At each index add a new Vector3 to our list with the x and y coordinates of that position.
				gridPositions.Add (new Vector3(x, y, 0f));
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
		
		//Loop along x axis, starting from -1 (to fill corner) with floor or outerwall edge tiles.
		for(int x = -1; x < columns + 1; x++){
			//Loop along y axis, starting from -1 to place floor or outerwall tiles.
			for(int y = -1; y < rows + 1; y++){
				//Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
				GameObject toInstantiate = floorTiles[Random.Range (0,floorTiles.Length)];
				
				//Check if we current position is at board edge, if so choose a random outer wall prefab from our array of outer wall tiles.
				if(x == -1 || x == columns || y == -1 || y == rows)
					toInstantiate = outerWallTiles [Random.Range (0, outerWallTiles.Length)];
				
				//Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
				GameObject instance =
					Instantiate (toInstantiate, new Vector3 (x, y, 0f), Quaternion.identity) as GameObject;
				
				//Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
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
		playerPositions.Clear ();

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
		UnityEngine.Debug.Log (playerPositions.Count);
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
			new_wall.transform.SetParent (wall_parent.transform);
		}
		GameObject new_exit = Instantiate (exit, exitPos, Quaternion.identity) as GameObject;
		new_exit.transform.SetParent (wall_parent.transform);
	}
		
	//RandomPosition returns a random position from our list gridPositions.
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
		//Creates the outer walls and floor.
		BoardSetup ();
		//Reset our list of gridpositions.
		InitialiseList ();
		level = (int) Mathf.Floor(GameManager.instance.level / 3.0f);
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
		int cur_y = 7;//start from top left corner

		//read through the file until desired level is found
		foreach (string line in lvldata_split) {
			if (line == "END")//reach end of a level layout
				reading_level = false;

			if (reading_level) {//actually loading layout
				//check for valid index
				if (cur_y >= 0) {
					//do things
					for (int i = 0; i < line.Length; ++i) {
						if (line [i] == 'w')//wall
							wallPositions.Add (new Vector3 (i, cur_y, 0f));
						else if (line [i] == 'e')//exit
							exitPos = new Vector3 (i, cur_y, 0f);
						else if (line [i] == 's')//start positions
							playerPositions.Add (new Vector3 (i, cur_y, 0f));
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
