using System.Threading;
using Unity.Netcode;
using UnityEngine;


namespace Assets.ArcherBattle
{
    public class ArcheryPlayer : NetworkBehaviour
    {

        public ArcherBattleGame game;
        public GameObject playerObject;
        public Transform rotater;

        public Transform arrowSpawn;

        public float rotateSpeed = 5f;

        public float maxCharge;

        public float chargeSpeed;
        public float charge;

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

            if (Input.GetKey(KeyCode.Space) && charge < maxCharge)
            {
                charge += Time.deltaTime * chargeSpeed;
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                Vector3 direction = rotater.right;
                game.gameState.LaunchArrowServerRpc(arrowSpawn.position, direction, charge);
                charge = 0;
            }


            //game.gameState.OnTurnEndServerRpc();

        }
    }
}

