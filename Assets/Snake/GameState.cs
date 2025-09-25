using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

namespace Assets.Snake
{
    public class GameState : State
    {
        SnakeGame snakeGame;
        public float gridSize = 0.3f;
        public int width = 26;
        public int height = 26;
        public float gridUpdateInterval = 0.25f;
        float timer;
        public NetworkList<int> grid = new NetworkList<int>();
        public GameObject snakePiecePrefab;
        SnakePlayer player;
        public GameObject tilePrefab;
        private GameObject[,] tiles;

        public NetworkVariable<int> score = new NetworkVariable<int>();
        bool isFoodSpawned = false;

        void Start()
        {
            if (game is SnakeGame sg)
            {
                snakeGame = sg;
            }
            player = snakeGame.player;

            grid.OnListChanged += OnGridChanged;
        }

        public override void OnNetworkSpawn()
        {
            if(IsServer)
            {
                for (int i = 0; i < width * height; i++)
                {
                    grid.Add(0);
                }

                for (int x = 0; x < width; x++)
                {
                    SetCell(x, 0, 3);             // bottom edge
                    SetCell(x, height - 1, 3);    // top edge
                }
                for (int y = 0; y < height; y++)
                {
                    SetCell(0, y, 3);             // left edge
                    SetCell(width - 1, y, 3);     // right edge
                }
            }

            tiles = new GameObject[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector3 pos = GetGridPosition(x, y);
                    GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                    tile.transform.localScale = Vector3.one * 0.3f;
                    tiles[x, y] = tile;
                }
            }

            UpdateAllTiles();
        }

        private void UpdateAllTiles()
        {
            for (int i = 0; i < grid.Count; i++)
            {
                int x = i % width;
                int y = i / width;

                MeshRenderer meshRenderer = tiles[x, y].GetComponent<MeshRenderer>();
                switch (grid[i])
                {
                    case 0: meshRenderer.material.color = Color.black; break;
                    case 1: meshRenderer.material.color = Color.white; break;
                    case 2: meshRenderer.material.color = Color.red; break;
                    case 3: meshRenderer.material.color = Color.white; break;
                }
            }
        }

        public override void OnStateEnter()
        {
            gameObject.SetActive(true);
            UpdateGrid();
            
        }

        public override void OnStateUpdate()
        {
            if (!IsServer) return;
            
            timer += Time.deltaTime;
            if(timer >= gridUpdateInterval)
            {
                
                UpdateGrid();
                timer = 0;
            }
            
            if(!isFoodSpawned)
            {
                SpawnFood();
                isFoodSpawned = true;
            }
        }
        
        void UpdateGrid()
        {
            if (!player) return;

                for (int i = 0; i < grid.Count; i++)
            {
                if (grid[i] != 0 && grid[i] != 2 && grid[i] != 3)
                {
                    grid[i] = 0;
                }
            }
            
            if(IsSnakeHeadAtValue(2))
            {
                player.Grow();
                score.Value += 10;
                isFoodSpawned = false;
            }

           
            for (int i = 0; i < player.snakePositions.Count; i++)
            {
                Vector2 location = player.snakePositions[i];
                int x = (int)location.x;
                int y = (int)location.y;
                Vector3 pos = GetGridPosition(x, y);
                SetCell(x, y, 1);
            }

            player.Move();

            if (IsSnakeHeadAtValue(1) || IsSnakeHeadAtValue(3))
            {
                snakeGame.ChangeStateServerRpc(SnakeGame.GameState.GAME_OVER);
            }
        }
        
        void SpawnFood()
        {
            int x = Random.Range(1, width);
            int y = Random.Range(1, height);
            SetCell(x, y, 2);
        }

        private void OnGridChanged(NetworkListEvent<int> changeEvent)
        {
            int index = changeEvent.Index;
            int x = index % width;
            int y = index / height;

            MeshRenderer meshRenderer = tiles[x, y].GetComponent<MeshRenderer>();
            if (grid[index] == 1 || grid[index] == 3)
            {
                meshRenderer.material.color = Color.white;
            }
            else if(grid[index] == 2)
            {
                meshRenderer.material.color = Color.red;
            }
            else
            {
                meshRenderer.material.color = Color.black;
            }
                
        }

        public bool IsSnakeHeadAtValue(int value)
        {
            if (player == null || !player.IsSpawned) return false;
            if (player.snakePositions == null || player.snakePositions.Count == 0) return false;

            Vector2Int headPosition = player.snakePositions[0];

            // Check bounds first
            if (headPosition.x < 0 || headPosition.x >= width ||
                headPosition.y < 0 || headPosition.y >= height)
                return false;

            return GetCell(headPosition.x, headPosition.y) == value;
        }

        

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }

        private int GetIndex(int x, int y)
        {
            return y * width + x;
        }
        public void SetCell(int x, int y, int value)
        {
            if (IsServer) // Only server writes
            {
                grid[GetIndex(x, y)] = value;
            }
        }

        public int GetCell(int x, int y)
        {
            return grid[GetIndex(x, y)];
        }
        
        public Vector3 GetGridPosition(int x, int y)
        {
            return transform.position + new Vector3(x * gridSize, y * gridSize, 1.28f);
        }
    }
}

