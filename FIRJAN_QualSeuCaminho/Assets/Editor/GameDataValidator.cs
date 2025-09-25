using UnityEditor;
using UnityEngine;
using System.IO;

public class GameDataValidator
{
    [MenuItem("Tools/GameData/Validate gamedata.json")]
    public static void Validate()
    {
        string path = Path.Combine(Application.dataPath, "StreamingAssets/gamedata.json");
        if (!File.Exists(path))
        {
            Debug.LogError("gamedata.json not found at " + path);
            return;
        }
        string json = File.ReadAllText(path);
        try
        {
            var data = JsonUtility.FromJson<GameData>(json);
            if (data == null)
            {
                Debug.LogError("gamedata.json parsed to null GameData");
                return;
            }
            Debug.Log($"Gamedata loaded, Questions: {data.questions?.Count ?? 0}");
            if (data.questions != null)
            {
                foreach (var q in data.questions)
                {
                    Debug.Log($"{q.id}: {q.title} -- {q.description} (answers: {q.answers?.Count ?? 0})");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to parse gamedata.json: " + ex.Message);
        }
    }
}
