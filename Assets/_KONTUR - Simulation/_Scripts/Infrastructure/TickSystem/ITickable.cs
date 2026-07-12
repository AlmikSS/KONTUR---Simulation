namespace KofeyekToolkit.TickSystem
{
    public interface ITickable
    {
        TickPhase Phase { get; }
        void Tick(float deltaTime);
    }
}