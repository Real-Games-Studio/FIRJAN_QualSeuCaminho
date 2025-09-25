using System.Collections;
using System.IO;
using UnityEngine;

public class GameDataLoader : MonoBehaviour
{
    public static GameDataLoader instance;

    [Tooltip("Relative filename under StreamingAssets (e.g. gamedata.json)")]
    public string fileName = "gamedata.json";

    public GameData loadedData;
    public GameConfig loadedConfig;

    private void Start()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        // Start initial load and listen for runtime language changes
        StartCoroutine(Load());
        // If LocalizationManager isn't initialized yet, wait and subscribe when available
        StartCoroutine(WaitForLocalizationAndSubscribe());
    }

    private IEnumerator WaitForLocalizationAndSubscribe()
    {
        while (LocalizationManager.instance == null)
        {
            yield return null;
        }
        LocalizationManager.instance.OnLanguageChanged += OnLanguageChanged_ReloadTranslations;
    }

    private void OnDestroy()
    {
        if (LocalizationManager.instance != null)
        {
            LocalizationManager.instance.OnLanguageChanged -= OnLanguageChanged_ReloadTranslations;
        }
    }

    public IEnumerator Load()
    {
        // Load base game data (non-translatable rules/ids/values)
        string basePath = Path.Combine(Application.streamingAssetsPath, "gamebase.json");
        GameData baseData = null;
        if (File.Exists(basePath))
        {
            string baseJson = File.ReadAllText(basePath);
            if (!string.IsNullOrEmpty(baseJson))
            {
                baseData = JsonUtility.FromJson<GameData>(baseJson);
            }
        }

        // Load game config first (shared settings)
        string configPath = Path.Combine(Application.streamingAssetsPath, "gameconfig.json");
        string configJson = null;
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var cfgReq = UnityEngine.Networking.UnityWebRequest.Get(configPath))
        {
            yield return cfgReq.SendWebRequest();
            if (cfgReq.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                configJson = cfgReq.downloadHandler.text;
            }
            else
            {
                Debug.LogWarning("Could not load gameconfig.json: " + cfgReq.error);
            }
        }
#else
        if (File.Exists(configPath))
        {
            configJson = File.ReadAllText(configPath);
        }
        else
        {
            Debug.LogWarning("gameconfig.json not found at: " + configPath);
        }
#endif
        if (!string.IsNullOrEmpty(configJson))
        {
            loadedConfig = JsonUtility.FromJson<GameConfig>(configJson);
        }

        // Try language-specific files first (gamedata.{lang}.json), then fallback to default fileName and gamedata.pt.json
        string lang = PlayerPrefs.GetString("lang", "pt");
        string baseName = Path.GetFileNameWithoutExtension(fileName);
        string ext = Path.GetExtension(fileName);
        string langFileName = baseName + "." + lang + ext; // e.g. gamedata.pt.json
        string[] candidates = new string[] { langFileName, fileName, baseName + ".pt" + ext };

        string json = null;
        foreach (var candidate in candidates)
        {
            string path = Path.Combine(Application.streamingAssetsPath, candidate);
#if UNITY_ANDROID && !UNITY_EDITOR
            using (var www = UnityEngine.Networking.UnityWebRequest.Get(path))
            {
                yield return www.SendWebRequest();
                if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"Could not load {candidate}: {www.error}");
                    continue;
                }
                json = www.downloadHandler.text;
            }
#else
            if (!File.Exists(path))
            {
                Debug.LogWarning($"Gamedata file not found at: {path}");
                continue;
            }
            json = File.ReadAllText(path);
#endif
            if (!string.IsNullOrEmpty(json))
            {
                loadedData = JsonUtility.FromJson<GameData>(json);
                if (loadedData != null) break;
            }
            yield return null;
        }

        if (loadedData == null)
        {
            // If no localized gamedata was found, but we have a baseData, use it
            if (baseData != null)
            {
                loadedData = baseData;
            }
            else
            {
                Debug.LogError("Failed to load any gamedata.json candidate.");
                yield break;
            }
        }
        // If we loaded a baseData and a language-specific gamedata, merge translations
        if (baseData != null && loadedData != null && !ReferenceEquals(baseData, loadedData))
        {
            MergeTranslations(baseData, loadedData);
            loadedData = baseData; // keep base structure, but with translated strings applied
        }

        // We use 'casas' exclusively now. Any logic that previously read 'points' should be updated to use 'casas'.

        Debug.Log("Gamedata loaded. Questions: " + (loadedData?.questions?.Count ?? 0));
    }

    private void OnLanguageChanged_ReloadTranslations()
    {
        // Reload translations when language changes at runtime
        StartCoroutine(ReloadTranslationsCoroutine());
    }

    private IEnumerator ReloadTranslationsCoroutine()
    {
        // Load base and localized translation again using same logic but only merge translations
        string basePath = Path.Combine(Application.streamingAssetsPath, "gamebase.json");
        GameData baseData = null;
        if (File.Exists(basePath))
        {
            string baseJson = File.ReadAllText(basePath);
            baseData = JsonUtility.FromJson<GameData>(baseJson);
        }

        // Load localized gamedata for current language
        string lang = PlayerPrefs.GetString("lang", "pt");
        string baseName = Path.GetFileNameWithoutExtension(fileName);
        string ext = Path.GetExtension(fileName);
        string langFileName = baseName + "." + lang + ext;
        string[] candidates = new string[] { langFileName, fileName, baseName + ".pt" + ext };

        string json = null;
        GameData localized = null;
        foreach (var candidate in candidates)
        {
            string path = Path.Combine(Application.streamingAssetsPath, candidate);
            if (!File.Exists(path)) continue;
            json = File.ReadAllText(path);
            if (!string.IsNullOrEmpty(json))
            {
                localized = JsonUtility.FromJson<GameData>(json);
                if (localized != null) break;
            }
            yield return null;
        }

        if (localized != null)
        {
            if (baseData != null)
            {
                MergeTranslations(baseData, localized);
                loadedData = baseData;
            }
            else
            {
                // No base: replace loadedData directly with localized file
                loadedData = localized;
            }
            Debug.Log("Gamedata translations reloaded for language: " + lang);
        }
    }

    private void MergeTranslations(GameData baseData, GameData localized)
    {
        if (baseData?.questions == null || localized?.questions == null) return;

        // Build dictionary by id for localized questions if ids exist
        var locById = new System.Collections.Generic.Dictionary<string, Question>();
        foreach (var q in localized.questions)
        {
            if (!string.IsNullOrEmpty(q?.id)) locById[q.id] = q;
        }

        for (int i = 0; i < baseData.questions.Count; i++)
        {
            var baseQ = baseData.questions[i];
            Question locQ = null;
            if (!string.IsNullOrEmpty(baseQ?.id) && locById.TryGetValue(baseQ.id, out locQ))
            {
                // match by id
            }
            else if (i < localized.questions.Count)
            {
                // match by index as fallback
                locQ = localized.questions[i];
            }

            if (locQ != null)
            {
                // copy textual fields
                baseQ.title = string.IsNullOrEmpty(locQ.title) ? baseQ.title : locQ.title;
                baseQ.description = string.IsNullOrEmpty(locQ.description) ? baseQ.description : locQ.description;

                // merge answers
                if (baseQ.answers != null && locQ.answers != null)
                {
                    for (int a = 0; a < baseQ.answers.Count && a < locQ.answers.Count; a++)
                    {
                        var baseA = baseQ.answers[a];
                        var locA = locQ.answers[a];
                        baseA.text = string.IsNullOrEmpty(locA.text) ? baseA.text : locA.text;
                        baseA.feedback = string.IsNullOrEmpty(locA.feedback) ? baseA.feedback : locA.feedback;
                        // keep numeric fields like 'casas' in base
                    }
                }
            }
        }
    }
}
