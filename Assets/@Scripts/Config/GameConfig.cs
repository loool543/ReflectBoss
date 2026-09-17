using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Config/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Game Settings")]
    [SerializeField] private int initialHP = 100;

    public int InitialHP => initialHP;
}
