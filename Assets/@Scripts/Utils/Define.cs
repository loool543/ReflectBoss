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
