using UnityEngine;
using System.Collections;

public class createGrid : MonoBehaviour {

	public GameObject[,] grid;
	private int gridSizeX = 5;
	private int gridSizeY = 5;
	//private int gridSizeX;
	//private int gridSizeY;
	private Material pathMaterial;
	private Material wallMaterial;
	private GameObject player;

	//public createGrid(int sizeX, int sizeY) {
	//	gridSizeX = sizeX;
	//	gridSizeY = sizeY;
	//}


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
	
	}
}
