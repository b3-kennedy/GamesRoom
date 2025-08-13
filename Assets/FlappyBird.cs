using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class FlappyBird : ArcadeGame
{
    public GameObject mainMenu;
    public GameObject gameScene;

    public FlappyBirdLevel level;

    public enum GameState { MAIN_MENU, GAME, GAME_OVER }

    private GameState _gameState = GameState.MAIN_MENU;

    public TextMeshPro scoreText;
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
                MainMenuServerRpc();
                break;
            case GameState.GAME:
                GameStateServerRpc();
                level.SpawnPipesServerRpc();
 
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

    [ServerRpc(RequireOwnership = false)]
    void GameStateServerRpc()
    {
        GameStateClientRpc();
    }

    [ClientRpc]
    void GameStateClientRpc()
    {
        mainMenu.SetActive(false);
        gameScene.SetActive(true);
    }

    [ServerRpc(RequireOwnership = false)]
    void MainMenuServerRpc()
    {
        Debug.Log("main menu");
        MainMenuClientRpc();
    }

    [ClientRpc]
    void MainMenuClientRpc()
    {
        mainMenu.SetActive(true);
        gameScene.SetActive(false);
    }
    
}
