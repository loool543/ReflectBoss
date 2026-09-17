public static class Define 
{
    public enum EScene
    {
        Unknown,
        LoadingScene,
        DevScene,
    }

    public enum EEventType
    {
        None,

        // Preserve existing event IDs after removing example events.
        HPChanged = 3,
        PlayerHit,
        ReflectSuccess,
        BossHit,
        BossHPChanged,
        BossStageChanged,
        GameStateChanged,

    }

    public enum EGameState { Playing, Success, Fail }

    public enum ESound
    {
        Bgm,
        Collision,
        Reflect,
        Success,
        Fail,
        MaxCount
    }

}
