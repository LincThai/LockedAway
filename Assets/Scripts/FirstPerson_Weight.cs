using UnityEngine;
using System.Collections;

public class FirstPerson_Weight : MonoBehaviour
{
    /// <summary>
    /// Reference to the Player component to get the motion and mass for momentum calculations
    /// </summary>
    public FirstPerson_Controller m_Player;

    /// <summary>
    /// 
    /// </summary>
    public Collider m_GroundCollider;


    /// <summary>
    /// Temporary rigid body for current collidable
    /// </summary>
    private Rigidbody t_RigidBody;


    private void Awake()
    {
        m_GroundCollider = transform.GetComponent<Collider>();
    }
    /// <summary>
    /// How to react on controller collision with rigid body object
    /// </summary>
    /// <param name="a_collision">Collider that the grounded trigger is contacting</param>
    private void OnTriggerStay(Collider a_collider)
    {
        // Get the contacted rigidbody
        t_RigidBody = null;
        t_RigidBody = a_collider.transform.GetComponent<Rigidbody>();
        
        // Apply force based on layer and reletive displacement
        if (t_RigidBody != null)
        {
            if (!t_RigidBody.isKinematic)
            {
                Vector3 closestPoint = a_collider.ClosestPoint(transform.position);
                Vector3 direction = (transform.position - closestPoint).normalized;
                t_RigidBody.AddForceAtPosition(
                        Vector3.up * Mathf.Clamp(Vector3.Dot(direction, Vector3.up), 0.0f, 1.0f) * m_Player.Mass * m_Player.Gravity,
                        closestPoint,
                        ForceMode.Force);
            }
        }
    }
}
