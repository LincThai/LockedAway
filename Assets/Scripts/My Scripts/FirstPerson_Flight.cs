using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPerson_Flight : MonoBehaviour
{
    // set variables
    public bool canFly;
    public float speed;

    //list for rigid bodies
    public List<Rigidbody> myBody;

    // Update is called once per frame
    void Update()
    {
        // if input is q toggle can fly
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Fly();
        }
        if (Input.GetKey(KeyCode.J))
        {
            transform.position += transform.up * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.K))
        {
            transform.position -= transform.up * speed * Time.deltaTime;
        }
    }
    
    public void Fly()
    {
        canFly = !canFly;
        for (int i = 0; i < myBody.Count; i++)
        {
            myBody[i].isKinematic = canFly;
        }
    }
}
