using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;

[System.Serializable]
public class Character
{
    public string characterName;
    public string role;
    public string traits;
}

[System.Serializable]
public class Map
{
    public string name;
    public string imageUrl;
}

public class GameController : MonoBehaviour
{
    
}