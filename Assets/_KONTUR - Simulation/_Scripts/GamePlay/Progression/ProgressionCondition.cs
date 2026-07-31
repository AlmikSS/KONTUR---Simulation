using System;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.Progression
{
    [CreateAssetMenu(menuName = "Progression/Condition")]
    public sealed class ProgressionCondition : ScriptableObject
    {
        [field: SerializeField] public string Key { get; private set; }
        [field: SerializeField] public int RequiredValue { get; private set; } // Добавь это поле
        [SerializeField] private ComparisonType _comparison;
        
        public bool IsSatisfied(ProgressionService progressionService)
        {
            var currentValue = progressionService.GetState(Key);
            return _comparison switch
            {
                ComparisonType.Equal => currentValue == RequiredValue,
                ComparisonType.NotEqual => currentValue != RequiredValue,
                ComparisonType.Less => currentValue < RequiredValue,
                ComparisonType.LessOrEqual => currentValue <= RequiredValue,
                ComparisonType.Greater => currentValue > RequiredValue,
                ComparisonType.GreaterOrEqual => currentValue >= RequiredValue,
                _ => false
            };
        }
        
        public int GetRequiredValue()
        {
            return RequiredValue;
        }
    }

    public enum ComparisonType
    {
        Equal,
        NotEqual,
        Less,
        LessOrEqual,
        Greater,
        GreaterOrEqual
    }
}