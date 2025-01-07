using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int requestId = 0;
    public string response = null;
    public string request = null;
    public MoveInfo moveInfo = null;
    public Player Color;
    public Position boardPos = new Position(0, 0);
    public string message = null;
}