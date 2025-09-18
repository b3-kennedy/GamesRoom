using UnityEngine;

namespace Assets.Dodger
{
    public class GameOver : State
    {

        DodgerGame dodgerGame;

        void Start()
        {
            if (game is DodgerGame dg)
            {
                dodgerGame = dg;
            }
        }

        public override void OnStateEnter()
        {
            gameObject.SetActive(true);
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }
    }
}

