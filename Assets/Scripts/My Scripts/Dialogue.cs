using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// add atribute to make editable in investigater
[System.Serializable]
public class Dialogue
{
    // set variables
    public string name;

    // add atribute increase text lines 
    [TextArea(3, 10)]
    //set array
    public string[] sentences;
}
