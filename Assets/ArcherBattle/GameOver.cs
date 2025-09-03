using TMPro;
using UnityEngine;


namespace Assets.ArcherBattle
{
    public class GameOver : State
    {

        ArcherBattleGame archerBattleGame;

        string winnerName;

        public GameObject cam;

        public TextMeshPro winnerText;


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

        public void SetWinner(string playerName)
        {
            winnerName = playerName;
        }

    }
}

