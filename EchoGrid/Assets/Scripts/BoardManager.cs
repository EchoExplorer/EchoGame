﻿using UnityEngine;
using System;
using System.Linq;
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
	public GameObject exit;											//Prefab to spawn for exit.
	public Count wallCount = new Count (5, 9);	//Lower and upper limit for our random number of walls per level.

	private Transform boardHolder;
	private List <Vector3> gridPositions = new List<Vector3>();
	private List <Vector3> wallPositions = new List<Vector3>();

	public List <Vector3> pathPositions = new List<Vector3>();

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
		int index = Random.Range (0, positions.Count());
		return positions [index];
	}

	//Straight path
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
			Debug.Log(i);
			Vector3 position = wallPositions[i];
			
			//Choose a random tile from tileArray and assign it to tileChoice
			GameObject tileChoice = wallTiles[0];
			
			//Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
			Instantiate(tileChoice, position, Quaternion.identity);
		}

		//Determine a random position for the player on the path
		pathPositions.Clear ();
		for (int i = 0; i < columns-1; i++) {
			pathPositions.Add(new Vector3(i, 0, 0));
		}
		Vector3 randomPlayerPos = getRandomVector (pathPositions);
		GameObject player = GameObject.Find("Player");
		player.transform.localPosition = randomPlayerPos;

		//Instantiate the exit tile at the end of the straight corridor
		//Instantiate (exit, new Vector3 (columns - 1, rows - 1, 0f), Quaternion.identity);
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
			Debug.Log(i);
			Vector3 position = wallPositions[i];
			
			//Choose a random tile from tileArray and assign it to tileChoice
			GameObject tileChoice = wallTiles[0];
			//tileChoice.tag = "Wall";
			
			//Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
			Instantiate(tileChoice, position, Quaternion.identity);
		}

		//Determine a random position for the player on the path
		pathPositions.Clear ();
		for (int i = 0; i < columns-1; i++) {
			pathPositions.Add(new Vector3(i, 0, 0));
			pathPositions.Add(new Vector3(7, i, 0));
		}
		pathPositions.Add (new Vector3 (7, 0, 0));
		Vector3 randomPlayerPos = getRandomVector (pathPositions);
		GameObject player = GameObject.Find("Player");
		player.transform.localPosition = randomPlayerPos;
		
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
			Debug.Log(i);
			Vector3 position = wallPositions[i];
			
			//Choose a random tile from tileArray and assign it to tileChoice
			GameObject tileChoice = wallTiles[0];
			
			//Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
			Instantiate(tileChoice, position, Quaternion.identity);
		}

		//Determine a random position for the player on the path
		pathPositions.Clear ();
		for (int i = 0; i < columns-1; i++) {
			pathPositions.Add(new Vector3(i, 7, 0));
			pathPositions.Add(new Vector3(0, i, 0));
		}
		pathPositions.Add (new Vector3 (0, 7, 0));
		Vector3 randomPlayerPos = getRandomVector (pathPositions);
		GameObject player = GameObject.Find("Player");
		player.transform.localPosition = randomPlayerPos;
		
	}
	
	//Spiral maze towards center
	void Level4Walls ()
	{
		//Clear our list gridPositions.
		wallPositions.Clear ();
		
		wallPositions.Add (new Vector3 (1f, 6f, 0f));
		wallPositions.Add (new Vector3 (2f, 6f, 0f));
		wallPositions.Add (new Vector3 (3f, 6f, 0f));
		wallPositions.Add (new Vector3 (4f, 6f, 0f));
		wallPositions.Add (new Vector3 (5f, 6f, 0f));
		
		wallPositions.Add (new Vector3 (1f, 0f, 0f));
		wallPositions.Add (new Vector3 (1f, 1f, 0f));
		wallPositions.Add (new Vector3 (1f, 2f, 0f));
		wallPositions.Add (new Vector3 (1f, 3f, 0f));
		wallPositions.Add (new Vector3 (1f, 4f, 0f));
		wallPositions.Add (new Vector3 (1f, 5f, 0f));
		wallPositions.Add (new Vector3 (1f, 6f, 0f));
		
		wallPositions.Add (new Vector3 (7f, 0f, 0f));
		wallPositions.Add (new Vector3 (7f, 1f, 0f));
		wallPositions.Add (new Vector3 (7f, 2f, 0f));
		wallPositions.Add (new Vector3 (7f, 3f, 0f));
		wallPositions.Add (new Vector3 (7f, 4f, 0f));
		wallPositions.Add (new Vector3 (7f, 5f, 0f));
		wallPositions.Add (new Vector3 (7f, 6f, 0f));
		wallPositions.Add (new Vector3 (7f, 7f, 0f));
		
		wallPositions.Add (new Vector3 (2f, 0f, 0f));
		wallPositions.Add (new Vector3 (3f, 0f, 0f));
		wallPositions.Add (new Vector3 (4f, 0f, 0f));
		wallPositions.Add (new Vector3 (5f, 0f, 0f));
		wallPositions.Add (new Vector3 (6f, 0f, 0f));
		
		wallPositions.Add (new Vector3 (5f, 2f, 0f));
		wallPositions.Add (new Vector3 (5f, 3f, 0f));
		wallPositions.Add (new Vector3 (5f, 4f, 0f));
		wallPositions.Add (new Vector3 (5f, 5f, 0f));
		
		wallPositions.Add (new Vector3 (3f, 2f, 0f));
		wallPositions.Add (new Vector3 (4f, 2f, 0f));
		
		wallPositions.Add (new Vector3 (3f, 3f, 0f));
		wallPositions.Add (new Vector3 (3f, 4f, 0f));
		
		
		for(int i = 0; i < wallPositions.Count; i++)
		{
			Debug.Log(i);
			Vector3 position = wallPositions[i];
			
			//Choose a random tile from tileArray and assign it to tileChoice
			GameObject tileChoice = wallTiles[0];
			tileChoice.gameObject.tag = "Wall";

			
			//Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
			Instantiate(tileChoice, position, Quaternion.identity);
		}

		//Determine a random position for the player on the path
		pathPositions.Clear ();
		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < columns; j++) {
				Vector3 pathP = new Vector3(i, j, 0);
				if (!wallPositions.Contains(pathP)) {
					pathPositions.Add(pathP);
				}
			}
		}
		pathPositions.Remove (new Vector3 (4, 3, 0));
		Vector3 randomPlayerPos = getRandomVector (pathPositions);
		GameObject player = GameObject.Find("Player");
		player.transform.localPosition = randomPlayerPos;

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
		
		//Instantiate a random number of wall tiles based on minimum and maximum, at randomized positions.
		//LayoutObjectAtRandom (wallTiles, wallCount.minimum, wallCount.maximum);
		Level4Walls ();

		//Instantiate (exit, new Vector3 (columns - 1, rows - 1, 0f), Quaternion.identity);

	}

}
