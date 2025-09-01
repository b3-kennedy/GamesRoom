using UnityEditor;
using UnityEngine;
using System.IO;

public class CreateMinigameWindow : EditorWindow
{
    private string minigameName = "NewMinigame";

    [MenuItem("Tools/Create Minigame Script")]
    public static void ShowWindow()
    {
        GetWindow<CreateMinigameWindow>("Create Minigame");
    }

    void OnGUI()
    {
        GUILayout.Label("Minigame Generator", EditorStyles.boldLabel);

        minigameName = EditorGUILayout.TextField("Minigame Name", minigameName);

        if (GUILayout.Button("Create Script"))
        {
            CreateMinigameScript(minigameName);
        }
    }

    private void CreateMinigameScript(string gameName)
    {
        // Ensure valid name
        if (string.IsNullOrEmpty(gameName))
        {
            EditorUtility.DisplayDialog("Error", "Please enter a valid name!", "OK");
            return;
        }

        // Folder path
        string folderPath = $"Assets/{gameName}";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        // File path
        string scriptPath = $"{folderPath}/{gameName}Game.cs";

        if (File.Exists(scriptPath))
        {
            EditorUtility.DisplayDialog("Error", "A script with this name already exists!", "OK");
            return;
        }

        // Template content
        string scriptContent = $@"using UnityEngine;
        using Unity.Netcode;

        namespace {gameName}
        {{
            public class {gameName}Game : Game
            {{
                public enum GameState {{ MAIN_MENU, GAME, GAME_OVER }}

                public NetworkVariable<GameState> netGameState = new NetworkVariable<GameState>(
                    GameState.MAIN_MENU,
                    NetworkVariableReadPermission.Everyone,
                    NetworkVariableWritePermission.Server
                );

                void Start()
                {{
                    // Listen for state changes
                    netGameState.OnValueChanged += OnNetworkGameStateChanged;

                    // Apply initial state locally
                    ApplyState(netGameState.Value);
                }}

                [ServerRpc(RequireOwnership = false)]
                public override void BeginServerRpc(ulong clientID)
                {{


                }}

                private void OnNetworkGameStateChanged(GameState oldState, GameState newState)
                {{
                    LeaveState(oldState);
                    Debug.Log($""Game state changed from {{oldState}} to {{newState}}"");
                    ApplyState(newState);
                }}

                [ServerRpc(RequireOwnership = false)]
                public void ChangeStateServerRpc(GameState newState)
                {{
                    netGameState.Value = newState;
                }}

                void LeaveState(GameState state)
                {{
                    switch (state)
                    {{
                        case GameState.MAIN_MENU:                    
                            break;
                        case GameState.GAME:                    
                            break;
                        case GameState.GAME_OVER:
                            break;
                    }}
                }}

                private void ApplyState(GameState state)
                {{
                    switch (state)
                    {{
                        case GameState.MAIN_MENU:
                            break;

                        case GameState.GAME:
                            break;

                        case GameState.GAME_OVER:
                            break;
                    }}
                }}
            }}
        }}
        ";

        // Write file
        File.WriteAllText(scriptPath, scriptContent);

        // Refresh Unity asset database
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", $"{gameName}Game.cs created!", "OK");
    }
}
