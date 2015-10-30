using UnityEngine;
using System.Collections;

public class gridCreateAndMovement : MonoBehaviour {

	//Grid creation information
	public GameObject[,] grid;
	private int gridSizeX = 5;
	private int gridSizeY = 5;
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
		grid = new GameObject[gridSizeX,gridSizeY];
		initGrid ();
		setWall (2, 0);
		setWall (3, 0);

		pos = transform.position;  //Set initial position
	}
	
	public void initGrid () {
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
		background.transform.position = new Vector3 (gridSizeX/2, gridSizeY/2, 0);
		//Rescale the background size
		background.transform.localScale += new Vector3 ((gridSizeX - 1) * 1.0F, (gridSizeY - 1) * 1.0F, 0);
	}
	
	public void setWall(int xVal, int yVal) {
		grid [xVal,yVal].gameObject.name = "wall";
		//grid [xVal] [yVal].gameObject.AddComponent(wallMaterial);
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
					if (swipedSideways && deltaX > 0 && transform.position == pos) { //swiped left
						if (pos.x > 0) {
							if (grid[(int)(pos.x-1), (int)(pos.y)].gameObject.name == "wall") {
								Debug.Log("Detected a collision in the movement script");
								//Probably call another function to handle what to do for collisions (TO DO)
							} else {
								pos += Vector3.left;
							}
						}

					} 
					else if (swipedSideways && deltaX <= 0 && transform.position == pos) { //swiped right
						if (pos.x < gridSizeX - 1) {
							if (grid[(int)(pos.x+1), (int)(pos.y)].gameObject.name == "wall") {
								Debug.Log("Detected a collision in the movement script");
								//Probably call another function to handle what to do for collisions (TO DO)
							} else {
								pos += Vector3.right;
							}
						}
					} 
					else if (!swipedSideways && deltaY > 0 && transform.position == pos) { //swiped down
						if (pos.y > 0) {
							//if (gameGrid.grid[(int)pos.x,(int)(pos.y-1)].gameObject.name == "wall") {
							//	Debug.Log("Detected a collision in the movement script");
							//}
							if (grid[(int)pos.x, (int)(pos.y-1)].gameObject.name == "wall") {
								Debug.Log("Detected a collision in the movement script");
								//Probably call another function to handle what to do for collisions (TO DO)
								
							} else {
								pos += Vector3.down;
							}
						}
					} 
					else if (!swipedSideways && deltaY <= 0 && transform.position == pos) { //swipped up
						if (pos.y < gridSizeY - 1) {
							if (grid[(int)pos.x, (int)(pos.y+1)].gameObject.name == "wall") {
								Debug.Log("Detected a collision in the movement script");
								//Probably call another function to handle what to do for collisions (TO DO)
							} else {
								pos += Vector3.up;
							}
						}
					}
					
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
		/*
		if (Input.GetKey(KeyCode.DownArrow) && transform.position == pos) {
			if (pos.y > 0) {
				//if (gameGrid.grid[(int)pos.x,(int)(pos.y-1)].gameObject.name == "wall") {
				//	Debug.Log("Detected a collision in the movement script");
				//}
				if (grid[(int)pos.x, (int)(pos.y-1)].gameObject.name == "wall") {
					Debug.Log("Detected a collision in the movement script");
					//Probably call another function to handle what to do for collisions (TO DO)

				} else {
					pos += Vector3.down;
				}
			}
			//transform.Translate (new Vector3 (0, -moveSpeed, 0) * Time.deltaTime);
		}
		
		if (Input.GetKey (KeyCode.UpArrow) && transform.position == pos) {
			if (pos.y < gridSizeY - 1) {
				if (grid[(int)pos.x, (int)(pos.y+1)].gameObject.name == "wall") {
					Debug.Log("Detected a collision in the movement script");
					//Probably call another function to handle what to do for collisions (TO DO)
				} else {
					pos += Vector3.up;
				}
			}
			//transform.Translate (new Vector3 (0, moveSpeed, 0));
		}
		
		if (Input.GetKey (KeyCode.LeftArrow) && transform.position == pos) {
			if (pos.x > 0) {
				if (grid[(int)(pos.x-1), (int)(pos.y)].gameObject.name == "wall") {
					Debug.Log("Detected a collision in the movement script");
					//Probably call another function to handle what to do for collisions (TO DO)
				} else {
					pos += Vector3.left;
				}
			}
			//transform.Translate (new Vector3 (-moveSpeed, 0, 0));
		}
		
		if (Input.GetKey (KeyCode.RightArrow) && transform.position == pos) {
			if (pos.x < gridSizeX - 1) {
				if (grid[(int)(pos.x+1), (int)(pos.y)].gameObject.name == "wall") {
					Debug.Log("Detected a collision in the movement script");
					//Probably call another function to handle what to do for collisions (TO DO)
				} else {
					pos += Vector3.right;
				}
			}
			//transform.Translate (new Vector3 (moveSpeed, 0, 0));
		}
		transform.position = Vector3.MoveTowards (transform.position, pos, Time.deltaTime * speed); //Move to new location
		//Debug.Log ("X: " + pos.x.ToString () + ", Y: " + pos.y.ToString ()); */
	}

	//Returns the distance to the nearest wall to the right (in discrete game units)
	//public float distRight() {
	//
	//}

}
