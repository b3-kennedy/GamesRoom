using Assets.Farkle;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Farkle
{
    public class FarklePlayer : NetworkBehaviour
    {

        public FarkleGame farkleGame;
        public NetworkVariable<bool> isTurn;
        public GameObject dice;

        public bool isPlayer1;

        Wager wagerState;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (farkleGame.wagerState is Wager wager)
            {
                wagerState = wager;
            }
        }

        // Update is called once per frame
        void Update()
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
            if (farkleGame.netGameState.Value == FarkleGame.GameState.WAGER && isPlayer1)
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
    }
}

