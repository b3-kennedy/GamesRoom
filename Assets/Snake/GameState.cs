using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

namespace Assets.Snake
{
    public class GameState : State
    {
        SnakeGame snakeGame;
        public float gridSize = 0.3f;
        public int width = 25;
        public int height = 25;

        public float gridUpdateInterval = 0.25f;
        float timer;
        public NetworkList<int> grid = new NetworkList<int>();

        public GameObject snakePiecePrefab;

        SnakePlayer player;

        public GameObject tilePrefab;
        private GameObject[,] tiles;

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
        }
        public override void OnStateEnter()
        {
            gameObject.SetActive(true);
            UpdateGrid();
            //Instantiate(snakePiecePrefab, GetGridPosition(5, 5), Quaternion.identity);
        }

        public override void OnStateUpdate()
        {
            if (!IsServer) return;
            
            timer += Time.deltaTime;
            if(timer >= gridUpdateInterval)
            {
                player.Move();
                UpdateGrid();
                timer = 0;
            }
        }
        
        void UpdateGrid()
        {
            for (int i = 0; i < grid.Count; i++)
            {
                if (grid[i] != 0)
                {
                    grid[i] = 0;
                }
            }

            for (int i = 0; i < player.snakePositions.Count; i++)
            {
                Vector2 location = player.snakePositions[i];
                int x = (int)location.x;
                int y = (int)location.y;
                Vector3 pos = GetGridPosition(x, y);
                SetCell(x, y, 1);
            }
        }

        private void OnGridChanged(NetworkListEvent<int> changeEvent)
        {
            int index = changeEvent.Index;
            int x = index % width;
            int y = index / height;

            MeshRenderer meshRenderer = tiles[x, y].GetComponent<MeshRenderer>();
            if (grid[index] == 1)
                meshRenderer.material.color = Color.white;
            else
                meshRenderer.material.color = Color.black;
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

