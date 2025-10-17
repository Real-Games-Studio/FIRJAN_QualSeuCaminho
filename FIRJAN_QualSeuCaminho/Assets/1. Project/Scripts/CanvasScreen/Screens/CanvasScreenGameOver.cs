using UnityEngine;
using TMPro;
using UnityEngine.UI;
using _4._NFC_Firjan.Scripts.NFC;
using _4._NFC_Firjan.Scripts.Server;
using System.Net;
using System.Threading.Tasks;

public class CanvasScreenGameOver : CanvasScreen
{
    [SerializeField] private TMP_Text decisionMakingText;
    [SerializeField] private TMP_Text criticalThinkingText;
    [SerializeField] private TMP_Text problemSolvingText;

    [Header("Barras Segmentadas")]
    [SerializeField] private SegmentedBarNew decisionMakingBar;
    [SerializeField] private SegmentedBarNew criticalThinkingBar;
    [SerializeField] private SegmentedBarNew problemSolvingBar;

    [SerializeField] private Image nfcConnectionFeedback; // cor amarela para nenhum nfc detectado.
                                                          // cor verde para nfc detectado e dados enviados

    // optional reference to the server communicator in scene (can also be found at runtime)
    [SerializeField] private ServerComunication serverComunication;

    private NFCReceiver _nfcReceiver;

    public override void TurnOff()
    {
        if (_nfcReceiver != null)
        {
            _nfcReceiver.OnNFCConnected.RemoveListener(OnNfcConnected);
            _nfcReceiver.OnNFCDisconnected.RemoveListener(OnNfcDisconnected);
        }

        base.TurnOff();
    }


    // Helper to support multiple Unity versions: prefer FindFirstObjectByType/FindAnyObjectByType when available
    private T FindFirstOrAny<T>() where T : UnityEngine.Object
    {
        // try to call newer API via reflection to keep compatibility
        var type = typeof(UnityEngine.Object);
        try
        {
            var method = type.GetMethod("FindFirstObjectByType", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            if (method != null)
            {
                return (T)method.MakeGenericMethod(typeof(T)).Invoke(null, null);
            }
        }
        catch { }

        try
        {
            var method2 = type.GetMethod("FindAnyObjectByType", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            if (method2 != null)
            {
                return (T)method2.MakeGenericMethod(typeof(T)).Invoke(null, null);
            }
        }
        catch { } //?

        // fallback to the legacy API (may be marked obsolete on newer Unity versions)
#pragma warning disable 0618
        var found = UnityEngine.Object.FindObjectOfType<T>();
#pragma warning restore 0618
        return found;
    }
    private void OnDestroy()
    {
        if (_nfcReceiver != null)
        {
            _nfcReceiver.OnNFCConnected.RemoveListener(OnNfcConnected);
            _nfcReceiver.OnNFCDisconnected.RemoveListener(OnNfcDisconnected);
        }
    }

    public override void TurnOn()
    {
        try
        {
            if (SessionPlacar.Instance == null)
            {
                Debug.LogError("SessionPlacar.Instance is null. Cannot update UI.");
            }
            else
            {
                UpdateUI();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error updating UI: " + ex.Message);
        }


        if (serverComunication == null)
        {
            serverComunication = FindFirstOrAny<ServerComunication>();
        }
        // try to find NFCReceiver in scene (use newer API when available)
        _nfcReceiver = FindFirstOrAny<NFCReceiver>();
        if (_nfcReceiver != null)
        {
            _nfcReceiver.OnNFCConnected.AddListener(OnNfcConnected);
            _nfcReceiver.OnNFCDisconnected.AddListener(OnNfcDisconnected);
        }

        // set feedback to yellow (no card)
        if (nfcConnectionFeedback != null)
            // nfcConnectionFeedback.color = Color.yellow;

            // Atualizar as barras segmentadas quando a tela for ativada
            UpdateSegmentedBars();

        base.TurnOn();
    }

    private void UpdateUI()
    {
        // Atualizar textos
        decisionMakingText.text = SessionPlacar.Instance.TomadaDeDecisao.ToString();
        criticalThinkingText.text = SessionPlacar.Instance.PensamentoCritico.ToString();
        problemSolvingText.text = SessionPlacar.Instance.SolucaoDeProblemas.ToString();

        // Atualizar barras segmentadas
        UpdateSegmentedBars();
    }

    private void UpdateSegmentedBars()
    {
        if (SessionPlacar.Instance == null) return;

        // Tomada de Decisão (max: 27 pontos)
        if (decisionMakingBar != null)
        {
            // Converter pontuação atual para segmentos (assumindo 16 segmentos por padrão)
            int segments = CalculateSegments(SessionPlacar.Instance.TomadaDeDecisao, SessionPlacar.Instance.TomadaDeDecisaoMax, 16);
            StartCoroutine(decisionMakingBar.AnimateBar(segments));
        }

        // Pensamento Crítico (max: 27 pontos)
        if (criticalThinkingBar != null)
        {
            int segments = CalculateSegments(SessionPlacar.Instance.PensamentoCritico, SessionPlacar.Instance.PensamentoCriticoMax, 16);
            StartCoroutine(criticalThinkingBar.AnimateBar(segments));
        }

        // Solução de Problemas (max: 18 pontos)
        if (problemSolvingBar != null)
        {
            int segments = CalculateSegments(SessionPlacar.Instance.SolucaoDeProblemas, SessionPlacar.Instance.SolucaoDeProblemasMax, 16);
            StartCoroutine(problemSolvingBar.AnimateBar(segments));
        }
    }

    private int CalculateSegments(int currentPoints, int maxPoints, int totalSegments)
    {
        if (maxPoints <= 0) return 0;

        // Calcula proporcionalmente quantos segmentos devem estar preenchidos
        float percentage = (float)currentPoints / maxPoints;
        return Mathf.RoundToInt(percentage * totalSegments);
    }

    /// <summary>
    /// Método público para atualizar as barras segmentadas externamente
    /// </summary>
    public void RefreshSegmentedBars()
    {
        UpdateSegmentedBars();
    }

    // NFC events
    private void OnNfcDisconnected()
    {
        if (nfcConnectionFeedback != null)
            nfcConnectionFeedback.color = Color.yellow;
    }

    private async void OnNfcConnected(string nfcId, string readerName)
    {
        Debug.Log($"NFC connected: {nfcId} via {readerName}");

        if (nfcConnectionFeedback != null)
            nfcConnectionFeedback.color = Color.cyan; // card detected, pending send

        // build GameModel for "Qual o caminho" which is gameId = 1
        var model = new GameModel
        {
            nfcId = nfcId,
            gameId = 1,
            // According to mapping in SessionPlacar: 
            // TomadaDeDecisao -> skill1
            // PensamentoCritico -> skill2
            // SolucaoDeProblemas -> skill3
            skill1 = SessionPlacar.Instance != null ? SessionPlacar.Instance.TomadaDeDecisao : 0,
            skill2 = SessionPlacar.Instance != null ? SessionPlacar.Instance.PensamentoCritico : 0,
            skill3 = SessionPlacar.Instance != null ? SessionPlacar.Instance.SolucaoDeProblemas : 0
        };

        if (serverComunication == null)
        {
            Debug.LogWarning("ServerComunication not found in scene. Cannot send NFC data.");
            if (nfcConnectionFeedback != null)
                nfcConnectionFeedback.color = Color.red;
            return;
        }

        try
        {
            var status = await serverComunication.UpdateNfcInfoFromGame(model);
            if (status == HttpStatusCode.OK || status == HttpStatusCode.Created || status == HttpStatusCode.Accepted)
            {
                Debug.Log("NFC data sent successfully.");
                if (nfcConnectionFeedback != null)
                    nfcConnectionFeedback.color = Color.green;

                // show success feedback briefly, then advance to the next screen
                try
                {
                    await Task.Delay(700);
                    CallNextScreen();
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("Failed to advance to next screen: " + ex.Message);
                }
            }
            else
            {
                Debug.LogWarning($"Server returned status {status}");
                if (nfcConnectionFeedback != null)
                    nfcConnectionFeedback.color = Color.red;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error sending NFC data: " + ex.Message);
            if (nfcConnectionFeedback != null)
                nfcConnectionFeedback.color = Color.red;
        }
    }
}
