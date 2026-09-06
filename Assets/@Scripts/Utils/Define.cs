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

    }

    public enum ESound
    {
        Bgm,
        Effect,
       
        MaxCount
    }
    public enum ELanguage
    {
        KOR,
        ENG
    }

}
