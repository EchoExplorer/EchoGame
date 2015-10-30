using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TouchTest : MonoBehaviour {
	private Touch initialTouch = new Touch();
	private bool hasSwiped = false;
	private float minSwipeDist = 100f;

	public Text gestureText;
	public GameObject gestureImage;
	
	void Start () {
		gestureImage = GameObject.Find ("GestureImage");
		gestureText = GameObject.Find ("GestureText").GetComponent<Text> ();
		gestureText.text = "?";
		gestureImage.SetActive (true);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
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
						gestureText.text =  "LEFT";
					} 
					else if (swipedSideways && deltaX <= 0) { //swiped right
						gestureText.text =  "RIGHT";
					} 
					else if (!swipedSideways && deltaY > 0) { //swiped down
						gestureText.text =  "DOWN";
					} 
					else if (!swipedSideways && deltaY <= 0) { //swipped up
						gestureText.text =  "UP";
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
	}
}
