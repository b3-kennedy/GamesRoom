using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class FlappyBird : ArcadeGame
{
    public GameObject mainMenu;
    public GameObject gameScene;

    public FlappyBirdLevel level;

    public enum GameState { MAIN_MENU, GAME, GAME_OVER }

    private GameState _gameState = GameState.MAIN_MENU;
    public GameState gameState
    {
        get => _gameState;
        set
        {
            if (_gameState != value)
            {
                _gameState = value;
                OnGameStateChanged(_gameState);
            }
        }
    }

    void Start()
    {
        gameScene.SetActive(false);
        mainMenu.SetActive(true);
    }

    public override void Begin()
    {
        gameState = GameState.GAME;
    }

    private void OnGameStateChanged(GameState newState)
    {
        // This method fires **once** when the game state changes
        Debug.Log("Game state changed to: " + newState);

        switch (newState)
        {
            case GameState.MAIN_MENU:
                mainMenu.SetActive(true);
                gameScene.SetActive(false);
                break;
            case GameState.GAME:
                level.SpawnPipes();
                mainMenu.SetActive(false);
                gameScene.SetActive(true);
                break;
            case GameState.GAME_OVER:
                // Handle game over UI
                break;
        }
    }

    void Update()
    {
        if (gameState == GameState.GAME)
        {
            level.Move();
            
        }
    }
}
