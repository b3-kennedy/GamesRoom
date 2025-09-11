using Unity.Netcode;
using UnityEngine;

namespace Assets.Football
{
    public class FootballMenuControl : NetworkBehaviour
    {

        public FootballGame footballGame;
        public bool isPlayer1;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (!IsOwner) return;

            if (!footballGame) return;

            if (footballGame.netGameState.Value == FootballGame.GameState.WAGER && isPlayer1)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    footballGame.wagerState.ChangeWagerAmountServerRpc(10);
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    footballGame.wagerState.ChangeWagerAmountServerRpc(-10);
                }

                if (Input.GetKeyDown(KeyCode.Return))
                {
                    footballGame.wagerState.ChangePlayer1LockedInStateServerRpc();
                }

                if (Input.GetKeyDown(KeyCode.X))
                {
                    footballGame.wagerState.ZeroWagerAmountServerRpc();
                    footballGame.ChangeStateServerRpc(FootballGame.GameState.GAME);
                }
            }

            if (footballGame.netGameState.Value == FootballGame.GameState.WAGER && !isPlayer1)
            {
                if (footballGame.wagerState.isPlayer1Locked.Value)
                {
                    if (Input.GetKeyDown(KeyCode.Y))
                    {
                        footballGame.ChangeStateServerRpc(FootballGame.GameState.GAME);
                    }

                    if (Input.GetKeyDown(KeyCode.N))
                    {
                        footballGame.wagerState.ChangePlayer1LockedInStateServerRpc();
                    }
                }
            }
        }
    }
}

