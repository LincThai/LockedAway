using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndUI : MonoBehaviour
{
    GameManager GM;
    // Start is called before the first frame update
    void Start()
    {
        GM = GameObject.FindObjectOfType<GameManager>().GetComponent<GameManager>();
    }

    public void OnClickButton(int buttonClicked)
    {
        if (buttonClicked == 1)
        {
            GM.gameState = GameState.preGame;
        }
    }
}
