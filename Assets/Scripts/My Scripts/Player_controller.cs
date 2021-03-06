using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_controller : MonoBehaviour
{
    // Set Variablez
    private float speed;
    public float walkSpeed;
    public float runSpeed;
    public float crouchSpeed;

    public float jumpForce;

    // reference rigid body
    private Rigidbody myBody;


    public Transform groundCheck;
    public Transform roofCheck;

    CapsuleCollider capCollider;

    // a bool to check if the player is grounded
    public bool isGrounded;
    // a bool to check if the player hits the roof
    public bool isTouchRoof;
    // if player can jump
    public bool canJump;
    // if the player can fly
    public bool canFly;
    // when player is crouching
    public bool isCrouch;

    //inventory bools
    public bool hasKey;
    public bool hasRope;

    // Start is called before the first frame update
    void Start()
    {
        //reference rigidbody
        myBody = GetComponent<Rigidbody>();
        // reference to capsule collider
        capCollider = GetComponent<CapsuleCollider>();
        // stops curser from being seen during gameplay.
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        // movement wasd keys imputs
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // player movement
        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical) * speed * Time.deltaTime;
        transform.Translate(moveDirection);

        // creates a sphere and when that speere hits something it will return isGrounded=true
        isGrounded = Physics.CheckSphere(groundCheck.transform.position, 0.1f);

        isTouchRoof = Physics.CheckSphere(roofCheck.transform.position, 0.1f);

        //
        if (canFly)
        {
            if (isTouchRoof)
                {
                    transform.position -= transform.up * 1f;
                }
        }
       
        
        //
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            canJump = !canJump;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            //DoCrouch();
        }

        if (Input.GetKeyDown(KeyCode.R) && !isCrouch)
        {
            speed = runSpeed;
        }

        if (Input.GetKey(KeyCode.W))
        {
            speed = walkSpeed;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Fly();
        }

        if (Input.GetKey(KeyCode.Z) && canFly)
        {
            transform.position += transform.up * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.X) && canFly)
        {
            transform.position -= transform.up * speed * Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if(canJump)
        {
            myBody.AddForce(Vector3.up * jumpForce);
            canJump = !canJump;
        }
    }

    public void DoCrouch()
    {
        if (isCrouch)
        {
            capCollider.height += 1f;
        }
        else
        {
            capCollider.height -= 1f;
        }
        isCrouch = !isCrouch;
    }

    public void Fly()
    {
        canFly = !canFly;
        myBody.isKinematic = canFly;
    }
}
