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
                if (Input.GetKey(KeyCode.Space) && charge < maxCharge)
                {
                    charge += Time.deltaTime * chargeSpeed;
                    float chargePercent = charge / maxCharge;
                    if (!chargeBar.parent.gameObject.activeSelf)
                    {
                        chargeBar.parent.gameObject.SetActive(true);
                    }
                    chargeBar.transform.localScale = new Vector3(chargePercent, chargeBar.transform.localScale.y, chargeBar.transform.localScale.z);
                }
                else if (Input.GetKeyUp(KeyCode.Space))
                {
                    Vector3 direction = rotater.right;
                    game.gameState.LaunchArrowServerRpc(arrowSpawn.position, direction, charge);
                    charge = 0;
                    chargeBar.transform.localScale = new Vector3(0, chargeBar.transform.localScale.y, chargeBar.transform.localScale.z);
                    ChangeShotValueServerRpc(true);
                }
            }




            //game.gameState.OnTurnEndServerRpc();

        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangeShotValueServerRpc(bool value)
        {
            hasShot.Value = value;
        }
    }
}

