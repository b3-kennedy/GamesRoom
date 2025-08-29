using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;



namespace Assets.CreditClicker
{
    public class UpgradeUI : NetworkBehaviour
    {
        Image image;

        public NetworkVariable<bool> isSelected;

        public Upgrade upgrade;

        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI costText;

        [HideInInspector] public int cost;

        [HideInInspector] public Transform layout;
        public GameObject tierBlip;

        public int currentTier = 0;


        void Start()
        {
            image = GetComponent<Image>();
            isSelected.OnValueChanged += OnValueChanged;
            titleText.text = $"{upgrade.upgradeName}:";
            descriptionText.text = upgrade.upgradeDescription;
            costText.text = $"${upgrade.cost}";
            cost = upgrade.cost;
            layout = transform.GetChild(0).GetChild(3);
            for (int i = 0; i < upgrade.maxTiers; i++)
            {
                Instantiate(tierBlip, layout);
            }

        }

        [ServerRpc(RequireOwnership = false)]
        public void UpgradeCostServerRpc(int newCost)
        {
            UpgradeCostClientRpc(newCost);
        }

        [ClientRpc]
        void UpgradeCostClientRpc(int newCost)
        {
            if (currentTier >= upgrade.maxTiers)
            {
                costText.text = $"MAX";
                cost = newCost;
                return;
            }
            costText.text = $"${newCost}";
            cost = newCost;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SelectServerRpc()
        {
            isSelected.Value = true;
        }

        [ServerRpc(RequireOwnership = false)]
        public void DeselectServerRpc()
        {
            isSelected.Value = false;
        }

        private void OnValueChanged(bool previousValue, bool newValue)
        {
            if (newValue)
            {
                image.color = Color.white;
            }
            else
            {
                image.color = Color.black;
            }
        }
    }
}

