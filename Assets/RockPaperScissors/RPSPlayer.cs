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

        [HideInInspector] public int index;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            isPicking.OnValueChanged += OnTurnChange;
        }

        private void OnTurnChange(bool previousValue, bool newValue)
        {

            OnTurnChange();

        }


        public void OnTurnChange()
        {

            if (!IsOwner) return;
            if (isPicking.Value)
            {
                rpsGame.gameState.pickScreen.SetActive(true);
                rpsGame.gameState.pickingTMP.gameObject.SetActive(false);
            }
            else
            {
                rpsGame.gameState.pickScreen.SetActive(false);
                rpsGame.gameState.pickingTMP.gameObject.SetActive(true);
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
            if (!IsOwner) return;

            if (!rpsGame) return;

            if (isPicking.Value && rpsGame.gameState.pickScreen.activeSelf)
            {
                if(Input.GetKeyDown(KeyCode.DownArrow))
                {
                    index++;
                    if(index > 2)
                    {
                        index = 0;
                    }
                }
                
                if(Input.GetKeyDown(KeyCode.UpArrow))
                {
                    index--;
                    if(index < 0)
                    {
                        index = 2;
                    }
                }

                if (Input.GetKeyDown(KeyCode.Return) && rpsGame.netGameState.Value == RockPaperScissorsGame.GameState.GAME)
                {
                    
                    if(index == 0)
                    {
                        rpsGame.gameState.SelectItemServerRpc(isLeftPlayer, GameState.SelectedItem.ROCK);
                    }
                    else if (index == 1)
                    {
                        rpsGame.gameState.SelectItemServerRpc(isLeftPlayer, GameState.SelectedItem.PAPER);
                    }
                    else if (index == 2)
                    {
                        rpsGame.gameState.SelectItemServerRpc(isLeftPlayer, GameState.SelectedItem.SCISSORS);
                    }
                }
                Select();
            }
            
            if(rpsGame.netGameState.Value == RockPaperScissorsGame.GameState.WAGER && isLeftPlayer)
            {
                if(Input.GetKeyDown(KeyCode.RightArrow))
                {
                    rpsGame.wagerState.ChangeWagerAmountServerRpc(10);
                }
                
                if(Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    rpsGame.wagerState.ChangeWagerAmountServerRpc(-10);
                }
                
                if(Input.GetKeyDown(KeyCode.Return))
                {
                    rpsGame.wagerState.ChangePlayer1LockedInStateServerRpc();
                }
            }

            if (rpsGame.netGameState.Value == RockPaperScissorsGame.GameState.WAGER && !isLeftPlayer)
            {
                if (rpsGame.wagerState.isPlayer1Locked.Value)
                {
                    if (Input.GetKeyDown(KeyCode.Y))
                    {
                        rpsGame.ChangeStateServerRpc(RockPaperScissorsGame.GameState.GAME);
                    }

                    if (Input.GetKeyDown(KeyCode.N))
                    {
                        rpsGame.wagerState.ChangePlayer1LockedInStateServerRpc();
                    }
                }
            }
        }
        
        void Select()
        {
            for (int i = 0; i < rpsGame.gameState.items.Length; i++)
            {
                if(i == index)
                {
                    rpsGame.gameState.items[i].GetComponent<RPSItem>().ChangeSelectedValue(true);
                }
                else
                {
                    rpsGame.gameState.items[i].GetComponent<RPSItem>().ChangeSelectedValue(false);
                }
            }
        }
    }
}

