using UnityEngine;
using System.Collections;

/// <summary>
/// Class works through a managed polling of input, unity character controller,
/// and a mass for calculating force applied upon collision.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FirstPerson_Controller : MonoBehaviour
{
    #region Unity Public References
    /// <summary>
    /// The character controller that this script is attached to
    /// </summary>
    public CharacterController m_CharacterController;

    /// <summary>
    /// How deep into the controller, the camera sits.
    /// </summary>
    public int m_collider_layer = -1;

    /// <summary>
    /// How deep into the controller, the camera sits.
    /// </summary>
    public float m_fEyeDepthRatio = 0.15f;

    /// <summary>
    /// Script attached to child object with collider that occupies the crouch volume.
    /// </summary>
    public FirstPerson_CollisionRegister m_HeightProne;

    /// <summary>
    /// Script attached to child object with collider that occupies the crouch volume.
    /// </summary>
    public FirstPerson_CollisionRegister m_HeightCrouch;

    /// <summary>
    /// Script attached to child object with collider that occupies the standing volume.
    /// </summary>
    public FirstPerson_CollisionRegister m_HeightStand;

    /// <summary>
    /// Script attached to child object with collider that occupies the head volume.
    /// </summary>
    public FirstPerson_CollisionRegister m_HeightJump;

    /// <summary>
    /// Script attached to child object with collider that occupies the feet volume.
    /// </summary>
    public FirstPerson_CollisionRegister m_HeightGrounded;

    #endregion

    #region Player Physics Properties
    /// <summary>
    /// Character mass in kgs
    /// </summary>
    private float m_fMass = 90.0f;

    /// <summary>
    /// Returns the mass of this player
    /// </summary>
    public float Mass
    {
        get { return m_fMass; }
    }

    /// <summary>
    /// Maximumm weight a player can push, pull in any direction
    /// </summary>
    private float m_fHitWeight = 100.0f;

    /// <summary>
    /// Strength accessor for maximum physics interaction weights
    /// </summary>
    public float HitStrength
    {
        get { return m_fHitWeight; }
        set
        {
            if (value >= 0)
            {
                m_fHitWeight = value;
            }
        }
    }

    /// <summary>
    /// Maximumm weight a player can push, pull in any direction
    /// </summary>
    private float m_fPushWeight = 249.0f;

    /// <summary>
    /// Strength accessor for maximum physics interaction weights
    /// </summary>
    public float PushStrength
    {
        get { return m_fPushWeight; }
        set
        {
            if (value >= 0)
            {
                m_fPushWeight = value;
            }
        }
    }
    #endregion

    #region Camera
    /// <summary>
    /// Referent to the POV Camera for looking up and down
    /// </summary>
    public GameObject m_POVCamera;

    /// <summary>
    /// Referent to the POV Camera for looking up and down
    /// </summary>
    public float m_LookSensitivity;
    #endregion

    #region Grounded Properties
    /// <summary>
    /// Anchor for grounded on a rigidbody
    /// </summary>
    public Transform m_Anchor;

    /// <summary>
    /// Anchor for grounded on a rigidbody
    /// </summary>
    public float m_fAnchorDot;

    /// <summary>
    /// Anchor for grounded on a rigidbody
    /// </summary>
    private Vector3 m_v3AnchorOffset;

    /// <summary>
    /// Spherecast for edge case grounding 
    /// </summary>
    private Ray m_SphereRaycast = new Ray(Vector3.zero, -Vector3.up);

    /// <summary>
    /// Local raycast result
    /// </summary>
    private RaycastHit m_RaycastResult;

    #endregion

    #region InputBinds
    /// <summary>
    /// Axis to look left and right
    /// </summary>
    public string m_AxisLookX = "Mouse X";

    /// <summary>
    /// Axis to look up and down
    /// </summary>
    public string m_AxisLookY = "Mouse Y";

    /// <summary>
    /// Axis to move forwards and backwards - relative space
    /// </summary>
    public string m_AxisMoveZ = "Vertical";

    /// <summary>
    /// Axis to move left and right - relative space
    /// </summary>
    public string m_AxisMoveX = "Horizontal";
    
    /// <summary>
    /// Primary fire button
    /// </summary>
    public string m_BtnFire = "Fire";

    /// <summary>
    /// Alternate fire button
    /// </summary>
    public string m_BtnAltFire = "Alt Fire";

    /// <summary>
    /// Melee fire button
    /// </summary>
    public string m_BtnMelee = "Melee";

    /// <summary>
    /// Reload button
    /// </summary>
    public string m_BtnReload = "Reload";

    /// <summary>
    /// Use object
    /// </summary>
    public string m_BtnUse = "Use";

    /// <summary>
    /// Use object
    /// </summary>
    public string m_BtnManipulate = "Manipulate";

    /// <summary>
    /// Interface/Menu button
    /// </summary>
    public string m_BtnInterface = "Interface";

    /// <summary>
    /// Crouch button
    /// </summary>
    public string m_BtnCrouch = "Crouch";

    /// <summary>
    /// Prone button
    /// </summary>
    public string m_BtnProne = "Prone";

    /// <summary>
    /// Walk button
    /// </summary>
    public string m_BtnWalk = "Walk";

    /// <summary>
    /// Sprint button
    /// </summary>
    public string m_BtnSprint = "Sprint";

    /// <summary>
    /// Jump button
    /// </summary>
    public string m_BtnJump = "Jump";
    #endregion

    #region EnumerableStates
    /// <summary>
    /// State indicating height of the controller and view
    /// </summary>
    public enum Pose
    {
        Stand,
        Crouch,
        Prone
    }

    /// <summary>
    /// State indicating whether character is jumping
    /// </summary>
    public enum Jump
    {
        Grounded,
        Jumping,
        InAir
    }

    /// <summary>
    /// Current character volume
    /// </summary>
    private Pose m_ePose = Pose.Stand;

    /// <summary>
    /// Previous character volume 
    /// - Used to ensure if Crouch to prone then releasing crouch does not go to stand. 
    /// </summary>
    private Pose m_ePreviousPose = Pose.Stand;

    /// <summary>
    /// Flag for whether lerping between poses
    /// </summary>
    private bool m_bPoseLerping = false;

    /// <summary>
    /// How long the player has been lerping between heights
    /// </summary>
    private float m_fPoseTimer = 0.0f;

    /// <summary>
    /// Time taken to lerp between heights
    /// </summary>
    private float m_fPoseTime = 0.25f;

    /// <summary>
    /// Current jump state
    /// </summary>
    private Jump m_eJump = Jump.Grounded;

    public bool Grounded
    {
        get { return (m_eJump == Jump.Grounded); }
    }

    /// <summary>
    /// True if currently manipulating a physics object
    /// </summary>
    public bool m_bPhysManipulation = false;


    #endregion

    #region Speeds

    /* Speeds for all mobility states in m/s */
    private float m_fCurrentSpeed = 6.0f;
    /// <summary>
    /// Player Crawl Speed - m/s
    /// </summary>
    private float m_fProneSpeed = 1.0f;
    /// <summary>
    /// Player Crouch Speed - m/s
    /// </summary>
    private float m_fCrouchSpeed = 2.0f;


    /// <summary>
    /// Player Walk Speed - m/s
    /// </summary>
    private float m_fWalkSpeed = 2.5f;
    /// <summary>
    /// Player Run Speed - m/s
    /// </summary>
    private float m_fRunSpeed = 6.0f;
    /// <summary>
    /// Player Sprint Speed - m/s
    /// </summary>
    private float m_fSprintSpeed = 10.0f;

    /* Speed ratios for motion along local X and -Z axis */
    /// <summary>
    /// Local X Axis speed is this ratio of current state speed
    /// </summary>
    private float m_fStrafeRatio = 0.8f;
    /// <summary>
    /// Local Negative Z Axis speed is this ratio of current state speed
    /// </summary>
    private float m_fReverseRatio = 0.65f;
    /// <summary>
    /// In air speed limit on X/Z as a ratio of current state speeds
    /// </summary>
    private float m_fAirborneRatio = 0.75f;

    /* Jump values */
    /// <summary>
    /// Y speed impulse on jump - m/s
    /// </summary>
    private float m_fJumpSpeed = 4.25f;
    /// <summary>
    /// Ratio of jump impulse if crouching
    /// </summary>
    private float m_fCrouchJumpRatio = 0.75f;
    /// <summary>
    /// Ratio of jump impulse if prone
    /// </summary>
    private float m_fProneJumpRatio = 0.65f;

    #endregion

    #region Motion
    /* Player motion co-efficients */
    /// <summary>
    /// Player gravity taken from world setup
    /// </summary>
    private float m_fGravity = Physics.gravity.y;

    /// <summary>
    /// Public read-only player gravity
    /// </summary>
    public float Gravity
    {
        get { return m_fGravity; }
    }

    /// <summary>
    /// Current Y velocity of the player
    /// </summary>
    private float m_fYVelocity = 0.0f;
    
    /// <summary>
    /// Current input velocity of the player
    /// </summary>
    private Vector3 m_v3Motion = Vector3.zero;

    /// <summary>
    /// Read only motion accessor
    /// </summary>
    public Vector3 Motion
    {
        //get { return m_CharacterController.; }
        get { return m_v3Motion; }
    }

    /// <summary>
    /// Launch velocity for minimising air steering
    /// </summary>
    private Vector3 m_v3LaunchVelocity;

    /// <summary>
    /// Local temporary vector 3 for calculation
    /// </summary>
    private Vector3 t_Vector3;

    /// <summary>
    /// Temporary rigid body for current collidable
    /// </summary>
    private Rigidbody t_RigidBody;

    /// <summary>
    /// Temporary float storage of the dot product
    /// </summary>
    private float t_fDot;

    /// <summary>
    /// Const radians in a degree
    /// </summary>
    private const float RADIANS_TO_DEGREES = 0.0174533f;

    /// <summary>
    /// Const radians in a degree
    /// </summary>
    private const float ROTATION_PADDING = 1.0f;

    #endregion

    /// <summary>
    /// Awake to set up the layers procedurally
    /// </summary>
    public void Awake()
    {
        // Find the first 'Empty' layer counting down from unity's limit of 32.
        if (m_collider_layer < 0)
        {
            // Find and store first empty layer for our colliders
            for (m_collider_layer = 31; LayerMask.LayerToName(m_collider_layer) != ""; --m_collider_layer)
            {}
        }

        gameObject.layer = m_collider_layer;
        var colliders = GetComponentsInChildren<Collider>();
        for (int collider_index = 0; collider_index < colliders.Length; ++collider_index)
        {
            colliders[collider_index].gameObject.layer = m_collider_layer;
        }

        Physics.IgnoreLayerCollision(m_collider_layer, m_collider_layer, true);

        // Stops head jitters. :D but starts to clip :(
        //m_CharacterController.enableOverlapRecovery = false;
    }

    /// <summary>
    /// Called each frame to poll this Controllers inputs and move accordingly
    /// </summary>
    public void Update()
    {
        float delta_time = Time.deltaTime;
        // Maintain Cursor lock
        Cursor.lockState = CursorLockMode.Locked;
        // Update character pose
        PoseUpdate(delta_time);
        // Mouse look - if not manipulating physics
        if (!m_bPhysManipulation)
        {
            Look(delta_time);
        }
        // Update character grounded state
        GroundedCheck();
        // Move the character
        Move(delta_time);
    }

    /// <summary>
    /// Update enumerable states
    /// </summary>
    /// <param name="a_fDeltaTime"></param>
    private void PoseUpdate(float a_fDeltaTime)
    {
        // Always goto couch on crouch key
        //if (m_BtnCrouch.CurrentState == ButtonState.Down)
        if (Input.GetButtonDown(m_BtnCrouch))
        {
            StopAllCoroutines();
            StartCoroutine(ControllerToCrouching());
        }

        // Prone Toggle
        if (Input.GetButtonDown(m_BtnProne))
        {
            StopAllCoroutines();
            if (m_ePose == Pose.Prone)
            {
                StartCoroutine(ControllerToStanding());
            }
            else
            {
                StartCoroutine(ControllerToProne());
            }
        }

        // If crouch was released and we didt go to prone from crouch,
        //   then stand.
        //if (m_BtnCrouch.CurrentState == ButtonState.Released &&
        //    m_ePose != Pose.Prone)
        if (!Input.GetButton(m_BtnCrouch) &&
             m_ePose != Pose.Prone)
        {
            if (!(m_ePreviousPose == Pose.Prone && m_bPoseLerping))
            {
                StopAllCoroutines();
                StartCoroutine(ControllerToStanding()); /// THis here starts EVERY FRAME in 'default'
            }
        }

        // If we were crouching and crouch is released, stand
        //if ((m_BtnCrouch.CurrentState == ButtonState.Released || m_BtnCrouch.CurrentState == ButtonState.None) &&
        if (!Input.GetButton(m_BtnCrouch) &&
            m_ePose == Pose.Crouch && !m_bPoseLerping)
        {
            StopAllCoroutines();
            StartCoroutine(ControllerToStanding());
        }
        
        // Update speed - Based on pose, keypresses, and whether airborne
        if (m_ePose == Pose.Stand)
        {
            //if (m_BtnSprint.CurrentState != ButtonState.None)
            if (Input.GetButton(m_BtnSprint))
            {
                m_fCurrentSpeed = m_fSprintSpeed;
            }
            //else if (m_BtnWalk.CurrentState != ButtonState.None)
            else if (Input.GetButton(m_BtnWalk))
            {
                m_fCurrentSpeed = m_fWalkSpeed;
            }
            else
            {
                m_fCurrentSpeed = m_fRunSpeed;
            }
        }
        else if (m_ePose == Pose.Crouch && m_eJump == Jump.Grounded)
        {
            m_fCurrentSpeed = m_fCrouchSpeed;
        }
        else if (m_ePose == Pose.Prone)
        {
            m_fCurrentSpeed = m_fProneSpeed;
        }
    }

    /// <summary>
    /// Rotates camera and controller as as per axes
    /// </summary>
    private void Look(float a_fDeltaTime)
    {
        // Rotation around Y ( Up )
        // float yRot = m_AxisLookX.AxisValue * a_fDeltaTime * m_LookSensitivity;
        float yRot = Input.GetAxis(m_AxisLookX) * a_fDeltaTime * m_LookSensitivity;
        // Turn Left / Right
        transform.Rotate(Vector3.up, yRot, Space.Self);

        // Rotation around Local X ( Left/Right )
        // float xRot = m_AxisLookY.AxisValue * a_fDeltaTime * m_LookSensitivity;
        float xRot = Input.GetAxis(m_AxisLookY) * a_fDeltaTime * m_LookSensitivity;
        // Look Up / Down
        m_POVCamera.transform.Rotate(transform.right, -xRot, Space.World);

        // If camera is upside down
        if (Vector3.Dot(m_POVCamera.transform.up, Vector3.up) < 0.0f)
        {
            // Looking up - Rolling Backwards
            if (xRot > 0)
            {
                m_POVCamera.transform.localEulerAngles = Vector3.right * -90.0f;
            }
            // Looking down - Tumbling Forwards
            else if (xRot < 0)
            {
                m_POVCamera.transform.localEulerAngles = Vector3.right * 90.0f;
            }
        }
    }

    /// <summary>
    /// Check the motion and triggers to determine if grounded
    /// </summary>
    /// <returns>True if grounded</returns>
    private void GroundedCheck()
    {
        if (m_HeightGrounded.Colliding && m_eJump != Jump.Jumping)
        {
            // Sphere cast to check find ground to anchor to
            // Cast from half height so we dont stand on an object on our head
            
            //m_SphereRaycast.origin = transform.position + (m_CharacterController.height * 0.5f - m_CharacterController.radius) * Vector3.up;
            m_SphereRaycast.origin = transform.position + m_CharacterController.radius * Vector3.up;

            // Cast a sphere from within the controller to the base + 2x Skin width
            if (Physics.SphereCast(m_SphereRaycast, m_CharacterController.radius, out m_RaycastResult,
                //m_CharacterController.height * 0.5f - (m_CharacterController.radius * 2) + // The size of the sphere is added to each end of the 'ray'
                0.1f,//m_CharacterController.skinWidth * 2, // Doubling down on skin width in case of perfect collision / Unity.Grounded
                ~(1 << gameObject.layer)))
                //~(1 << LayerMask.NameToLayer("Player"))))
            {
                // If there was overlap, setup the anchor
                m_Anchor.gameObject.SetActive(true);
                m_Anchor.transform.position = m_RaycastResult.point;
                m_Anchor.parent = m_RaycastResult.collider.transform;

                // Record the current offset to the anchor
                m_v3AnchorOffset = transform.position - m_Anchor.transform.position;

                // Push the controller out of the object based on the displacement to the anchor 
                t_Vector3 = m_v3AnchorOffset + Vector3.up * (m_CharacterController.radius - m_CharacterController.skinWidth);
                
                float ratio = t_Vector3.magnitude / (m_CharacterController.radius);
                transform.position += (1 - ratio) * t_Vector3;
                //Debug.Log(ratio + " " + t_Vector3.ToString());

                // Record the final offset
                m_v3AnchorOffset = transform.position - m_Anchor.transform.position;
            }
            else
            {
                // Airborne?
                m_eJump = Jump.InAir;
            }

            // If the anchor is active, Adjust the controller and set falling to 0 if flat enough
            if (m_Anchor.gameObject.activeInHierarchy)
            {
                // Set pos by anchor
                //transform.position = m_Anchor.transform.position + m_v3AnchorOffset;
                //Debug.Log("here");
                m_eJump = Jump.Grounded;
                m_v3LaunchVelocity = Vector3.zero;

            }
        }
        // In the air but not jumping state
        // Walked off an edge or reached the peak of a jump.
        else if (m_eJump != Jump.Jumping)
        {
            m_eJump = Jump.InAir;
        }
    }   

    /// <summary>
    /// Moves the character controller in local space
    /// </summary>
    /// <param name="a_fDeltaTime"></param>
    private void Move(float a_fDeltaTime)
    {
        // Get axes w/ weights
        //m_v3Motion = new Vector3(m_AxisMoveX.AxisValue, 0, m_AxisMoveZ.AxisValue);
        m_v3Motion = new Vector3(Input.GetAxis(m_AxisMoveX), 0, Input.GetAxis(m_AxisMoveZ));
        // Ignore them if we are manipulating an object
        if (m_bPhysManipulation)
        {
            m_v3Motion = Vector3.zero;
        }
        
        // Normalise the vector if it is > 1
        if (m_v3Motion.sqrMagnitude > 1)
        {
            m_v3Motion.Normalize();
        }

        // Scale by speed - Component wise, post input scaling
        // Forwards
        m_v3Motion.z *= m_fCurrentSpeed;
        // Backwards
        if (m_v3Motion.z < 0)
        {
            m_v3Motion.z *= m_fReverseRatio;
        }
        // Strafe
        m_v3Motion.x *= m_fCurrentSpeed * m_fStrafeRatio;

        // Transform the vector into relative motion
        m_v3Motion = transform.TransformDirection(m_v3Motion);
        // Reduce input by ratio if airborne
        if (m_eJump != Jump.Grounded)
        {
            m_v3Motion *= m_fAirborneRatio;
        }

        // If jump pressed and jump hitbox is clear, jump.
        //if ((m_BtnJump.CurrentState == ButtonState.Down) && (m_eJump == Jump.Grounded))
        if (Input.GetButtonDown(m_BtnJump) && (m_eJump == Jump.Grounded))
        {
            // Head butt things
            if (m_HeightJump.Colliding)
            {
                m_HeightJump.ForceOutRigidbodies(ForceMode.Impulse);
            }
            else // Jump with conditions
            {
                // Set jump speed if on a slope less that slope limit
                t_Vector3 = m_Anchor.transform.position - (transform.position + Vector3.up * (m_CharacterController.radius - m_CharacterController.skinWidth));

                // If collision tangent is < slope, we can jump
                m_fAnchorDot = Vector3.Dot(t_Vector3.normalized, -Vector3.up);
                if (m_fAnchorDot > Mathf.Cos(m_CharacterController.slopeLimit * RADIANS_TO_DEGREES))
                {
                    // Add jump speed to Y
                    m_fYVelocity = m_fJumpSpeed;

                    // Update state
                    m_eJump = Jump.Jumping;
                    // Scale jump if crouching...
                    if (m_ePose == Pose.Crouch)
                    {
                        m_fYVelocity *= m_fCrouchJumpRatio;
                    }
                    // .. or prone.
                    if (m_ePose == Pose.Prone)
                    {
                        m_fYVelocity *= m_fProneJumpRatio;
                    }
                    // Store the X/Z of projectile motion
                    m_v3LaunchVelocity = m_CharacterController.velocity;
                    m_v3LaunchVelocity.y = 0.0f;
                }
            }
        }
        // else apply gravity
        else if (m_eJump != Jump.Grounded)
        {
            // Apply gravity
            m_fYVelocity = m_fYVelocity + m_fGravity * a_fDeltaTime;

            // Roof hit check
            if (m_fYVelocity > 0 && m_HeightJump.Colliding) // AND NOT GROUNDED LOL oops
            {
                m_fYVelocity = 0;
                m_eJump = Jump.InAir;
                m_HeightJump.ForceOutRigidbodies(ForceMode.Impulse);
            }
            // Check y speed and jump state to transition to in-air
            if (m_eJump == Jump.Jumping)
            {
                // Toggle from jump to falling
                if (m_fYVelocity <= 0)
                {
                    m_eJump = Jump.InAir;
                }
            }
        }
        else // Grounded
        {
            t_Vector3 = m_Anchor.transform.position - (transform.position + Vector3.up * (m_CharacterController.radius - m_CharacterController.skinWidth));
            // If collision tangent is < slope, we can 'stick' by wiping off velocity
            m_fAnchorDot = Vector3.Dot(t_Vector3.normalized, -Vector3.up);

            m_fAnchorDot = Mathf.Clamp(m_fAnchorDot, 0.0f, 1.0f);

            if (!(m_fAnchorDot > Mathf.Cos(m_CharacterController.slopeLimit * RADIANS_TO_DEGREES)))
            {
                // Becomes velocity down the slope
                m_fYVelocity = m_fYVelocity + m_fGravity * a_fDeltaTime * (1 - m_fAnchorDot);

                // Z/X Motion 
                //t_Vector3 = m_Anchor.transform.position - transform.position;
                t_Vector3.y = 0;
                t_Vector3.Normalize();
                t_Vector3 *= m_fYVelocity * (m_fAnchorDot);

                // Wipe out motion into the surface...
                m_v3Motion -=
                    (m_Anchor.transform.position - transform.position).normalized *
                    Vector3.Dot((m_Anchor.transform.position - transform.position).normalized, m_v3Motion.normalized) *
                    m_v3Motion.magnitude;
        
                // Apply Reacrtion force
                m_v3Motion += t_Vector3;
                //Debug.Log(t_Vector3.ToString());
            }
            else // we walk around on a shallow. no slope
            {
                // Direction to the plane highside
                t_Vector3 = m_Anchor.transform.position - transform.position;
                t_Vector3.y = 0;
                t_Vector3.Normalize();

                float fMotionDot = Vector3.Dot(m_v3Motion.normalized, t_Vector3);
                // Rotate by 90 around Y
                float t = t_Vector3.x;
                t_Vector3.x = -t_Vector3.z;
                t_Vector3.z = t;

                // Rotate motion around t_Vector3 by acos(m_fAnchorDot), slightly into the surface
                if (fMotionDot < 0)
                {
                    m_v3Motion = Quaternion.AngleAxis(Mathf.Acos(m_fAnchorDot) / RADIANS_TO_DEGREES + ROTATION_PADDING, t_Vector3) * m_v3Motion;
                }
                else
                {
                    m_v3Motion = Quaternion.AngleAxis(Mathf.Acos(m_fAnchorDot) / RADIANS_TO_DEGREES - ROTATION_PADDING, t_Vector3) * m_v3Motion;
                }

                m_fYVelocity = m_v3Motion.y;
            }
        }

        // Assign from velocity
        m_v3Motion.y = m_fYVelocity;

        // If theres any motion, disable the anchor till next frame.
        if (m_v3Motion.magnitude > float.Epsilon)
        {
            if (m_Anchor.gameObject.activeInHierarchy)
            {
                m_Anchor.parent = transform;
                m_Anchor.localPosition = Vector3.zero;
                m_Anchor.gameObject.SetActive(false);
            }
        }

        // Apply Motion
        if (m_CharacterController.enabled)
        {
            m_v3Motion *= a_fDeltaTime;
            m_CharacterController.Move(m_v3Motion);
        }
        //else // Fly mode
        //{
        //    transform.Translate(m_v3Motion * a_fDeltaTime);
        //}
    }

    /// <summary>
    /// How to react on controller collision with rigid body object
    /// </summary>
    /// <param name="a_collision"></param>
    private void OnControllerColliderHit(ControllerColliderHit a_collision)
    {
        // Get the contacted rigidbody
        t_RigidBody = null;
        t_RigidBody = a_collision.transform.GetComponent<Rigidbody>();

        // Apply force based on layer and reletive displacement
        if (t_RigidBody != null)
        {
            // Displacement calculation to collision point
            t_Vector3 = a_collision.point - transform.position;
            //t_Vector3 = (t_RigidBody.position + t_RigidBody.centerOfMass) - transform.position;
            t_Vector3.y = 0;
            t_Vector3.Normalize();              // RELATIVE GOD DAMN VELOCITY

            t_fDot = Vector3.Dot(t_Vector3, Motion);// a_collision.moveDirection.normalized);

            // Apply force to object if valid, as a function of motion, and strength
            if (!t_RigidBody.isKinematic)
            {
                if (t_fDot > 0 && t_RigidBody.mass <= PushStrength)
                {
                    t_RigidBody.AddForceAtPosition(
                            t_Vector3 * t_fDot * (PushStrength / Mathf.Max( 10.0f, t_RigidBody.mass)),
                            //t_Vector3 * t_fDot * Mathf.Clamp((PushStrength / t_RigidBody.mass), 0.0f , m_fCurrentSpeed * 0.5f),
                            //a_collision.normal * -t_fDot,
                            //a_collision.moveDirection,
                            //t_Vector3 * t_fDot,// * PushStrength,// * Motion.magnitude,
                            //a_collision.normal * t_fDot * PushStrength,
                            (t_RigidBody.position + t_RigidBody.centerOfMass),//a_collision.point,
                            //a_collision.point,//(t_RigidBody.position + t_RigidBody.centerOfMass),
                            ForceMode.VelocityChange);
                }
            }
        }
    }


    #region Character Controller Height Lerping
    /// <summary>
    /// Co-routine to lerp the controller to standing size
    /// </summary>
    private IEnumerator ControllerToStanding()
    {
        // Get the target height from the heightStand trigger
        float fFinishHeight = m_HeightStand.GetComponent<CapsuleCollider>().height;
        // Wait untill clear to stand
        bool bCantStand = true;
        while (bCantStand)
        {
            // If grounded, check the heightStand trigger
            if (m_eJump == Jump.Grounded)
            {
                bCantStand = m_HeightStand.Colliding;
                if (m_HeightStand.OnlyDynamicCollisions)
                {
                    m_HeightStand.ForceOutRigidbodies();
                }
            }
            // Not grounded, then there is space
            else
            {
                bCantStand = false;
            }
            // Keep waiting.
            yield return new WaitForEndOfFrame();
        }

        // Store the previous pose
        m_ePreviousPose = m_ePose;
        // Set the current pose
        m_ePose = Pose.Stand;
        // Initialise timer
        m_fPoseTimer = 0.0f;
        // Set the lerping flag
        m_bPoseLerping = true;
        // Current height is stored as the start height as to lerp from any beginning
        float fStartHeight = m_CharacterController.height;
        // Continue to lerp towards target height while unobstructed
        while (m_fPoseTimer < m_fPoseTime)
        {
            // If new collisions detected, wait.
            if (m_HeightStand.Colliding)
            {
                yield return new WaitForEndOfFrame();
                if (m_HeightStand.OnlyDynamicCollisions)
                {
                    m_HeightStand.ForceOutRigidbodies();
                }
            }
            else
            {
                // ... else Lerp
                m_fPoseTimer += Time.deltaTime;
                m_CharacterController.height = Mathf.Lerp(fStartHeight, fFinishHeight, m_fPoseTimer / m_fPoseTime);
                m_CharacterController.center = Vector3.up * m_CharacterController.height * 0.5f;
                m_POVCamera.transform.parent.localPosition = Vector3.up * (1 - m_fEyeDepthRatio) * m_CharacterController.height;
                yield return new WaitForEndOfFrame();
            }
        }

        // Once finished, snap to height, centre, and camera, to remove any inaccuracy
        m_CharacterController.height = fFinishHeight;
        m_CharacterController.center = Vector3.up * m_CharacterController.height * 0.5f;
        m_POVCamera.transform.parent.localPosition = Vector3.up * (1 - m_fEyeDepthRatio) * m_CharacterController.height;
        m_bPoseLerping = false;
    }

    /// <summary>
    /// Co-routine to lerp the controller to crouch size
    /// </summary>
    private IEnumerator ControllerToCrouching()
    {
        // Get the target height from the heightCrouch trigger
        float fFinishHeight = m_HeightCrouch.GetComponent<CapsuleCollider>().height;

        // Wait until it is clear.
        //   This should only be colliding if character is prone and under an object
        while (m_HeightCrouch.Colliding)
        {
            if (m_HeightCrouch.OnlyDynamicCollisions)
            {
                m_HeightCrouch.ForceOutRigidbodies();
            }
            yield return new WaitForEndOfFrame();
        }

        // Store the previous pose
        m_ePreviousPose = m_ePose;
        // Set the current pose
        m_ePose = Pose.Crouch;
        // Initialise timer
        m_fPoseTimer = 0.0f;
        // Set the lerping flag
        m_bPoseLerping = true;
        // Current height is stored as the start height as to lerp from any beginning
        float fStartHeight = m_CharacterController.height;

        // Continue to lerp towards target height while unobstructed
        while (m_fPoseTimer < m_fPoseTime)
        {
            // If new collisions detected, wait.
            if (m_HeightCrouch.Colliding)
            {
                if (m_HeightCrouch.OnlyDynamicCollisions)
                {
                    m_HeightCrouch.ForceOutRigidbodies();
                }
                yield return new WaitForEndOfFrame();
            }
            else
            {
                // We track the previous and new lerps in order to calculate the lifting of legs in a crouch jump
                float previousLerp = Mathf.Lerp(fStartHeight, fFinishHeight, m_fPoseTimer / m_fPoseTime);
                // Update pose timer
                m_fPoseTimer += Time.deltaTime;
                float newLerp = Mathf.Lerp(fStartHeight, fFinishHeight, m_fPoseTimer / m_fPoseTime);

                // Update height and centre
                m_CharacterController.height = newLerp;
                m_CharacterController.center = Vector3.up * m_CharacterController.height * 0.5f;
                m_POVCamera.transform.parent.localPosition = Vector3.up * (1 - m_fEyeDepthRatio) * m_CharacterController.height;

                // If crouching in the air, Lift the legs by moving the whole collider up.
                if (m_eJump != Jump.Grounded)
                {
                    m_CharacterController.Move(Vector3.up * (previousLerp - newLerp));
                }    
                
                yield return new WaitForEndOfFrame();
            }
        }

        // Once finished, snap to height, centre, and camera, to remove any inaccuracy
        m_CharacterController.height = fFinishHeight;
        m_CharacterController.center = Vector3.up * m_CharacterController.height * 0.5f;
        m_POVCamera.transform.parent.localPosition = Vector3.up * (1 - m_fEyeDepthRatio) * m_CharacterController.height;
        m_fPoseTimer = 0.0f;
        m_bPoseLerping = false;
    }

    /// <summary>
    /// Coroutine for changing the shape of the collider to the prone size.
    /// </summary>
    private IEnumerator ControllerToProne()
    {
        // Start prone lerp immediately, prone collider should never be occupied
        m_fPoseTimer = 0.0f;
        m_bPoseLerping = true;
        m_ePose = Pose.Prone;
        // Current height is stored as the start height as to lerp from any beginning
        float fStartHeight = m_CharacterController.height;
        // Get the target height from the heightStand trigger
        float fFinishHeight = m_HeightProne.GetComponent<CapsuleCollider>().height;

        // Lerp as a function of time and framerate
        while (m_fPoseTimer < m_fPoseTime)
        {
            m_fPoseTimer += Time.deltaTime;
            m_CharacterController.height = Mathf.Lerp(fStartHeight, fFinishHeight, m_fPoseTimer / m_fPoseTime);
            m_CharacterController.center = Vector3.up * m_CharacterController.height * 0.5f;
            m_POVCamera.transform.parent.localPosition = Vector3.up * (1 - m_fEyeDepthRatio) * m_CharacterController.height;
            yield return new WaitForEndOfFrame();
        }

        // Once finished, snap to height, centre, and camera, to remove any inaccuracy
        m_CharacterController.height = fFinishHeight;
        m_CharacterController.center = Vector3.up * m_CharacterController.height * 0.5f;
        m_POVCamera.transform.parent.localPosition = Vector3.up * (1 - m_fEyeDepthRatio) * m_CharacterController.height;
        m_fPoseTimer = 0.0f;
        m_bPoseLerping = false;
    }
    #endregion
}

