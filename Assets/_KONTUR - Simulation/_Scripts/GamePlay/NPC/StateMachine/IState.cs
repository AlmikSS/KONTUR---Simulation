namespace _KONTUR___Simulation._Scripts.GamePlay.NPC
{
    public interface IState
    {
        string Name { get; }
        
        void Enter();
        void Update(float deltaTime);
        void Exit();
    }
}