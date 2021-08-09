using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickScripts
{
    public class QS_DemoPlayer : MonoBehaviour
    {
        public bool startAtSpawn;
        public Transform spawnPoint;
        CharacterController cc;
        Rigidbody rb;
        float speed = 5;
        bool isJumping;
        bool groundedPlayer;
        private Vector3 playerVelocity;
        private float jumpHeight = 1.0f;
        private float gravityValue = -9.81f;

        private void Start()
        {
            cc = GetComponent<CharacterController>();
            rb = GetComponent<Rigidbody>();

            if (startAtSpawn && spawnPoint != null)
                this.transform.position = spawnPoint.position;
        }

        private void Update()
        {
          //  playerCameraPivot.position = this.transform.position;

             groundedPlayer = cc.isGrounded;
             if (groundedPlayer && playerVelocity.y < 0)
             {
                 playerVelocity.y = 0f;
             }

            


            Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            cc.Move(move * Time.deltaTime * speed);

            if (move != Vector3.zero)
            {
                gameObject.transform.forward = move;
            }

            // Changes the height position of the player..
            if (Input.GetButton("Jump") && groundedPlayer)
            {
                playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            }

            playerVelocity.y += gravityValue * Time.deltaTime;
            cc.Move(playerVelocity * Time.deltaTime);



            //cc.SimpleMove(Vector3.forward * Input.GetAxis("Vertical") * speed + 
            //    Vector3.right * Input.GetAxis("Horizontal") * speed);
            //
            //if (cc.velocity.y == 0)
            //    isGrounded = true;
            //
            //if (isGrounded && Input.GetKeyDown(KeyCode.Space))
            //{
            //    Debug.Log("Jump!");
            //    rb.AddForce(Vector3.up * 1000, ForceMode.Impulse);
            //}

        }

        // this script pushes all rigidbodies that the character touches
        float pushPower = 2.0f;

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;

            // no rigidbody
            if (body == null || body.isKinematic)
            {
                return;
            }

            // We dont want to push objects below us
            if (hit.moveDirection.y < -0.3)
            {
                return;
            }

            // Calculate push direction from move direction,
            // we only push objects to the sides never up and down
            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

            // If you know how fast your character is trying to move,
            // then you can also multiply the push velocity by that.

            // Apply the push
            body.velocity = pushDir * pushPower;
        }
    }

}
