using UnityEngine;

public class SessionPlacar : MonoBehaviour
{
    // Singleton instance
    public static SessionPlacar Instance { get; private set; }

    [Tooltip("Pontos atuais da sessão (visível no Inspector)")]
    public int pontos;

    public int TomadaDeDecisao;
    public int TomadaDeDecisaoMax = 27;
    public int PensamentoCritico;
    public int PensamentoCriticoMax = 27;
    public int SolucaoDeProblemas;
    public int SolucaoDeProblemasMax = 18;



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
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
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
            SolucaoDeProblemas += 2;
        }
        else if (value == 2)
        {
            TomadaDeDecisao += 2;
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
