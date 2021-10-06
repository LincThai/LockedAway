using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartUI : MonoBehaviour
{
    // game manager variable
    GameManager GM;


    // Start is called before the first frame update
    void Start()
    {
        // call game manager from the game manager game object
        GM = GameObject.FindObjectOfType<GameManager>().GetComponent<GameManager>();
    }

    public void OnClickButton(int buttonClicked)
        {
            if (buttonClicked == 1)
            {
                GM.gameState = GameState.game;
            }
            if (buttonClicked == 2)
            {
                Application.Quit();
            }
        }
}
