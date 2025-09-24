using UnityEngine;

namespace Assets.Combiner
{
    public class Leaderboards : State
    {
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

