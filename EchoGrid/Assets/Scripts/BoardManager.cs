using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour {

	[Serializable]
	public class Count
	{
		public int minimum;
		public int maximum;

		public Count (int min, int max) 
		{
			minimum = min;
			maximum = max;
		}
	}

	public int columns = 8;
	public int rows = 8;

	// number of walls and such
	public GameObject[] floorTiles;
	public GameObject[] wallTiles;
	public GameObject[] outerWallTiles;
	public Count wallCount = new Count (5, 9);	//Lower and upper limit for our random number of walls per level.
	public GameObject exit;

	private Transform boardHolder;
	private List <Vector3> gridPositions = new List<Vector3>();
	public List <Vector3> wallPositions = new List<Vector3>();

	//Clears our list gridPositions and prepares it to generate a new board.
	void InitialiseList ()
	{
		//Clear our list gridPositions.
		gridPositions.Clear ();
		
		//Loop through x axis (columns).
		for(int x = 1; x < columns-1; x++)
		{
			//Within each column, loop through y axis (rows).
			for(int y = 1; y < rows-1; y++)
			{
				//At each index add a new Vector3 to our list with the x and y coordinates of that position.
				gridPositions.Add (new Vector3(x, y, 0f));
			}
		}
	}
	
	
	//Sets up the outer walls and floor (background) of the game board.
	void BoardSetup ()
	{
		//Instantiate Board and set boardHolder to its transform.
		boardHolder = new GameObject ("Board").transform;
		
		//Loop along x axis, starting from -1 (to fill corner) with floor or outerwall edge tiles.
		for(int x = -1; x < columns + 1; x++)
		{
			//Loop along y axis, starting from -1 to place floor or outerwall tiles.
			for(int y = -1; y < rows + 1; y++)
			{
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
	void Level1Walls ()
	{
		//Clear our list gridPositions.
		wallPositions.Clear ();

		wallPositions.Add (new Vector3 (0f, 1f, 0f));
		wallPositions.Add (new Vector3 (1f, 1f, 0f));
		wallPositions.Add (new Vector3 (2f, 1f, 0f));
		wallPositions.Add (new Vector3 (3f, 1f, 0f));
		wallPositions.Add (new Vector3 (4f, 1f, 0f));
		wallPositions.Add (new Vector3 (5f, 1f, 0f));
		wallPositions.Add (new Vector3 (6f, 1f, 0f));
		wallPositions.Add (new Vector3 (7f, 1f, 0f));

		wallPositions.Add (new Vector3 (7f, 0f, 0f));

		for(int i = 0; i < wallPositions.Count; i++)
		{
			Vector3 position = wallPositions[i];
			
			//Choose a random tile from tileArray and assign it to tileChoice
			GameObject tileChoice = wallTiles[0];
			
			//Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
			Instantiate(tileChoice, position, Quaternion.identity);
		}

		Vector3 exitPos = new Vector3 (columns - 2, 0f, 0f);
		Instantiate (exit, exitPos, Quaternion.identity);

		//Determine a random position for the player on the path
		int randomDelta = Random.Range (0, 4);
		GameObject player = GameObject.Find("Player");
		player.transform.localPosition = new Vector3(randomDelta, 0, 0);
	}

	//Straight and left turn
	void Level2Walls ()
	{
		//Clear our list gridPositions.
		wallPositions.Clear ();
		
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
		
		
		for(int i = 0; i < wallPositions.Count; i++)
		{

			Vector3 position = wallPositions[i];
			
			//Choose a random tile from tileArray and assign it to tileChoice
			GameObject tileChoice = wallTiles[0];
			
			//Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
			Instantiate(tileChoice, position, Quaternion.identity);
		}

		Vector3 exitPos = new Vector3 (columns - 1, rows - 1, 0f);
		Instantiate (exit, exitPos, Quaternion.identity);

		//Determine a random position for the player on the path
		int randomDelta = Random.Range (0, 4);
		GameObject player = GameObject.Find("Player");
		player.transform.localPosition = new Vector3(randomDelta, 0, 0);
	}

	//Straight and right turn
	void Level3Walls ()
	{
		//Clear our list gridPositions.
		wallPositions.Clear ();
		
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
		
		
		for(int i = 0; i < wallPositions.Count; i++)
		{
			Vector3 position = wallPositions[i];
			
			//Choose a random tile from tileArray and assign it to tileChoice
			GameObject tileChoice = wallTiles[0];
			
			//Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
			Instantiate(tileChoice, position, Quaternion.identity);
		}

		Vector3 exitPos = new Vector3 (0f, 0f, 0f);
		Instantiate (exit, exitPos, Quaternion.identity);

		//Determine a random position for the player on the path
		int randomDelta = Random.Range (0, 4);
		GameObject player = GameObject.Find("Player");
		player.transform.localPosition = new Vector3(randomDelta, columns - 1, 0);
	}

	//RandomPosition returns a random position from our list gridPositions.
	Vector3 RandomPosition ()
	{
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
	void LayoutObjectAtRandom (GameObject[] tileArray, int minimum, int maximum)
	{
		//Choose a random number of objects to instantiate within the minimum and maximum limits
		int objectCount = Random.Range (minimum, maximum+1);
		
		//Instantiate objects until the randomly chosen limit objectCount is reached
		for(int i = 0; i < objectCount; i++)
		{
			//Choose a position for randomPosition by getting a random position from our list of available Vector3s stored in gridPosition
			Vector3 randomPosition = RandomPosition();
			
			//Choose a random tile from tileArray and assign it to tileChoice
			GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
			
			//Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
			Instantiate(tileChoice, randomPosition, Quaternion.identity);
		}
	}
	
	
	//SetupScene initializes our level and calls the previous functions to lay out the game board
	public void SetupScene (int level)
	{
		//Creates the outer walls and floor.
		BoardSetup ();
		//Reset our list of gridpositions.
		InitialiseList ();
		level = (int) Mathf.Floor(GameManager.instance.level / 3.0f);
		switch (level) 
		{
			case 0:
				Level1Walls();
				break;
			case 1:
				Level2Walls();
				break;
			case 2:
				Level3Walls();
				break;
			default:
				Level1Walls();
				break;
		}
	}

}
