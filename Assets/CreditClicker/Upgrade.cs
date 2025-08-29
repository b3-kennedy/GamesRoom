using UnityEngine;


namespace Assets.CreditClicker
{
    [CreateAssetMenu(fileName = "Upgrade", menuName = "CreditClicker/Upgrade")]
    public class Upgrade : ScriptableObject
    {
        public string upgradeName;
        public string upgradeDescription;
        public int cost;
        public enum UpgradeType
        {
            ACTIVE, PASSIVE, CLICK_SPEED, PASSIVE_INCREASE,
            PASSIVE_MONEY_INCREASE, DOUBLE_CREDIT_CHANCE, INTEREST, BACKGROUND_FLASH,
            PLAYER_COUNT, TIME, ACTIVE_PASSIVE, NO_CAP
        };
        public UpgradeType upgradeType;
        public float value;
        public int tier;

        public int maxTiers;
        public float costIncreasePerTier;

    }
}

