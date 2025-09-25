using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Snake
{
    public class SnakePlayer : NetworkBehaviour
    {
        [HideInInspector] public SnakeGame game;
        [HideInInspector] public GameObject playerObject;
        
        public enum Direction {UP, DOWN, RIGHT, LEFT};
        public NetworkVariable<Direction> direction = new NetworkVariable<Direction>(Direction.UP);
        public NetworkList<Vector2Int> snakePositions = new NetworkList<Vector2Int>();
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (IsServer)
            {
                if (snakePositions.Count == 0)
                {
                    snakePositions.Add(new Vector2Int(12, 12));
                    snakePositions.Add(new Vector2Int(12, 11));
                    snakePositions.Add(new Vector2Int(12, 10));
                    snakePositions.Add(new Vector2Int(12, 9));
                }
            }

        }

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                ChangeDirectionServerRpc(Direction.UP);
            }
            else if(Input.GetKeyDown(KeyCode.DownArrow))
            {
                ChangeDirectionServerRpc(Direction.DOWN);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ChangeDirectionServerRpc(Direction.RIGHT);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ChangeDirectionServerRpc(Direction.LEFT);
            }
        }

        [ServerRpc]
        void ChangeDirectionServerRpc(Direction newDirection)
        {
            direction.Value = newDirection;
        }

        public void Move()
        {
            if (!IsServer) return;
            if (snakePositions.Count == 0) return;

            var currentPos = snakePositions[0];
            Vector2Int newPos;

            switch (direction.Value)
            {
                case Direction.UP:
                    newPos = new Vector2Int(currentPos.x, currentPos.y + 1);
                    break;
                case Direction.DOWN:
                    newPos = new Vector2Int(currentPos.x, currentPos.y - 1);
                    break;
                case Direction.LEFT:
                    newPos = new Vector2Int(currentPos.x - 1, currentPos.y);
                    break;
                case Direction.RIGHT:
                    newPos = new Vector2Int(currentPos.x + 1, currentPos.y);
                    break;
                default:
                    return;
            }
            snakePositions[0] = newPos;

            for (int i = snakePositions.Count - 1; i > 0; i--)
            {
                snakePositions[i] = snakePositions[i - 1];
            }
        }
        
    }
}

