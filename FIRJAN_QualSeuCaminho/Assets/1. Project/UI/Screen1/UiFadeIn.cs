using UnityEngine;
using UnityEngine.UI;

public class UIFadeIn : MonoBehaviour
{
    [Header("Configurações de Fade")]
    [Tooltip("Tempo mínimo para o fade (segundos)")]
    public float tempoMin = 1f;

    [Tooltip("Tempo máximo para o fade (segundos)")]
    public float tempoMax = 3f;

    [Tooltip("Usar CanvasGroup (recomendado para grupos de UI)")]
    public bool usarCanvasGroup = true;

    private CanvasGroup canvasGroup;
    private CanvasGroup canvasPai;
    private Graphic graphic;
    private float duracao;
    private float tempoAtual = 0f;
    private bool terminou = false;
    private bool iniciado = false;

    void Start()
    {
        // Sorteia uma duração aleatória
        duracao = Random.Range(tempoMin, tempoMax);

        // Obtém componentes
        canvasGroup = GetComponent<CanvasGroup>();
        graphic = GetComponent<Graphic>();

        // Tenta achar um CanvasGroup no pai
        if (transform.parent != null)
            canvasPai = transform.parent.GetComponentInParent<CanvasGroup>();

        // Inicia com opacidade 0
        if (usarCanvasGroup && canvasGroup != null)
            canvasGroup.alpha = 0f;
        else if (graphic != null)
        {
            Color c = graphic.color;
            c.a = 0f;
            graphic.color = c;
        }
        else
        {
            Debug.LogWarning("Nenhum componente de UI encontrado! Adicione um CanvasGroup ou Image/Text.");
            enabled = false;
        }
    }

    void Update()
    {
        if (terminou) return;

        // Verifica se o canvas pai está totalmente visível antes de iniciar
        if (!iniciado)
        {
            if (canvasPai == null || canvasPai.alpha < 1f)
                return; // espera o pai atingir alpha >= 1
            iniciado = true; // inicia o fade
        }

        tempoAtual += Time.deltaTime;
        float t = Mathf.Clamp01(tempoAtual / duracao);
        float novaOpacidade = Mathf.Lerp(0f, 1f, t);

        // Aplica a opacidade
        if (usarCanvasGroup && canvasGroup != null)
            canvasGroup.alpha = novaOpacidade;
        else if (graphic != null)
        {
            Color c = graphic.color;
            c.a = novaOpacidade;
            graphic.color = c;
        }

        if (t >= 1f)
            terminou = true;
    }
}
