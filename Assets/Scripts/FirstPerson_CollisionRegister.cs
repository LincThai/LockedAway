using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FirstPerson_CollisionRegister : MonoBehaviour
{
    /// <summary>
    /// List for responsive ridgid bodies
    /// </summary>
    private List<Rigidbody> m_RigidBodyList;

    /// <summary>
    /// Temporary rigid body for current collidable
    /// </summary>
    private Rigidbody t_RigidBody;

    /// <summary>
    /// Counter for number of colliders currently in this trigger
    /// </summary>
    private int m_iCount = 0;

    /// <summary>
    /// Returns true if colliding with anything
    /// </summary>
    public bool Colliding
    {
        get { return m_iCount > 0 ? true : false; }
    }

    /// <summary>
    /// Number of colliders in the trigger
    /// </summary>
    public int ColliderCount
    {
        get { return m_iCount;  }
    }

    /// <summary>
    /// Returns the first Rigidbody in the list if present
    /// </summary>
    public Rigidbody FirstRigidbody
    {
        get 
        { 
            return ( m_RigidBodyList.Count == 0 ? null : m_RigidBodyList[0]);
        }
    }

    /// <summary>
    /// Returns true if ONLY colliding with rigidbodies
    /// </summary>
    public bool OnlyDynamicCollisions
    {
        get { return m_iCount == m_RigidBodyList.Count && Colliding ? true : false; }
    }

    /// <summary>
    /// Impulse for clearling this register
    /// </summary>
    private float m_fImpulse = 50.0f;

    /// <summary>
    /// Impulse get / set
    /// </summary>
    public float Impulse
    {
        get { return m_fImpulse; }
        set
        {
            if (value >= 0)
            {
                m_fImpulse = value;
            }
        }
    }

    /// <summary>
    /// Force amount for gradual clearing of this collision register
    /// </summary>
    private float m_fForce = 150.0f;

    /// <summary>
    /// Accessor for force get / set
    /// </summary>
    public float Force
    {
        get { return m_fForce; }
        set
        {
            if (value >= 0)
            {
                m_fForce = value;
            }
        }
    }

    /// <summary>
    /// Get the layers in question
    /// </summary>
    private void Awake()
    {
        m_RigidBodyList = new List<Rigidbody>();
    }

    /// <summary>
    /// Apply force to placeables overlapping this trigger as per player strength
    /// </summary>
    public void ForceOutRigidbodies(ForceMode a_ForceMode = ForceMode.Force)
    {
        for (int i = 0; i < m_RigidBodyList.Count; ++i)
        {
            m_RigidBodyList[i].AddForceAtPosition
                ((m_RigidBodyList[i].position - transform.position).normalized * (a_ForceMode == ForceMode.Impulse ? Impulse : Force),
                transform.position,
                a_ForceMode);
        }
    }

    /// <summary>
    /// Track any colliders that intersect this trigger
    /// </summary>
    /// <param name="other">Collider that entered the trigger</param>
	public void OnTriggerEnter(Collider other)
    {
        // Increment the count
        ++m_iCount;

        // Get the contacted rigidbody, if any
        t_RigidBody = null;
        t_RigidBody = other.transform.GetComponent<Rigidbody>();
        
        // Apply force based on layer and reletive displacement
        if (t_RigidBody != null)
        {
            if (!t_RigidBody.isKinematic)
            {
                m_RigidBodyList.Add(t_RigidBody);
            }
        }
    }

    /// <summary>
    /// Deregister the collider
    /// </summary>
    /// <param name="other">Collider that left the trigger</param>
    public void OnTriggerExit(Collider other)
    {
        // Decrement counter
        --m_iCount;
        // Get the contacted rigidbody
        t_RigidBody = null;
        t_RigidBody = other.transform.GetComponent<Rigidbody>();

        // Apply force based on layer and reletive displacement
        if (t_RigidBody != null)
        {
            if (!t_RigidBody.isKinematic)
            {
                m_RigidBodyList.Remove(t_RigidBody);
            }
        }
    }
}
