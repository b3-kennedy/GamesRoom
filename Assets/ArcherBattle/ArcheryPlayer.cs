using Unity.Netcode;
using UnityEngine;


namespace Assets.ArcherBattle
{
    public class ArcheryPlayer : NetworkBehaviour
    {

        [HideInInspector] public ArcherBattleGame game;


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
    }
}

