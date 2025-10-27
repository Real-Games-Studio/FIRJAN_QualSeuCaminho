using System.Collections;
using UnityEngine;

public class SessionPlacar : MonoBehaviour
{
    // Singleton instance
    public static SessionPlacar Instance { get; private set; }

    [Tooltip("Pontos atuais da sessão (visível no Inspector)")]
    public int pontos;

    public int TomadaDeDecisao;
    public int TomadaDeDecisaoMax = 15;
    public int PensamentoCritico;
    public int PensamentoCriticoMax = 15;
    public int SolucaoDeProblemas;
    public int SolucaoDeProblemasMax = 15;

    private Coroutine waitForLoaderRoutine;

    // Para cada resposta de 3 pontos:
    // +3 ponto em TomadaDeDecisao
    // +3 ponto em PensamentoCritico
    // +2 ponto em SolucaoDeProblemas


    // Para cada resposta de 2 pontos:
    // +2 ponto em TomadaDeDecisao
    // +1 ponto em PensamentoCritico
    // +1 ponto em SolucaoDeProblemas

    // Para cada resposta de 1 ou 0 pontos:
    // +0 ponto em TomadaDeDecisao
    // +0 ponto em PensamentoCritico
    // +0 ponto em SolucaoDeProblemas

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        TrySubscribeToConfig();
    }

    private void OnDestroy()
    {
        if (GameDataLoader.instance != null)
        {
            GameDataLoader.instance.OnGameDataUpdated -= OnGameDataUpdated;
        }

        if (waitForLoaderRoutine != null)
        {
            StopCoroutine(waitForLoaderRoutine);
            waitForLoaderRoutine = null;
        }

        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        ApplyConfigValues(GameDataLoader.instance?.loadedConfig);
    }

    private void TrySubscribeToConfig()
    {
        if (GameDataLoader.instance != null)
        {
            GameDataLoader.instance.OnGameDataUpdated -= OnGameDataUpdated;
            GameDataLoader.instance.OnGameDataUpdated += OnGameDataUpdated;
            ApplyConfigValues(GameDataLoader.instance.loadedConfig);
        }
        else if (waitForLoaderRoutine == null)
        {
            waitForLoaderRoutine = StartCoroutine(WaitForLoaderAndSubscribe());
        }
    }

    private IEnumerator WaitForLoaderAndSubscribe()
    {
        while (GameDataLoader.instance == null)
        {
            yield return null;
        }

        waitForLoaderRoutine = null;
        GameDataLoader.instance.OnGameDataUpdated -= OnGameDataUpdated;
        GameDataLoader.instance.OnGameDataUpdated += OnGameDataUpdated;
        ApplyConfigValues(GameDataLoader.instance.loadedConfig);
    }

    private void OnGameDataUpdated()
    {
        ApplyConfigValues(GameDataLoader.instance?.loadedConfig);
    }

    private void ApplyConfigValues(GameConfig config)
    {
        if (config == null) return;

        TomadaDeDecisaoMax = config.tomadaDeDecisaoMax;
        PensamentoCriticoMax = config.pensamentoCriticoMax;
        SolucaoDeProblemasMax = config.solucaoDeProblemasMax;

        TomadaDeDecisao = Mathf.Clamp(TomadaDeDecisao, 0, TomadaDeDecisaoMax);
        PensamentoCritico = Mathf.Clamp(PensamentoCritico, 0, PensamentoCriticoMax);
        SolucaoDeProblemas = Mathf.Clamp(SolucaoDeProblemas, 0, SolucaoDeProblemasMax);
    }

    // Instance methods
    public void AddPointsInstance(int value)
    {
        pontos += value;

        // Distribute competency points according to the rules:
        // For answers worth 3 points:
        //   +3 TomadaDeDecisao
        //   +3 PensamentoCritico
        //   +2 SolucaoDeProblemas
        // For answers worth 2 points:
        //   +2 TomadaDeDecisao
        //   +1 PensamentoCritico
        //   +1 SolucaoDeProblemas
        // For answers worth 1 or 0 points: no competency points

        if (value >= 3)
        {
            TomadaDeDecisao += 3;
            PensamentoCritico += 3;
            SolucaoDeProblemas += 3; // era 2
        }
        else if (value == 2)
        {
            TomadaDeDecisao += 1; // era 2
            PensamentoCritico += 1;
            SolucaoDeProblemas += 1;
        }
        else
        {
            // 1 or 0: no added competency points
        }

        Debug.Log("Pontos adicionados: " + value + ". Total agora: " + pontos);
    }

    public void ResetInstance()
    {
        pontos = 0;
        TomadaDeDecisao = 0;
        PensamentoCritico = 0;
        SolucaoDeProblemas = 0;
        Debug.Log("Placar reiniciado.");
    }

    // Static convenience methods to preserve existing call sites
    public static void AddPoints(int value)
    {
        EnsureInstanceExists();
        Instance.AddPointsInstance(value);
    }

    public static void Reset()
    {
        EnsureInstanceExists();
        Instance.ResetInstance();
    }

    private static void EnsureInstanceExists()
    {
        if (Instance == null)
        {
            var go = new GameObject("SessionPlacar");
            Instance = go.AddComponent<SessionPlacar>();
            DontDestroyOnLoad(go);
        }
    }
}
