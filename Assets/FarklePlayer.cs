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

        public List<int> selectedDiceValues = new List<int>();

        public NetworkVariable<int> playerScore = new NetworkVariable<int>();
        public NetworkVariable<int> roundScore = new NetworkVariable<int>();

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
            
            if (farkleGame.netGameState.Value == FarkleGame.GameState.GAME && isTurn.Value)
            {
                SelectDice();
            }
        }

        void SelectDice()
        {
            if (spawnedDice.Count < 6) return;

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                selectedDiceIndex--;
                if (selectedDiceIndex < 0) selectedDiceIndex = 5;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                selectedDiceIndex++;
                if (selectedDiceIndex > 5) selectedDiceIndex = 0;
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                SelectedDiceServerRpc(spawnedDice[selectedDiceIndex].GetComponent<NetworkObject>().NetworkObjectId);
            }

            if (Input.GetKeyDown(KeyCode.End))
            {
                RemoveSelectedDiceServerRpc();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {

                OnSwitchTurnServerRpc();
                farkleGame.SwitchTurnServerRpc(isPlayer1);
                hasRolled = false;
            }




            spawnedSelectGraphic.transform.position = spawnedDice[selectedDiceIndex].transform.position;

        }

        [ServerRpc(RequireOwnership = false)]
        void RemoveSelectedDiceServerRpc()
        {
            for (int i = spawnedDice.Count - 1; i >= 0; i--)
            {
                if (spawnedDice[i].GetComponent<FarkleDice>().isSelected.Value)
                {
                    spawnedDice[i].GetComponent<NetworkObject>().Despawn(true);
                    spawnedDice.RemoveAt(i);
                }
            }

        }


        [ServerRpc(RequireOwnership = false)]
        void CalculateDiceScoreServerRpc()
        {
            Dictionary<int, int> scoringDictionary = new Dictionary<int, int>();

            foreach (var value in selectedDiceValues)
            {
                if (scoringDictionary.ContainsKey(value))
                {
                    scoringDictionary[value]++;
                }
                else
                {
                    scoringDictionary[value] = 1;
                }
            }

            roundScore.Value = 0;
            foreach (var kvp in scoringDictionary)
            {
                int face = kvp.Key;
                int count = kvp.Value;

                if (count >= 3)
                {
                    if (face == 1)
                    {
                        roundScore.Value += 1000;
                    }
                    else
                    {
                        roundScore.Value += face * 100;
                    }

                    count -= 3;
                }

                if (face == 1)
                {
                    roundScore.Value += count * 100;
                }
                else if (face == 5)
                {
                    roundScore.Value += count * 50;
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void SetDiceScoreServerRpc(int score)
        {
            playerScore.Value = score;
        }

        [ServerRpc(RequireOwnership = false)]
        void SelectedDiceServerRpc(ulong netObjID)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObjID, out var dice))
            {
                if (!dice.GetComponent<FarkleDice>().isSelected.Value)
                {
                    dice.GetComponent<FarkleDice>().isSelected.Value = true;
                    selectedDiceValues.Add(dice.GetComponent<FarkleDice>().diceValue.Value);
                }
                else
                {
                    dice.GetComponent<FarkleDice>().isSelected.Value = false;
                    selectedDiceValues.Remove(dice.GetComponent<FarkleDice>().diceValue.Value);
                }

            }

            CalculateDiceScoreServerRpc();
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
            playerScore.Value += roundScore.Value;
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

