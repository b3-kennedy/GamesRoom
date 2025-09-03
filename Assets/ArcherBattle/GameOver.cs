using TMPro;
using Unity.Netcode;
using UnityEngine;


namespace Assets.ArcherBattle
{
    public class GameOver : State
    {

        ArcherBattleGame archerBattleGame;

        public NetworkVariable<string> winnerName;

        public GameObject cam;

        public TextMeshPro winnerText;

        public GameObject floor;


        void Start()
        {
            if (game is ArcherBattleGame g)
            {
                archerBattleGame = g;
            }
        }

        public override void OnStateEnter()
        {
            gameObject.SetActive(true);
            winnerText.text = $"{winnerName} has Won!";
            cam.GetComponent<CameraFollow>().isFollow = false;
            cam.transform.localPosition = new Vector3(0, 0, cam.transform.localPosition.z);
            floor.SetActive(false);
        }

        public override void OnStateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                archerBattleGame.ResetServerRpc();
            }
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetWinnerServerRpc(string playerName)
        {
            winnerName.Value = playerName;
        }

    }
}

