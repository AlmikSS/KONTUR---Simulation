using UnityEngine;

namespace _KONTUR___Simulation._Scripts.GamePlay.ProcedualAnims
{
    public interface IEnvironmentSensor
    {
        SensorResult Evaluate();
    }
    
    public struct SensorResult
    {
        public bool IsValid;
        public float Score;
        public Vector3 Position;
        public Quaternion Rotation;
    }
}