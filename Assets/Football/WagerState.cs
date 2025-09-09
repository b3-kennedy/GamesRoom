using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;

namespace Assets.Football
{
    public class WagerState : State
    {
        FootballGame footballGame;
        void Start()
        {
            if (game is FootballGame fg)
            {
                footballGame = fg;
            }
        }
        public override void OnStateEnter()
        {
            gameObject.SetActive(true);
        }

        public override void OnStateUpdate()
        {

        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }
    }
}
