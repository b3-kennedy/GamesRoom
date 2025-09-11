using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class RhythmPlayer : NetworkBehaviour
{

    public bool inZone1;
    public bool inZone2;
    public bool inZone3;

    [HideInInspector] public GameObject leftTarget;
    [HideInInspector] public GameObject middleTarget;
    [HideInInspector] public GameObject rightTarget;

    public GameObject leftLane;
    public GameObject middleLane;
    public GameObject rightLane;

    bool leftCooldown;
    bool middleCooldown;
    bool rightCooldown;

    public bool canLoseLife = true;

    public RythmDuel duel;

    [HideInInspector] public bool isLeftPlayer = false;
    [HideInInspector] public bool canInput = false;



    public override void OnGainedOwnership()
    {
        Debug.Log($"{OwnerClientId} gained ownership of this object");
    }

    public override void OnLostOwnership()
    {
        Debug.Log($"{OwnerClientId} lost ownership of this object");
    }

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Alpha1) && !leftCooldown)
        {
            if (inZone1)
            {
                leftTarget.SetActive(false);
                DestroyTargetServerRpc(leftTarget.GetComponent<NetworkObject>().NetworkObjectId);
            }
            else
            {
                leftCooldown = true; // start cooldown
                MeshRenderer rend = leftLane.GetComponent<MeshRenderer>();
                rend.material.color = Color.red;
                StartCoroutine(LerpColorWithCooldown(rend, Color.black, 0.5f, () => leftCooldown = false));
                ShowLaneCooldownServerRpc(NetworkManager.Singleton.LocalClientId, 0);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && !middleCooldown)
        {
            if (inZone2)
            {
                middleTarget.SetActive(false);
                DestroyTargetServerRpc(middleTarget.GetComponent<NetworkObject>().NetworkObjectId);
            }
            else
            {
                middleCooldown = true;
                MeshRenderer rend = middleLane.GetComponent<MeshRenderer>();
                rend.material.color = Color.red;
                StartCoroutine(LerpColorWithCooldown(rend, Color.black, 0.5f, () => middleCooldown = false));
                ShowLaneCooldownServerRpc(NetworkManager.Singleton.LocalClientId, 1);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && !rightCooldown)
        {
            if (inZone3)
            {
                rightTarget.SetActive(false);
                DestroyTargetServerRpc(rightTarget.GetComponent<NetworkObject>().NetworkObjectId);
            }
            else
            {
                rightCooldown = true;
                MeshRenderer rend = rightLane.GetComponent<MeshRenderer>();
                rend.material.color = Color.red;
                StartCoroutine(LerpColorWithCooldown(rend, Color.black, 0.5f, () => rightCooldown = false));
                ShowLaneCooldownServerRpc(NetworkManager.Singleton.LocalClientId, 2);
            }
        }

        if (duel.netGameState.Value == RythmDuel.GameState.GAME_OVER)
        {

            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("reset");
                duel.ResetServerRpc();
            }
        }
        else if (duel.netGameState.Value == RythmDuel.GameState.WAGER)
        {

            if (isLeftPlayer)
            {
                if (Input.GetKeyDown(KeyCode.X))
                {
                    duel.ChangeStateServerRpc(RythmDuel.GameState.GAME);
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    ChangeWagerAmountServerRpc(-10);
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    ChangeWagerAmountServerRpc(10);
                }

                if (Input.GetKeyDown(KeyCode.Return))
                {
                    ChangeLockValueLeftPlayerServerRpc(true);
                }
            }
            else
            {
                if (!canInput) return;

                if (Input.GetKeyDown(KeyCode.Y))
                {
                    duel.ChangeStateServerRpc(RythmDuel.GameState.GAME);
                }

                if (Input.GetKeyDown(KeyCode.N))
                {
                    ChangeLockValueLeftPlayerServerRpc(false);
                }
            }


        }

    }


    [ServerRpc(RequireOwnership = false)]
    void ChangeLockValueLeftPlayerServerRpc(bool value)
    {
        ChangeLockValueLeftPlayerClientRpc(value);
    }

    [ClientRpc]
    void ChangeLockValueLeftPlayerClientRpc(bool value)
    {
        duel.isLeftPlayerLocked = value;
    }

    [ServerRpc(RequireOwnership = false)]
    void ChangeWagerAmountServerRpc(int amount)
    {
        int player1Credits = duel.connectedPlayers[0].GetComponent<SteamPlayer>().credits.Value;
        int player2Credits = duel.connectedPlayers[1].GetComponent<SteamPlayer>().credits.Value;

        int maxWager = Mathf.Min(player1Credits, player2Credits);

        duel.wagerAmount.Value += amount;

        duel.wagerAmount.Value = Mathf.Clamp(duel.wagerAmount.Value, 0, maxWager);
    }

    public void LostLife()
    {
        leftLane.GetComponent<MeshRenderer>().material.color = Color.white;
        middleLane.GetComponent<MeshRenderer>().material.color = Color.white;
        rightLane.GetComponent<MeshRenderer>().material.color = Color.white;
        StartCoroutine(LerpColorWithCooldown(leftLane.GetComponent<MeshRenderer>(), Color.black, 1f, () => canLoseLife = true));
        StartCoroutine(LerpColorWithCooldown(middleLane.GetComponent<MeshRenderer>(), Color.black, 1f, () => canLoseLife = true));
        StartCoroutine(LerpColorWithCooldown(rightLane.GetComponent<MeshRenderer>(), Color.black, 1f, () => canLoseLife = true));
    }

    [ServerRpc(RequireOwnership = false)]
    void ShowLaneCooldownServerRpc(ulong clientID, int laneNumber)
    {
        ShowLaneCooldownClientRpc(clientID, laneNumber);
    }

    [ClientRpc]
    void ShowLaneCooldownClientRpc(ulong clientID, int laneNumber)
    {
        if (NetworkManager.Singleton.LocalClientId != clientID)
        {
            if (laneNumber == 0)
            {
                leftCooldown = true;
                MeshRenderer rend = leftLane.GetComponent<MeshRenderer>();
                rend.material.color = Color.red;
                StartCoroutine(LerpColorWithCooldown(rend, Color.black, 0.5f, () => leftCooldown = false));
            }
            else if (laneNumber == 1)
            {
                middleCooldown = true;
                MeshRenderer rend = middleLane.GetComponent<MeshRenderer>();
                rend.material.color = Color.red;
                StartCoroutine(LerpColorWithCooldown(rend, Color.black, 0.5f, () => middleCooldown = false));
            }
            else
            {
                rightCooldown = true;
                MeshRenderer rend = rightLane.GetComponent<MeshRenderer>();
                rend.material.color = Color.red;
                StartCoroutine(LerpColorWithCooldown(rend, Color.black, 0.5f, () => rightCooldown = false));
            }

        }
    }


    IEnumerator LerpColorWithCooldown(MeshRenderer rend, Color targetColor, float duration, System.Action onComplete)
    {
        Color startColor = rend.material.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rend.material.color = Color.Lerp(startColor, targetColor, elapsed / duration);
            yield return null;
        }

        rend.material.color = targetColor;
        onComplete?.Invoke(); // reset cooldown
    }

    void OnEnable()
    {
        StartCoroutine(LerpColorWithCooldown(middleLane.GetComponent<MeshRenderer>(), Color.black, 0.5f, () => middleCooldown = false));
        StartCoroutine(LerpColorWithCooldown(leftLane.GetComponent<MeshRenderer>(), Color.black, 0.5f, () => middleCooldown = false));
        StartCoroutine(LerpColorWithCooldown(rightLane.GetComponent<MeshRenderer>(), Color.black, 0.5f, () => middleCooldown = false));
    }

    [ServerRpc(RequireOwnership = false)]
    void DestroyTargetServerRpc(ulong netObjID)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObjID, out var target))
        {
            target.Despawn(true);
        }
    }
}
