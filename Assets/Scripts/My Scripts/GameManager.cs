using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // set variables
    // game state variable setup
    public GameState gameState;

    // UI variables
    public GameObject startUI;
    public GameObject gameUI;
    public GameObject endUI;

    // Start is called before the first frame update
    void Start()
    {
        //set gamestate at the start of game
        gameState = GameState.preGame;
        // Set up UI's at start of game
        startUI.SetActive(true);
        gameUI.SetActive(false);
        endUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        GameStateCheck();
    }

    public void GameStateCheck()
    {
        if (gameState == GameState.preGame)
        {
            startUI.SetActive(true);
            gameUI.SetActive(false);
            endUI.SetActive(false);
        }
        else if (gameState == GameState.game)
        {
            startUI.SetActive(false);
            gameUI.SetActive(true);
            endUI.SetActive(false);
        }
        else if (gameState == GameState.end)
        {
            startUI.SetActive(false);
            gameUI.SetActive(false);
            endUI.SetActive(true);
        }
    }
}
public enum GameState {preGame, game, end};