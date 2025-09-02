using System;
using System.Threading;
using Unity.Netcode;
using UnityEngine;


namespace Assets.ArcherBattle
{
    public class ArcheryPlayer : NetworkBehaviour
    {

        public ArcherBattleGame game;
        public GameObject playerObject;
        [HideInInspector] public Transform rotater;

        [HideInInspector] public Transform chargeBar;
        [HideInInspector] public Transform arrowSpawn;

        public float rotateSpeed = 5f;

        public float maxCharge;

        public float chargeSpeed;
        public float charge;

        public NetworkVariable<bool> hasShot;

        public NetworkVariable<bool> isTurn = new NetworkVariable<bool>(false);

        void Start()
        {
            game = transform.parent.GetComponent<ArcherBattleGame>();
            isTurn.OnValueChanged += OnTurnChange;
        }

        private void OnTurnChange(bool previousValue, bool newValue)
        {
            if (!newValue)
            {
                rotater.localEulerAngles = Vector3.zero;                
            }
        }

        public void AssignPlayer()
        {
            if (OwnerClientId == game.connectedPlayers[0].OwnerClientId)
            {
                game.gameState.SpawnPlayersServerRpc(true, OwnerClientId);
            }
            else
            {
                game.gameState.SpawnPlayersServerRpc(false, OwnerClientId);
            }

        }

        public void AddListeners()
        {
            playerObject.GetComponent<Health>().Death.AddListener(OnDeath);
        }

        void OnDeath()
        {
            string steamName = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.GetComponent<SteamPlayer>().playerName;
            game.gameState.OnGameOver(steamName);
        }


        void Update()
        {

            if (!isTurn.Value || !IsOwner) return;

            if (rotater)
            {
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    rotater.Rotate(new Vector3(0, 0, Time.deltaTime * rotateSpeed));
                }
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    rotater.Rotate(new Vector3(0, 0, -Time.deltaTime * rotateSpeed));
                }
            }

            if (!hasShot.Value)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Vector3 direction = rotater.right;
                    game.gameState.LaunchArrowServerRpc(arrowSpawn.position, direction, 50f);
                    ChangeShotValueServerRpc(true);
                }
            }




            //game.gameState.OnTurnEndServerRpc();

        }

        // function if i can think of a reason for charging to be necessary. Why would anyone not use full charge????
        void Charge()
        {
            // if (charge < 16.6666666667f)
            // {
            //     charge = 16.6666666667f;
            // }

            // if (charge > maxCharge)
            // {
            //     charge = maxCharge;
            // }

            // if (Input.GetKeyDown(KeyCode.RightArrow) && charge < maxCharge)
            // {
            //     charge += 16.6666666667f;
            //     float chargePercent = charge / maxCharge;
            //     if (!chargeBar.parent.gameObject.activeSelf)
            //     {
            //         chargeBar.parent.gameObject.SetActive(true);
            //     }
            //     chargeBar.transform.localScale = new Vector3(chargePercent, chargeBar.transform.localScale.y, chargeBar.transform.localScale.z);
            // }
            // else if (Input.GetKeyDown(KeyCode.LeftArrow) && charge > 16.6666666667f)
            // {
            //     charge -= 16.6666666667f;
            //     float chargePercent = charge / maxCharge;
            //     if (!chargeBar.parent.gameObject.activeSelf)
            //     {
            //         chargeBar.parent.gameObject.SetActive(true);
            //     }
            //     chargeBar.transform.localScale = new Vector3(chargePercent, chargeBar.transform.localScale.y, chargeBar.transform.localScale.z);
            // }

            // if (Input.GetKeyDown(KeyCode.Space))
            // {
            //     Vector3 direction = rotater.right;
            //     game.gameState.LaunchArrowServerRpc(arrowSpawn.position, direction, charge);
            //     charge = 16.6666666667f;
            //     float chargePercent = charge / maxCharge;
            //     chargeBar.transform.localScale = new Vector3(chargePercent, chargeBar.transform.localScale.y, chargeBar.transform.localScale.z);
            //     ChangeShotValueServerRpc(true);
            // }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangeShotValueServerRpc(bool value)
        {
            hasShot.Value = value;
        }
    }
}

