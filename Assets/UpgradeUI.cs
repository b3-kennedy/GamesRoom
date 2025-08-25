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


        void Start()
        {
            image = GetComponent<Image>();
            isSelected.OnValueChanged += OnValueChanged;
            titleText.text = $"{upgrade.upgradeName}:";
            descriptionText.text = upgrade.upgradeDescription;
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

