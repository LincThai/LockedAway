using System;
using UnityEngine;

/// <summary>
/// Controlls camera raycast projections to 
///     manipulate rigidbodies from an FPS perspective
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class Rigidbody_Manipulator : MonoBehaviour
{
    /// <summary>
    /// Reference to the player - if used with first person controller
    /// </summary>
    public FirstPerson_Controller m_Player;

    /// <summary>
    /// Grounded trigger reference to make sure we are not standing
    /// on the object we are trying to grab, where applicable
    /// </summary>
    public FirstPerson_CollisionRegister m_GroundedRegister;

    /// <summary>
    /// The main POV Camera
    /// </summary>
    public Camera m_RayCastCamera;
    
    /// <summary>
    /// Emission Colour of selected object
    /// </summary>
    public Color m_cSelected;

    /// <summary>
    /// True if currently carrying a physics object
    /// </summary>
    private bool m_bPhysCarry = false;

    /// <summary>
    /// Maximumm weight a player can push, pull in any direction
    /// </summary>
    private float m_fCarryWeight = 150.0f;

    /// <summary>
    /// Move held object on X axis
    /// </summary>
    public string m_AxisMoveX = "Mouse X";

    /// <summary>
    /// Move held object on Y axis
    /// </summary>
    public string m_AxisMoveY = "Mouse Y";

    /// <summary>
    /// Move held object on Z axis
    /// </summary>
    public string m_AxisMoveZ = "Mouse ScrollWheel";

    /// <summary>
    /// Rotate held object on X axis
    /// </summary>
    public string m_AxisRotateX = "Vertical";

    /// <summary>
    /// Rotate held object on Y axis
    /// </summary>
    public string m_AxisRotateY = "Horizontal";

    /// <summary>
    /// Rotate held object on Z axis
    /// </summary>
    public string m_AxisRotateZ = "Roll";

    /// <summary>
    /// Alternate fire button
    /// </summary>
    public string m_BtnPhysUse = "Alt Fire";

    /// <summary>
    /// Alternate fire button
    /// </summary>
    public string m_BtnDrop = "Fire";

    /// <summary>
    /// Current GameObject being manipulated
    /// </summary>
    private GameObject m_PhysicsObject;

    /// <summary>
    /// Read only access to held object
    /// </summary>
    public GameObject HeldObject
    {
        get { return m_PhysicsObject; }
    }

    /// <summary>
    /// Current GameObject being manipulated
    /// </summary>
    private Rigidbody m_Rigidbody;

    /// <summary>
    /// All renderer on the selected object
    /// </summary>
    private Renderer m_Renderer;

    /// <summary>
    /// All renderers on the selected object
    /// </summary>
    private Renderer[] m_Renderers;

    /// <summary>
    /// Drag of the object before applying manipulation dampening
    /// </summary>
    private float m_fDrag;

    /// <summary>
    /// Weight of the object before applying manipulation
    /// </summary>
    private float m_fOriginalWeight;

    /// <summary>
    /// Emission Colour of selected object before selection
    /// </summary>
    private Color m_cEmission;

    /// <summary>
    /// Emission Colour of selected object before selection
    /// </summary>
    private Color[] m_cEmissions;

    /// <summary>
    /// The displacement offset of the object
    /// </summary>
    private Vector3 m_v3Offset;

    /// <summary>
    /// The rotation offset of the object
    /// </summary>
    private Quaternion m_qRotation;

    /// <summary>
    /// Range of the raycast from the camera
    /// </summary>
    private float m_fRange = 3.0f;

    /// <summary>
    /// Range of the raycast from the camera
    /// </summary>
    private float m_fMaxRange = 5.0f;

    /// <summary>
    /// Drag during manipulation
    /// </summary>
    //private float m_fManipulationDrag = 5.0f;
    private float m_fManipulationDrag = 10.0f;

    /// <summary>
    /// Local ray storage
    /// </summary>
    Ray m_Ray;

    /// <summary>
    /// Local hit result storage
    /// </summary>
    RaycastHit m_RaycastHit;

    /// <summary>
    /// Collision mask to ignore the player's triggers
    /// </summary>
    int iMask = 1;

    /// <summary>
    /// On instantiation, ignore the layer that is this player
    /// </summary>
    public void Start()
    {
        m_Player = GetComponent<FirstPerson_Controller>();

        iMask = 1 << gameObject.layer;
        iMask = ~iMask;
    }

    /// <summary>
    /// Atempt to find and engage a rigidbody
    /// </summary>
    public bool Engage(float a_fWeight)
    {
        // Cast ray from centre of screen
        m_Ray = m_RayCastCamera.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        if (Physics.Raycast(m_Ray, out m_RaycastHit, m_fRange, iMask))
        {
            m_Rigidbody = m_RaycastHit.collider.GetComponent<Rigidbody>();
            // If we hit something with a rigid body
            if (m_Rigidbody != null)
            {
                // Can we pick it up?
                if (a_fWeight < m_Rigidbody.mass || m_Rigidbody.isKinematic)
                {
                    return false;
                }

                // Then this is our engaged game object
                m_PhysicsObject = m_RaycastHit.collider.gameObject;
                // Store a positional offset accounting for the players rotation
                m_v3Offset = Quaternion.Inverse(m_RayCastCamera.transform.rotation) * (m_Rigidbody.position - m_RayCastCamera.transform.position);
                // Toggle off Gravity
                m_Rigidbody.useGravity = false;
                // Store...
                m_fDrag = m_Rigidbody.drag;
                m_fOriginalWeight = m_Rigidbody.mass;

                // and assign drag.
                m_Rigidbody.drag = m_fManipulationDrag;
                m_Rigidbody.mass = 1.0f;
                // Continuous Dynamic detection
                m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                // Get the renderers and assign an emission colour
                m_Renderer = m_PhysicsObject.GetComponent<Renderer>();
                if (m_Renderer != null)
                {
                    m_cEmission = m_Renderer.material.GetColor("_EmissionColor");
                    m_Renderer.material.SetColor("_EmissionColor", m_cSelected);
                }
                else
                {
                    m_Renderers = m_PhysicsObject.GetComponentsInChildren<Renderer>();
                    if (m_Renderers != null)
                    {
                        m_cEmissions = new Color[m_Renderers.Length];
                        for (int i = 0; i < m_Renderers.Length; ++i)
                        {
                            m_cEmissions[i] = m_Renderers[i].material.GetColor("_EmissionColor");
                            m_Renderers[i].material.SetColor("_EmissionColor", m_cSelected);
                        }
                    }
                }
                return true;
            }
        }

        // We didn't find a target, or it was invalid
        m_PhysicsObject = null;
        return false;
    }


    private void Update()
    {
        // Select active
        //if (m_BtnAltFire.CurrentState == ButtonState.Down && !m_bPhysCarry)
        if (Input.GetButtonDown(m_BtnPhysUse) && !m_bPhysCarry)
        {
            m_bPhysCarry = Engage(m_fCarryWeight);
        }

        // Drop / Disengage
        if (Input.GetButtonDown(m_BtnDrop))
        {
            m_bPhysCarry = false;
        }

        // On hold, move object
        if (m_bPhysCarry)
        {
            m_bPhysCarry = Iterate();
            
            // btn held
            if ( Input.GetButton(m_BtnPhysUse) && !Input.GetButtonDown(m_BtnPhysUse))
            {

                if (m_Player)
                {
                    m_Player.m_bPhysManipulation = Manipulate();
                }
                else
                {
                    Manipulate();
                }
            }
            else
            {
                if (m_Player)
                {
                    m_Player.m_bPhysManipulation = false;
                }
            }
        }
        else
        { 
            Disengage();
            m_bPhysCarry = false;
            if (m_Player)
            {
                m_Player.m_bPhysManipulation = false;
            }
        }
    }

    /// <summary>
    /// Per frame updating of position and interaction
    /// </summary>
    public bool Iterate()
    {
        // If theres no object - Done
        if (m_PhysicsObject == null)
        {
            return false;
        }

        // Calculate displacement vector
        Vector3 displacement = (m_RayCastCamera.transform.position +
                                m_RayCastCamera.transform.rotation * m_v3Offset) -
                                m_Rigidbody.position;

        // Add force towards the offset point
        float fSqrMag = displacement.sqrMagnitude;
        // Change this to store the activation weight during engage, and so if it gets heavier, drop, and use a force proportional to mass F/M = const A tbd
        //m_Rigidbody.AddForce(displacement.normalized * fSqrMag * 20.0f, ForceMode.Force);
        //m_Rigidbody.AddForce(displacement.normalized * fSqrMag, ForceMode.VelocityChange);
        float AccScalar = 5.0f;
        //m_Rigidbody.AddForce(displacement.normalized * m_Rigidbody.mass, ForceMode.Acceleration);
        m_Rigidbody.AddForce(displacement.normalized * fSqrMag * 0.5f * m_fCarryWeight *  m_Rigidbody.mass, ForceMode.Force);

        // Drop if out of range
        if ((m_Rigidbody.position - transform.position).magnitude > m_fMaxRange)
        {
            return false;
        }

        // If being stood on, drop
        if (m_GroundedRegister)
        {
            if (m_GroundedRegister.OnlyDynamicCollisions
                && m_GroundedRegister.ColliderCount == 1
                && m_GroundedRegister.FirstRigidbody == m_Rigidbody)
            {
                return false;
            }
        }

        // Othewise, continue carrying
        return true;
    }
    
    /// <summary>
    /// Rotate the object based on mouse motion
    /// </summary>
    public bool Manipulate()                           
    {
        if (m_Rigidbody == null)
        {
            return false;
        }
        
        // Apply torque to object based on mouse motion
        Vector3 t_Vector3 = new Vector3(
            Input.GetAxis(m_AxisRotateX) * Time.deltaTime,
            -Input.GetAxis(m_AxisRotateY) * Time.deltaTime,
            Input.GetAxis(m_AxisRotateZ) * Time.deltaTime);
        t_Vector3 = transform.rotation * t_Vector3;
        m_Rigidbody.AddTorque(t_Vector3, ForceMode.VelocityChange);

        // Add to offset based on WASD axes
        m_v3Offset += Input.GetAxis(m_AxisMoveX) * Vector3.right * Time.deltaTime;
        m_v3Offset += Input.GetAxis(m_AxisMoveY) * Vector3.up * Time.deltaTime;
        m_v3Offset += Input.GetAxis(m_AxisMoveZ) * Vector3.forward;
        return true;
    }

    /// <summary>
    /// Reset the render and physics properties once we are done
    ///   manipulating the object
    /// </summary>
    public void Disengage()
    {
        if (m_PhysicsObject != null)
        {
            m_Rigidbody.useGravity = true;
            m_Rigidbody.drag = m_fDrag;
            m_Rigidbody.mass = m_fOriginalWeight;

            // Continuous Dynamic detection
            m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;

            if (m_Renderer != null)
            {
                m_Renderer.material.SetColor("_EmissionColor", m_cEmission);
            }
            else if (m_Renderers != null)
            {
                for (int i = 0; i < m_Renderers.Length; ++i)
                {
                    m_Renderers[i].material.SetColor("_EmissionColor", m_cEmissions[i]);
                }
            }
            m_PhysicsObject = null;
        }
    }
}
