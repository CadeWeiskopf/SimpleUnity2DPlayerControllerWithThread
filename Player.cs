using System.Collections;
using System.Threading;
using UnityEngine;

public class Player : MonoBehaviour
{
    private float MovementSpeed = 3f;

    private Vector2 JumpForce = new Vector2(0f, 5f);

    private Thread JumpThread;

    private int JumpCount;

    private bool Jumping;
    private bool Grounded;
    private bool StopThread;
    private bool JumpButtonUp;
    
    private Coroutine CoroutineGetMovement;
    private Coroutine CoroutineGetJump;
    private Coroutine CoroutineGetGrounded;

    private Rigidbody2D rb;



    private void Awake()
    {
        rb = transform.GetComponent<Rigidbody2D>();

        JumpThread = new Thread(new ThreadStart(this.JumpThreadTask));
        JumpThread.IsBackground = true;
        JumpThread.Start();
    }

    private void Update()
    {
        if (Input.GetButtonUp("Jump"))
        {
            JumpButtonUp = true;
        }
    }

    private void FixedUpdate()
    {
        if (CoroutineGetMovement == null)
        {
            CoroutineGetMovement = StartCoroutine(GetMovement());
        }
    }

    private IEnumerator GetMovement()
    {
        if (Input.GetButton("MoveRight"))
        {
            transform.position = Vector2.Lerp(transform.position, new Vector2(transform.position.x + MovementSpeed, transform.position.y), Time.fixedDeltaTime); 
        }
        else if (Input.GetButton("MoveLeft"))
        {
            transform.position = Vector2.Lerp(transform.position, new Vector2(transform.position.x - MovementSpeed, transform.position.y), Time.fixedDeltaTime); 
        }

        if ((Grounded || JumpCount < 2) && !Jumping  && Input.GetButton("Jump"))
        {
            Jumping = true;
            JumpCount++;
            rb.AddForce(JumpForce, ForceMode2D.Impulse);
        }

        yield return new WaitForEndOfFrame();

        CoroutineGetMovement = null;
    }

    private IEnumerator GetGrounded()
    {
        yield return new WaitForEndOfFrame();
        
        if (CoroutineGetGrounded != null)
        {
            Grounded = true;
            Jumping = false;
            JumpCount = 0;
            JumpButtonUp = false;
            CoroutineGetGrounded = null;
        }
        else
        {
            Debug.LogError("!");
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (CoroutineGetGrounded == null && col.collider.tag.Equals("Ground"))
        {
            CoroutineGetGrounded = StartCoroutine(GetGrounded());
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.collider.tag.Equals("Ground"))
        {
            Grounded = false;
            CoroutineGetGrounded = null;
        }
    }

    private void JumpThreadTask()
    {
        while (!StopThread)
        {
            if (!Grounded && JumpCount < 2 && JumpButtonUp)
            {
                Jumping = false;
                JumpButtonUp = false;
            }
        }
    }

    private void OnApplicationQuit()
    {
        StopThread = true;
    }
}
