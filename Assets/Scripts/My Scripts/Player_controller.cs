using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_controller : MonoBehaviour
{
    // Set Variablez
    public float Speed;
    public float jumpForce;

    // reference rigid body
    private Rigidbody myBody;


    public Transform groundCheck;
    // a bool for telling if the player is grounded
    public bool isGrounded;
    // if player can jump
    public bool canJump;

    // Start is called before the first frame update
    void Start()
    {
        //referenc rigidbody
        myBody = GetComponent<Rigidbody>();
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
        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical) * Speed * Time.deltaTime;
        transform.Translate(moveDirection);

        // creates a sphere and when that speere hits something it will return isGrounded=true
        isGrounded = Physics.CheckSphere(groundCheck.transform.position, 0.1f);

        //
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            canJump = !canJump;

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

}
