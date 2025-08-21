using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Farkle;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Farkle
{
    public class FarklePlayer : NetworkBehaviour
    {
        public TextMeshPro playerScoreText;
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
            playerScore.OnValueChanged += OnPlayerScoreChanged;
        }

        private void OnPlayerScoreChanged(int previousValue, int newValue)
        {
            var playerObject = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject;
            playerScoreText.text = $"{playerObject.GetComponent<SteamPlayer>().playerName}: {playerScore.Value}";
        }

        private void OnRoundScoreChanged(int previousValue, int newValue)
        {
            throw new NotImplementedException();
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
            //if (spawnedDice.Count < 6) return;

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                selectedDiceIndex--;
                if (selectedDiceIndex < 0) selectedDiceIndex = spawnedDice.Count-1;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                selectedDiceIndex++;
                if (selectedDiceIndex > spawnedDice.Count-1) selectedDiceIndex = 0;
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



            if (spawnedSelectGraphic)
            {
                spawnedSelectGraphic.transform.position = spawnedDice[selectedDiceIndex].transform.position;
            }
            

        }



        void CalculateDiceScore()
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

            int score = 0;
            foreach (var kvp in scoringDictionary)
            {
                int face = kvp.Key;
                int count = kvp.Value;

                if (count >= 3)
                {
                    if (face == 1)
                    {
                        score += 1000;
                    }
                    else
                    {
                        score += face * 100;
                    }

                    count -= 3;
                }

                if (face == 1)
                {
                    score += count * 100;
                }
                else if (face == 5)
                {
                    score += count * 50;
                }
            }
            roundScore.Value = score;

            
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

            CalculateDiceScore();
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

                    RollDiceServerRpc(6);
                    hasRolled = true;
                }
            }
            else
            {
                hasRolled = false;
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
            roundScore.Value = 0;
            RemoveDiceServerRpc();
            OnSwitchTurnClientRpc();
        }

        [ClientRpc]
        void OnSwitchTurnClientRpc()
        {
            spawnedDice.Clear();
            selectedDiceValues.Clear();
        }

        [ServerRpc(RequireOwnership = false)]
        void RemoveDiceServerRpc()
        {
            if (spawnedDice.Count == 0) return;

            for (int i = spawnedDice.Count - 1; i >= 0; i--)
            {
                spawnedDice[i].GetComponent<NetworkObject>().Despawn(true);
            }
            spawnedDice.Clear();
            ClearDiceListClientRpc();
        }

        [ClientRpc]
        void ClearDiceListClientRpc()
        {
            spawnedDice.Clear();
        }

        [ServerRpc(RequireOwnership = false)]
        void RollDiceServerRpc(int amountToRoll)
        {
            RemoveDiceServerRpc();
            for (int i = 0; i < amountToRoll; i++)
            {
                GameObject dice = Instantiate(dicePrefab, dicePositions[i].position, Quaternion.identity);
                spawnedDice.Add(dice);
                dice.GetComponent<NetworkObject>().Spawn();
                SetDiceListClientRpc(dice.GetComponent<NetworkObject>().NetworkObjectId);

            }
            if (CheckDice())
            {
                Debug.Log("continue");
            }
            else
            {
                roundScore.Value = 0;
                StartCoroutine(SwitchTurnAfterTime(3));
            }
            
            SetHasRolledClientRpc();
        }

        IEnumerator SwitchTurnAfterTime(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            OnSwitchTurnServerRpc();
            farkleGame.SwitchTurnServerRpc(isPlayer1);
            hasRolled = false;
        }

        bool CheckDice()
        {
            if (spawnedDice.Count == 0) return false;

            // Count occurrences of each dice value
            Dictionary<int, int> counts = new Dictionary<int, int>();
            foreach (var dice in spawnedDice)
            {
                int value = dice.GetComponent<FarkleDice>().diceValue.Value;
                if (counts.ContainsKey(value))
                    counts[value]++;
                else
                    counts[value] = 1;
            }

            // Check for scoring combinations
            foreach (var kvp in counts)
            {
                int face = kvp.Key;
                int count = kvp.Value;

                // Three or more of a kind
                if (count >= 3)
                {
                    return true;
                }

                // Single 1 or 5
                if (face == 1 || face == 5)
                {
                    return true;
                }
            }

            // No scoring combinations found
            return false;
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

            ModifyDiceListClientRpc();

            int diceToRoll = spawnedDice.Count > 0 ? spawnedDice.Count : 6;
            RollDiceServerRpc(diceToRoll);  // only server spawns dice
        }

        [ClientRpc]
        void ModifyDiceListClientRpc()
        {
            selectedDiceValues.Clear();
            for (int i = spawnedDice.Count - 1; i >= 0; i--)
            {
                if (spawnedDice[i] == null)
                {
                    spawnedDice.RemoveAt(i);
                }
            }
            
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
            selectedDiceIndex = 0;
        }
    }
}

