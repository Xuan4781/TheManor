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
}

[System.Serializable]
public class GameData
{
    public Character[] characters;
    public Map[] maps;
}

public class GameController : MonoBehaviour
{
    [Header("References")]
    public TMP_Text nameText;
    public TMP_Text roleText;
    public TMP_Text traitsText;
    public TMP_Text jokeText;

    [Header("OpenAI Key")]
    public string openAIKey = "";

    private Character[] characters;
    private Map[] maps;

    void Start()
    {
        LoadGameData();
    }

    void LoadGameData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("gamedata");
        if (jsonFile != null)
        {
            GameData data = JsonUtility.FromJson<GameData>(jsonFile.text);
            characters = data.characters;
            maps = data.maps;
            Debug.Log("Game data loaded! Characters: " + characters.Length + " Maps: " + maps.Length);
        }
        else
        {
            Debug.LogError("gamedata.json not found in Resources folder!");
        }
    }

    public void Reincarnate()
    {
        if (characters == null || maps == null)
        {
            Debug.LogError("Data not loaded yet!");
            return;
        }

        Map map = maps[Random.Range(0, maps.Length)];

        Character c = characters[Random.Range(0, characters.Length)];
        nameText.text = c.characterName;
        roleText.text = c.role;
        traitsText.text = c.traits;

        StartCoroutine(GenerateJoke(c, map));
    }

    IEnumerator GenerateJoke(Character c, Map m)
    {
        jokeText.text = "...";
        string prompt = $"Write one single short funny in-character joke that {c.characterName}, a {c.role} with the trait '{c.traits}', would say while on the map '{m.name}' in Identity V. Just the joke, no explanation.";
        yield return StartCoroutine(CallOpenAI(prompt, jokeText));
    }

    IEnumerator CallOpenAI(string prompt, TMP_Text targetText)
    {
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
                    if (response[endIndex] == '\\') { endIndex += 2; continue; }
                    if (response[endIndex] == '"') break;
                    endIndex++;
                }

                if (endIndex < response.Length)
                {
                    string result = response.Substring(contentIndex, endIndex - contentIndex);
                    targetText.text = result.Replace("\\n", "\n").Replace("\\\"", "\"");
                    yield break;
                }
            }

            targetText.text = "Parsing failed.";
        }
        else
        {
            targetText.text = "Error: " + req.error;
        }
    }
}