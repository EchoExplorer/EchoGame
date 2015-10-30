using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class playerMovement : MonoBehaviour {
	
	//public float moveSpeed;
	
	public Vector3 pos;  //Determine location of position
	public float speed = 2.0f;  //Speed of the movement
	public Text msg;
	private int gridSizeX = 5;
	private int gridSizeY = 5;
	//private createGrid gameGrid;
	
	//public bool isUp = false;
	//public bool isDown = false;
	//public bool isLeft = false;
	//public bool isRight = false;
	
	// Use this for initialization
	void Start () {
		pos = transform.position;  //Set initial position
		Debug.Log ("Initial position: " + "X: " + pos.x.ToString() + ", Y: " + pos.y.ToString());
		
		//msg = gameObject.GetComponent<Text>();
		//gameGrid = new createGrid (gridSizeX, gridSizeY);

	}
	
	// Update is called once per frame
	void Update () {
		
		if (Input.GetKey(KeyCode.DownArrow) && transform.position == pos) {
			if (pos.y > 0) {
				//if (gameGrid.grid[(int)pos.x,(int)(pos.y-1)].gameObject.name == "wall") {
				//	Debug.Log("Detected a collision in the movement script");
				//}
				pos += Vector3.down;
			}
			//transform.Translate (new Vector3 (0, -moveSpeed, 0) * Time.deltaTime);
		}
		
		if (Input.GetKey (KeyCode.UpArrow) && transform.position == pos) {
			if (pos.y < gridSizeY - 1) {
				pos += Vector3.up;
			}
			//transform.Translate (new Vector3 (0, moveSpeed, 0));
		}
		
		if (Input.GetKey (KeyCode.LeftArrow) && transform.position == pos) {
			if (pos.x > 0) {
				pos += Vector3.left;
			}
			//transform.Translate (new Vector3 (-moveSpeed, 0, 0));
		}
		
		if (Input.GetKey (KeyCode.RightArrow) && transform.position == pos) {
			if (pos.x < gridSizeX - 1) {
				pos += Vector3.right;
			}
			//transform.Translate (new Vector3 (moveSpeed, 0, 0));
		}
		transform.position = Vector3.MoveTowards (transform.position, pos, Time.deltaTime * speed); //Move to new location
		Debug.Log ("X: " + pos.x.ToString () + ", Y: " + pos.y.ToString ());
		
	}
	
	void OnCollisionEnter2D(Collision2D coll) {
		Debug.Log ("Collision detected - outside");
		Debug.Log (coll.gameObject.name);

		if (coll.gameObject.name == "wall") {
			//Debug.Log ("Collision detected");
			//Destroy(coll.gameObject);
			//Set player (Cube) to initial position again
			pos.x = 0;
			pos.y = 0;
			
			//Get the player object
			GameObject player = GameObject.Find("Player");
			if (player != null) {
				//player.SetActive(false);
			}
		}
	}
}
