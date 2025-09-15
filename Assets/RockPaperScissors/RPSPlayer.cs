using System;
using Unity.Netcode;
using UnityEngine;

namespace Assets.RockPaperScissors
{
    public class RPSPlayer : NetworkBehaviour
    {
        [HideInInspector]public RockPaperScissorsGame rpsGame;

        public NetworkVariable<bool> isPicking;
        public NetworkVariable<bool> isLockedIn;

        public bool isLeftPlayer;

        [HideInInspector] public GameObject playerObject;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            isPicking.OnValueChanged += OnTurnChange;
        }

        private void OnTurnChange(bool previousValue, bool newValue)
        {

            OnTurnChangeClientRpc(newValue);

        }


        [ClientRpc]
        void OnTurnChangeClientRpc(bool value)
        {
            if (value)
            {

            }
            else
            {
                if (isLeftPlayer)
                {
                    rpsGame.gameState.pickingTMP.text = $"{rpsGame.rightPlayerName} is picking...";
                }
                else
                {
                    rpsGame.gameState.pickingTMP.text = $"{rpsGame.leftPlayerName} is picking...";
                }
                
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

