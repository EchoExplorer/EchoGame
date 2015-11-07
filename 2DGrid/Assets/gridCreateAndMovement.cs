using UnityEngine;
using System.Collections;

public class gridCreateAndMovement : MonoBehaviour {

	//Grid creation information
	public GameObject[,] grid;
	private int gridSizeX = 5;
	private int gridSizeY = 3;
	private Material pathMaterial;
	private Material wallMaterial;
	private GameObject player;

	public Vector3 pos;  //Determine location of position
	public float speed = 2.0f;  //Speed of the movement

	private Touch initialTouch = new Touch();
	private bool hasSwiped = false;
	private float minSwipeDist = 100f;
	
	// Use this for initialization
	void Start () {
		pathMaterial = new Material (Shader.Find("Diffuse"));
		pathMaterial.SetColor ("_SpecColor", Color.green);
		wallMaterial = new Material (Shader.Find ("Diffuse"));
		wallMaterial.SetColor ("_SpecColor", Color.blue);
		//player = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		//grid = new GameObject[gridSizeX,gridSizeY];
		//initGrid ();
		//setWall (2, 0);
		//setWall (3, 0);
		//setWall (3, 1);
		//setWall (3, 2);

		level1 (5);

		//level2 (5);

		pos = transform.position;  //Set initial position
	}

	//Straight line with wall at end
	//@param n = how far wall is and how far corridor is
	public void level1(int n) {
		gridSizeX = 1;
		gridSizeY = n;
		initGrid ();
		setWall (0, n-1);
	}

	//Straight line with left turn and wall
	//@param n = how far wall is and how far corridor is
	public void level2(int n) {
		gridSizeX = 7;
		gridSizeY = 7;
		initGrid ();
		setWall (1, 1);
		setWall (2, 1);
		setWall (3, 1);

		setWall (1, 3);
		setWall (2, 3);
		setWall (3, 3);

		setWall (4, 1);
		setWall (3, 4);
		setWall (3, 5);

		setWall (1, 6);
		setWall (2, 6);
		setWall (3, 6);

		setWall (4, 6);
		setWall (5, 6);
		setWall (6, 6);

	}


	
	public void initGrid () {
		grid = new GameObject[gridSizeX,gridSizeY];
		for (int i = 0; i < gridSizeX; i++)
		{
			for (int j = 0; j < gridSizeY; j++)
			{
				Debug.Log("Creating a quad");
				GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
				plane.transform.position = new Vector3(i, j, 0); 
				//Rescale the grid pieces
				plane.transform.localScale -= new Vector3(0.05F, 0.05F, 0);
				plane.gameObject.name = "path";
				plane.GetComponent<Renderer>().material.color = Color.gray;
				grid[i,j] = plane;
			}
		}
		GameObject background = GameObject.CreatePrimitive (PrimitiveType.Quad);
		background.GetComponent<Renderer>().material.color = Color.black;
		background.gameObject.name = "background";
		//float backSizeX = (gridSizeX % 2 == 0) ? (((float)gridSizeX) / 2) - 0.5F : ((float)gridSizeX) / 2;
		//float backSizeY = (gridSizeY % 2 == 0) ? (((float)gridSizeY) / 2) - 0.5F : ((float)gridSizeY) / 2;
		if (gridSizeX % 2 == 0 && gridSizeY % 2 == 0) {
			background.transform.position = new Vector3 (gridSizeX/2 - 0.5F, gridSizeY/2 - 0.5F, 0);
		} else if (gridSizeX % 2 == 0 && gridSizeY % 2 != 0) {
			background.transform.position = new Vector3 (gridSizeX/2 - 0.5F, gridSizeY/2, 0);
		} else if (gridSizeX % 2 != 0 && gridSizeY % 2 == 0) {
			background.transform.position = new Vector3 (gridSizeX/2, gridSizeY/2 - 0.5F, 0);
		} else if (gridSizeX % 2 != 0 && gridSizeY % 2 != 0) {
			background.transform.position = new Vector3 (gridSizeX/2, gridSizeY/2, 0);
		}
		//Rescale the background size
		background.transform.localScale += new Vector3 ((gridSizeX - 1) * 1.0F, (gridSizeY - 1) * 1.0F, 0);
	}
	
	public void setWall(int xVal, int yVal) {
		grid [xVal,yVal].gameObject.name = "wall";
		grid [xVal, yVal].GetComponent<Renderer> ().material.color = Color.blue;
	}
	
	
	// Update is called once per frame
	void Update () {

		foreach (Touch t in Input.touches) {
			if (t.phase == TouchPhase.Began) 
			{
				initialTouch = t;
			} 
			else if (t.phase == TouchPhase.Moved && !hasSwiped) 
			{
				float deltaX = initialTouch.position.x - t.position.x;
				float deltaY = initialTouch.position.y - t.position.y;
				float distance = Mathf.Sqrt(Mathf.Pow(deltaX, 2f) + Mathf.Pow(deltaY, 2f));
				
				//really crude estimate, we can do better than this.
				bool swipedSideways = Mathf.Abs(deltaX) > Mathf.Abs (deltaY);
				
				if (distance > minSwipeDist) 
				{
					if (swipedSideways && deltaX > 0) { //swiped left
						Debug.Log ("Left swiped");
						if (pos.x > 0 && pos.x < gridSizeX) {
							if (grid[(int)(pos.x-1), (int)(pos.y)].gameObject.name == "wall") {
								Debug.Log("Detected a collision in the movement script");
								//Probably call another function to handle what to do for collisions (TO DO)
							} else {
								Debug.Log("About to move vector left");
								pos += Vector3.left;
							}
						}

					} 
					else if (swipedSideways && deltaX <= 0) { //swiped right
						if (pos.x < gridSizeX - 1 && pos.x >= 0) {
							if (grid[(int)(pos.x+1), (int)(pos.y)].gameObject.name == "wall") {
								Debug.Log("Detected a collision in the movement script");
								//Probably call another function to handle what to do for collisions (TO DO)
							} else {
								Debug.Log("About to move vector right");
								pos += Vector3.right;
							}
						}
					} 
					else if (!swipedSideways && deltaY > 0) { //swiped down
						if (pos.y > 0 && (pos.y < gridSizeY)) {
							//if (gameGrid.grid[(int)pos.x,(int)(pos.y-1)].gameObject.name == "wall") {
							//	Debug.Log("Detected a collision in the movement script");
							//}
							if (grid[(int)pos.x, (int)(pos.y-1)].gameObject.name == "wall") {
								Debug.Log("Detected a collision in the movement script");
								//Probably call another function to handle what to do for collisions (TO DO)
								
							} else {
								Debug.Log("About to move vector down");
								pos += Vector3.down;
							}
						}
					} 
					else if (!swipedSideways && deltaY <= 0 && transform.position == pos) { //swipped up
						Debug.Log("Swiped up");
						Debug.Log("X: " + pos.x.ToString() + ", Y: " + pos.y.ToString());
						if (pos.y >= 0 && pos.y < gridSizeY - 1) {
							if (grid[(int)pos.x, (int)(pos.y+1)].gameObject.name == "wall") {
								Debug.Log("Detected a collision in the movement script");
								//Probably call another function to handle what to do for collisions (TO DO)
							} else {
								Debug.Log("About to move vector up");
								pos += Vector3.up;
							}
						}
					}
					//transform.position = Vector3.MoveTowards (transform.position, pos, Time.deltaTime * speed);
					transform.position = Vector3.MoveTowards (transform.position, pos, 1.0F);
					Debug.Log ("X: " + pos.x.ToString () + ", Y: " + pos.y.ToString ());
					hasSwiped = true;
				}
				//direction
			} 
			else if (t.phase == TouchPhase.Ended) 
			{
				// rest touch
				initialTouch = new Touch();
				hasSwiped = false;
			}
		}
	}

	//Returns the distance to the nearest wall to the right (in discrete game units)
	/*public int distRight() {
		int dist = 0;
		for (int i = pos.x; i < gridSizeX; i++) {
			if (grid[i,pos.y].gameObject.name == "wall") {
				return dist
			}
		}
		//If no wall found, return -1 indicating no obstacles
		return -1;
	}

	//Returns the distance to the nearest wall to the left (in discrete game units)
	public int distLeft() {
		int dist = 0;
		for (int i = pos.x; i >= 0; i--) {
			if (grid[i,pos.y].gameObject.name == "wall") {
				return dist
			}
		}
		//If no wall found, return -1 indicating no obstacles
		return -1;
	}

	//Returns the distance to the nearest wall to the upward direction (in discrete game units)
	public int distUp() {
		int dist = 0;
		for (int i = pos.y; i < gridSizeY; i++) {
			if (grid[pos.x,i].gameObject.name == "wall") {
				return dist
			}
		}
		//If no wall found, return -1 indicating no obstacles
		return -1;
	}

	//Returns the distance to the nearest wall to the downward direction (in discrete game units)
	public int distDown() {
		int dist = 0;
		for (int i = pos.y; i >= 0; i--) {
			if (grid[pos.x,i].gameObject.name == "wall") {
				return dist
			}
		}
		//If no wall found, return -1 indicating no obstacles
		return -1;
	}*/
}
