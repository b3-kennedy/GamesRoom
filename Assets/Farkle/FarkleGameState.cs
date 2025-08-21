using UnityEngine;

namespace Assets.Farkle
{
    public class FarkleGameState : State
    {

        FarkleGame farkleGame;

        void Start()
        {
            gameObject.SetActive(false);
            if (game is FarkleGame fg)
            {
                farkleGame = fg;
            }
        }

        public override void OnStateEnter()
        {
            gameObject.SetActive(true);
            Debug.Log("gamee");
            if (IsServer)
            {
                int randomNum = Random.Range(0, 2);
                farkleGame.SetFirstTurn(randomNum);

            }
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }
    }
}

