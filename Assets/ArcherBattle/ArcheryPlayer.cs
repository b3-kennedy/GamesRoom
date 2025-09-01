using System.Threading;
using Unity.Netcode;
using UnityEngine;


namespace Assets.ArcherBattle
{
    public class ArcheryPlayer : NetworkBehaviour
    {

        [HideInInspector] public ArcherBattleGame game;
        public GameObject playerObject;
        public Transform rotater;

        public float rotateSpeed = 5f;

        void Start()
        {

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

        }
    }
}

