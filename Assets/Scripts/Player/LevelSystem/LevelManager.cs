using UnityEngine;
using UnityEngine.UI;

namespace LevelSystem
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Upgrades")]

        [SerializeField] private float SpeedUpgradeRatio;
        [SerializeField] private float WeightEnduranceUpgradeRatio;

        [SerializeField] private float MinGainExp;
        [SerializeField] private float MaxGainExp;

        // Constants (read-only)
        private const int MaxLevel = 10;

        // Static Constructor
        public static LevelManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        private void LevelUp(){
            PlayerStats.Instance.OnLevelUp();
        }

        public void GainExp()
        {
            float exp = Random.Range(MinGainExp, MaxGainExp);
            PlayerStats.Instance.ExperiencePoints += exp;

            if (PlayerStats.Instance.IsMaxExp())
                LevelUp();
        }

//        private void OnPlayerOilAbsorb()
//        {
//            GainExp();
//        }
    }
}

