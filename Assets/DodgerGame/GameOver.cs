using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Dodger
{
    public class GameOver : State
    {

        DodgerGame dodgerGame;
        public TextMeshPro scoreTMP;

        void Start()
        {
            if (game is DodgerGame dg)
            {
                dodgerGame = dg;
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
            if (!dodgerGame) return;
            scoreTMP.text = $"Score: {dodgerGame.gameState.score.Value}";
        }

        public override void OnStateUpdate()
        {
            if(Input.GetKeyDown(KeyCode.R))
            {
                dodgerGame.ResetServerRpc();
            }
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }
    }
}

