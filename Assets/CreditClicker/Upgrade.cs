using UnityEngine;


namespace Assets.CreditClicker
{
    [CreateAssetMenu(fileName = "Upgrade", menuName = "CreditClicker/Upgrade")]
    public class Upgrade : ScriptableObject
    {
        public string upgradeName;
        public string upgradeDescription;
        public int cost;
        public enum UpgradeType { ACTIVE, PASSIVE, CLICK_SPEED };
        public UpgradeType upgradeType;
        public float value;
        public int tier;

        public int maxTiers;
        public float costIncreasePerTier;

    }
}

