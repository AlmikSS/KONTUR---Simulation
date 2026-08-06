using _KONTUR___Simulation._Scripts.GamePlay.UI.Laboratory;

public sealed class SceneState
{
    public DeathPayload Death { get; set; }
    public int CurrentLevel { get; set; } = 1;
    public DayConfig DayConfig { get; set; }

    public void Clear()
    {
        Death = null;
        CurrentLevel = 1;
    }
}

public sealed class DeathPayload
{
    public PlayerDiedFromType Reason { get; }
    public string Message { get; }

    public DeathPayload(PlayerDiedFromType reason, string message = null)
    {
        Reason = reason;
        Message = message;
    }
}