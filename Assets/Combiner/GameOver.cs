using TMPro;
using UnityEngine;

namespace Assets.Combiner
{
    public class GameOver : State
    {
        CombinerGame combinerGame;
        public TextMeshPro scoreTMP;
        
        void Start()
        {
            if (game is CombinerGame cg)
            {
                combinerGame = cg;

            }
            if (IsServer)
            {
                Destroy(combinerGame.gameState.spawnedBall);
            }
            
            
            UpdateScore();
        }
        public override void OnStateEnter()
        {
            gameObject.SetActive(true);
            UpdateScore();
        }

        public void UpdateScore()
        {
            if (!combinerGame) return;
            scoreTMP.text = $"Score: {combinerGame.gameState.score.Value}";
        }

        public override void OnStateUpdate()
        {
            if(Input.GetKeyDown(KeyCode.R))
            {
                combinerGame.ResetServerRpc();
            }
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }
    }
}

