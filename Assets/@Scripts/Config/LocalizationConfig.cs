using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Define;
using static Unity.VisualScripting.Icons;


[System.Serializable]
public class FontAssetData
{
    public ELanguage language;
    public TMP_FontAsset fontAsset;
}

[CreateAssetMenu(fileName = "LocalizationConfig", menuName = "Config/LocalizationConfig")]
public class LocalizationConfig : ScriptableObject
{
    [Header("Font Asset Settings")]
    [SerializeField]
    private List<FontAssetData> _fontAssets = new List<FontAssetData>
    {
        //fontAsset은 drag&drop으로 넣을 것
        new FontAssetData { language = ELanguage.KOR, fontAsset = null },
        new FontAssetData { language = ELanguage.ENG, fontAsset = null }
    };

    public TMP_FontAsset GetFontAsset(ELanguage language)
    {
        foreach (FontAssetData data in _fontAssets)
        {
            if (data.language == language)
                return data.fontAsset;
        }

        Debug.LogWarning($"LocalizationConfig: Font asset not found for language: {language}");
        return null;
    }
}