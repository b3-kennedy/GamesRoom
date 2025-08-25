using UnityEngine;

namespace Assets.CreditClicker
{
    public class GameState : State
    {
        public override void OnStateEnter()
        {
            gameObject.SetActive(true);
        }

        public override void OnStateUpdate()
        {
            base.OnStateEnter();
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }

    }
}

