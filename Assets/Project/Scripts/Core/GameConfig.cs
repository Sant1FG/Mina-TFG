using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Scriptable Objects/GameConfig")]
///<summary>
/// Configurable parameters for the game.
/// Stored as Scriptable object so they can be edited in the Inspector.
///</summary>
public class GameConfig : ScriptableObject
{
    public int initialTimeSeconds = 150;
    public int pointsPerCoalUnit = 10;
    public int depositMax = 20;
    public float timePerCoalUnit = 2f;
}
