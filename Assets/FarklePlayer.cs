using System.Collections.Generic;
using Assets.Farkle;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Farkle
{
    public class FarklePlayer : NetworkBehaviour
    {

        public FarkleGame farkleGame;
        public NetworkVariable<bool> isTurn;
        public GameObject dicePrefab;

        public List<Transform> dicePositions = new List<Transform>();

        public List<GameObject> spawnedDice = new List<GameObject>();

        public bool isPlayer1;

        bool hasRolled;

        Wager wagerState;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (farkleGame.wagerState is Wager wager)
            {
                wagerState = wager;
            }
        }

        void WagerState()
        {
            if (farkleGame.netGameState.Value == FarkleGame.GameState.WAGER && isPlayer1)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow) && wagerState.wagerAmount.Value > 0)
                {
                    wagerState.SetWagerAmountServerRpc(-10);
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) && wagerState.wagerAmount.Value < 500)
                {
                    wagerState.SetWagerAmountServerRpc(10);
                }

                if (Input.GetKeyDown(KeyCode.Return))
                {

                }
            }
            if (farkleGame.netGameState.Value == FarkleGame.GameState.WAGER && !isPlayer1)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    wagerState.LockInAmountServerRpc(true);
                }

                if (Input.GetKeyDown(KeyCode.Y) && wagerState.player2Buttons.gameObject.activeSelf)
                {
                    farkleGame.ChangeStateServerRpc(FarkleGame.GameState.GAME);
                }

                if (Input.GetKeyDown(KeyCode.N) && wagerState.player2Buttons.gameObject.activeSelf)
                {
                    wagerState.LockInAmountServerRpc(false);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            WagerState();
            if (farkleGame.netGameState.Value == FarkleGame.GameState.GAME && isTurn.Value)
            {
                if (!hasRolled)
                {
                    RollDiceServerRpc();

                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void RollDiceServerRpc()
        {
            for (int i = 0; i < 6; i++)
            {
                GameObject dice = Instantiate(dicePrefab, dicePositions[i].position, Quaternion.identity);
                spawnedDice.Add(dice);
                dice.GetComponent<NetworkObject>().Spawn();

            }
            SetHasRolled();
        }

        [ClientRpc]
        void SetHasRolled()
        {
            hasRolled = true;
        }
    }
}

