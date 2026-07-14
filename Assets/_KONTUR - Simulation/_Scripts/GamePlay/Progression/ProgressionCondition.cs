using System;
using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.Progression
{
    [CreateAssetMenu(menuName = "Progression/Condition")]
    public sealed class ProgressionCondition : ScriptableObject
    {
        [field: SerializeField] public string Key;
        [SerializeField] private int _value;
        [SerializeField] private ComparisonType _comparison;
        
        public bool IsSatisfied(ProgressionService progressionService)
        {
            var currentValue = progressionService.GetState(Key);
            return _comparison switch
            {
                ComparisonType.Equal => currentValue == _value,
                ComparisonType.NotEqual => currentValue != _value,
                ComparisonType.Less => currentValue < _value,
                ComparisonType.LessOrEqual => currentValue <= _value,
                ComparisonType.Greater => currentValue > _value,
                ComparisonType.GreaterOrEqual => currentValue >= _value,
                _ => false
            };
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