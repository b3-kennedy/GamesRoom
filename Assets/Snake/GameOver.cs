using TMPro;
using UnityEngine;

namespace Assets.Snake
{
    public class GameOver : State
    {
        SnakeGame snakeGame;

        public TextMeshPro scoreTMP;
        void Start()
        {
            if (game is SnakeGame sg)
            {
                snakeGame = sg;
            }
        }
        public override void OnStateEnter()
        {
            gameObject.SetActive(true);
            scoreTMP.text = $"Score: {snakeGame.gameState.score.Value}";
            
        }

        public override void OnStateUpdate()
        {
            if(Input.GetKeyDown(KeyCode.R))
            {
                snakeGame.ResetServerRpc();
            }
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }
    }
}

