public sealed class SceneState
{
    public DeathPayload Death { get; set; }

    public void Clear()
    {
        Death = null;
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