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
        StartCoroutine(GenerateStory(c, map));
    }

    IEnumerator LoadMap(string url)
    {
        UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);
        yield return req.SendWebRequest();
        Texture tex = DownloadHandlerTexture.GetContent(req);
        worldScreen.material.mainTexture = tex;
    }

    IEnumerator GenerateStory(Character c, Map m)
    {
        storyText.text = "Generating story...";
        // will call chat
       string prompt = $"Write a short spooky Identity V style scene. {c.characterName} is a {c.role} in {m.name}.";
        string json = "{\"model\":\"gpt-3.5-turbo\",\"messages\":[{\"role\":\"user\",\"content\":\"" + prompt + "\"}]}";

        UnityWebRequest req = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + openAIKey);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string response = req.downloadHandler.text;

            string marker = "\"content\": \"";
            int contentIndex = response.IndexOf(marker);
            if (contentIndex != -1)
            {
                contentIndex += marker.Length;

                int endIndex = contentIndex;
                while (endIndex < response.Length)
                {
                    if (response[endIndex] == '\\')
                    {
                        endIndex += 2;
                        continue;
                    }
                    if (response[endIndex] == '"')
                        break;
                    endIndex++;
                }

                if (endIndex < response.Length)
                {
                    string story = response.Substring(contentIndex, endIndex - contentIndex);
                    story = story.Replace("\\n", "\n").Replace("\\\"", "\"");
                    storyText.text = story;
                    yield break;
                }
            }

            storyText.text = "Parsing failed. Raw:\n" + response;
        }
        else
        {
            storyText.text = "Error: " + req.error + "\n" + req.downloadHandler.text;
        }
    }

}