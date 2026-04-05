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
    [Header("References")]
    public Renderer worldScreen; 
    public TMP_Text nameText;
    public TMP_Text roleText;
    public TMP_Text traitsText;
    public TMP_Text storyText;

    [Header("OpenAI Key")]
    public string openAIKey = "KEY";

    [Header("Characters")]
    public Character[] characters = new Character[]
    {
        new Character{ characterName="Ada", role="Survivor", traits="Psycologist"},
        new Character{ characterName="Victor", role="Survivor", traits="Postman"},
        new Character{ characterName="Jack", role="Hunter", traits="The Ripper"}
    };

    [Header("Maps")]
    public Map[] maps = new Map[]
    {
        new Map{ name="Arms Factory", imageUrl="https://static.wikia.nocookie.net/id5/images/f/f5/D69d6822-c9a9-45c5-a976-b084d95a8fb4.jpg/revision/latest/smart/width/40/height/30?cb=20180613004334"},
        new Map{ name="China Town", imageUrl="https://static.wikia.nocookie.net/id5/images/6/66/Chinatown1.jpg/revision/latest/smart/width/40/height/30?cb=20240417014622"},
        new Map{ name="Darkwoods", imageUrl="https://static.wikia.nocookie.net/id5/images/8/85/Darkwoods2.jpg/revision/latest/smart/width/40/height/30?cb=20210711212827"}
    };

    public void Reincarnate()
    {
        // RANdom Map
        Map map = maps[Random.Range(0, maps.Length)];
        StartCoroutine(LoadMap(map.imageUrl));

        // Random Char
        Character c = characters[Random.Range(0, characters.Length)];
        nameText.text = c.characterName;
        roleText.text = c.role;
        traitsText.text = c.traits;

        // generate a story later
    }

    IEnumerator LoadMap(string url)
    {
        UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);
        yield return req.SendWebRequest();
        Texture tex = DownloadHandlerTexture.GetContent(req);
        worldScreen.material.mainTexture = tex;
    }

}