namespace _KONTUR___Simulation._Scripts.GamePlay.NPC
{
    public sealed class StateMachine
    {
        private IState _currentState;

        public void Update()
        {
            _currentState?.Update();
        }
        
        public void ChangeState(IState newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            _currentState?.Enter();
        }
    }
}