using UnityEngine;
using System.Collections;

/// <summary>
/// A skeletal implementation of moving objects, with smooth easing between positions.
/// </summary>
public abstract class MovingObject : MonoBehaviour
{
    // Seems to be copied from https://unity3d.com/learn/tutorials/projects/2d-roguelike-tutorial/moving-object-script
    public float moveTime = 0.4f;           //Time it will take object to move, in seconds.
    public LayerMask blockingLayer;         //Layer on which collision will be checked.


    private BoxCollider2D boxCollider;      //The BoxCollider2D component attached to this object.
    private Rigidbody2D rb2D;               //The Rigidbody2D component attached to this object.
    private float inverseMoveTime;          //Used to make movement more efficient.


    //Protected, virtual functions can be overridden by inheriting classes.
    protected virtual void Start()
    {
        //Get a component reference to this object's BoxCollider2D
        boxCollider = GetComponent<BoxCollider2D>();

        //Get a component reference to this object's Rigidbody2D
        rb2D = GetComponent<Rigidbody2D>();

        //By storing the reciprocal of the move time we can use it by multiplying instead of dividing, this is more efficient.
        inverseMoveTime = 1f / moveTime;
    }

    /// <summary>
    /// Move returns true if it is able to move and false if not. 
    ///  Move takes parameters for x direction, y direction and a RaycastHit2D to check collision.
    /// </summary>
    protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
    {
        float scale = (float)Utilities.SCALE_REF / (float)Utilities.MAZE_SIZE;

        //Store start position to move from, based on objects current transform position.
        Vector2 start = transform.position;

        // Calculate end position based on the direction parameters passed in when calling Move.
        Vector2 end = start + new Vector2(xDir * scale, yDir * scale);

        //Disable the boxCollider so that linecast doesn't hit this object's own collider.
        boxCollider.enabled = false;

        //Cast a line from start point to end point checking collision on blockingLayer.
        hit = Physics2D.Linecast(start, end, blockingLayer);

        //Re-enable boxCollider after linecast
        boxCollider.enabled = true;

        //Check if anything was hit
        if (hit.transform == null)
        {
            //If nothing was hit, start SmoothMovement co-routine passing in the Vector2 end as destination
            //StartCoroutine(SmoothMovement(end));

            // SmoothMovement() is DISABLED due to a FATAL flaw:
            // The position of the moving object is not thread safe. When one movement is still on processing, 
            // an immediate second command of movement will calculate its "end position" based on
            // the current temporary position, which results into an unexpected end position.
            rb2D.MovePosition(end);
            BoardManager.player_idx.x += xDir;
            BoardManager.player_idx.y += yDir;
            print("Player_idx.x: " + BoardManager.player_idx.x.ToString() + ", Player_idx.y: " + BoardManager.player_idx.y.ToString());

            //Return true to say that Move was successful
            return true;
        }
        //If something was hit, return false, Move was unsuccesful.
        print("Player_idx.x: " + BoardManager.player_idx.x.ToString() + ", Player_idx.y: " + BoardManager.player_idx.y.ToString());
        return false;
    }


    /// <summary>
    /// Co-routine for moving units from one space to next, takes a parameter end to specify where to move to.
    /// </summary>
    protected IEnumerator SmoothMovement(Vector3 end)
    {
        //Calculate the remaining distance to move based on the square magnitude of the difference between current position and end parameter. 
        //Square magnitude is used instead of magnitude because it's computationally cheaper.
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

        //While that distance is greater than a very small amount (Epsilon, almost zero):
        while (sqrRemainingDistance > float.Epsilon)
        {
            //Find a new position proportionally closer to the end, based on the moveTime
            Vector3 newPostion = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);

            //Call MovePosition on attached Rigidbody2D and move it to the calculated position.
            rb2D.MovePosition(newPostion);

            //Recalculate the remaining distance after moving.
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;

            //Return and loop until sqrRemainingDistance is close enough to zero to end the function
            yield return null;
        }        
    }


    /// <summary>
    /// AttemptMove takes a generic parameter T to specify the type of component we
    ///  expect our unit to interact with if blocked (Player for Enemies, Wall for Player).
    /// </summary>
    protected virtual bool AttemptMove<T>(int xDir, int yDir)
        where T : Component
    {
        //Hit will store whatever our linecast hits when Move is called.
        RaycastHit2D hit;

        //Set canMove to true if Move was successful, false if failed.
        bool canMove = Move(xDir, yDir, out hit);
        if (canMove)
        {
            OnMove();
        }

        //Check if nothing was hit by linecast
        if (hit.transform == null)
            //If nothing was hit, return and don't execute further code.
            return true;

        //Get a component reference to the component of type T attached to the object that was hit
        T hitComponent = hit.transform.GetComponent<T>();

        //If canMove is false and hitComponent is not equal to null, meaning MovingObject is blocked and has hit something it can interact with.
        if (!canMove && hitComponent != null)

            //Call the OnCantMove function and pass it hitComponent as a parameter.
            OnCantMove(hitComponent);
        return canMove;
    }

    /// <summary>
    /// Abstract method determining what the object does when it cannot move.
    /// </summary>
    protected abstract void OnCantMove<T>(T component)
        where T : Component;

    /// <summary>
    /// Abstract method determining what the object does when it moves.
    /// </summary>
	protected abstract void OnMove();

}