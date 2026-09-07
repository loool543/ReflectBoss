using UnityEngine;

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
        GoldChanged,
        LanguageChanged,
        HPChanged,
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
    public enum ELanguage
    {
        KOR,
        ENG
    }

}
