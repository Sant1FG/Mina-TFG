using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Scriptable Objects/GameConfig")]
public class GameConfig : ScriptableObject
{
    public int initialTimeSeconds = 180;
    public int pointsPerCoalUnit = 10;
    public int depositMax = 3;
    public float timePerCoalUnit = 2f;
}
