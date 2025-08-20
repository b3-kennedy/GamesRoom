using System;
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

        public GameObject selectGraphic;

        GameObject spawnedSelectGraphic;

        public List<Transform> dicePositions = new List<Transform>();

        public List<GameObject> spawnedDice = new List<GameObject>();

        public bool isPlayer1;

        bool hasRolled;

        int selectedDiceIndex;

        Wager wagerState;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (farkleGame.wagerState is Wager wager)
            {
                wagerState = wager;
            }

            isTurn.OnValueChanged += OnTurnChanged;
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
                    wagerState.LockInAmountServerRpc(true);
                }
            }
            if (farkleGame.netGameState.Value == FarkleGame.GameState.WAGER && !isPlayer1)
            {
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
            if (!IsOwner) return;

            WagerState();
            SelectDice();
            if (farkleGame.netGameState.Value == FarkleGame.GameState.GAME && isTurn.Value)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {

                    OnSwitchTurnServerRpc();
                    farkleGame.SwitchTurnServerRpc(isPlayer1);
                    hasRolled = false;
                }
            }
        }

        void SelectDice()
        {
            if (spawnedDice.Count < 6) return;

            if (Input.GetKeyDown(KeyCode.LeftArrow) && selectedDiceIndex > 0)
            {
                selectedDiceIndex--;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) && selectedDiceIndex < 6)
            {
                selectedDiceIndex++;
            }

            spawnedSelectGraphic.transform.position = spawnedDice[selectedDiceIndex].transform.position;
   
        }

        private void OnTurnChanged(bool previousValue, bool newValue)
        {
            if (!previousValue && newValue)
            {
                if (!hasRolled && spawnedDice.Count == 0)
                {
                    if (IsServer)
                    {
                        spawnedSelectGraphic = Instantiate(selectGraphic, dicePositions[0].transform.position, Quaternion.identity);
                        spawnedSelectGraphic.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
                        SetSelectGraphicClientRpc(spawnedSelectGraphic.GetComponent<NetworkObject>().NetworkObjectId);
                    }
                    
                    RollDiceServerRpc();
                    hasRolled = true;
                }
            }
            else
            {
                if (spawnedSelectGraphic && IsServer)
                {
                    spawnedSelectGraphic.GetComponent<NetworkObject>().Despawn(true);
                }
            }
        }

        [ClientRpc]
        void SetSelectGraphicClientRpc(ulong netObjID)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObjID, out var graphic))
            {
                spawnedSelectGraphic = graphic.gameObject;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void OnSwitchTurnServerRpc()
        {
            for (int i = spawnedDice.Count - 1; i >= 0; i--)
            {
                spawnedDice[i].GetComponent<NetworkObject>().Despawn(true);
            }
            OnSwitchTurnClientRpc();
        }

        [ClientRpc]
        void OnSwitchTurnClientRpc()
        {
            spawnedDice.Clear();
        }



        [ServerRpc(RequireOwnership = false)]
        void RollDiceServerRpc()
        {
            for (int i = 0; i < 6; i++)
            {
                GameObject dice = Instantiate(dicePrefab, dicePositions[i].position, Quaternion.identity);
                spawnedDice.Add(dice);
                dice.GetComponent<NetworkObject>().Spawn();
                SetDiceListClientRpc(dice.GetComponent<NetworkObject>().NetworkObjectId);

            }
            SetHasRolledClientRpc();
        }

        [ClientRpc]
        void SetDiceListClientRpc(ulong netObjID)
        {
            if (IsServer) return;

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObjID, out var dice))
            {
                spawnedDice.Add(dice.gameObject);
            }
        }

        [ClientRpc]
        void SetHasRolledClientRpc()
        {
            hasRolled = true;
        }
    }
}

