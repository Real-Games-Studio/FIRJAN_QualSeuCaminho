using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SegmentedBar : MonoBehaviour
{
    [Header("Configurações Gerais")]
    public GameObject prefabSegmento;
    public int totalSegmentos = 20;
    [Range(0, 100)] public int valorAtual = 0;
    public float offsetX = 30f;
    public bool esquerdaParaDireita = true;

    [Header("Efeito de Pop")]
    public float popEscala = 1.2f;
    public float popDuracao = 0.2f;

    [Header("Integração com Placar")]
    [Tooltip("Arraste aqui o script SessionPlacar (opcional)")]
    public SessionPlacar sessionPlacar;

    [Tooltip("Campo do SessionPlacar que será lido")]
    public TipoValorPlacar tipoValor; // enum abaixo

    private List<GameObject> segmentos = new List<GameObject>();
    private int valorAnterior = -1;

    public enum TipoValorPlacar
    {
        TomadaDeDecisao,
        PensamentoCritico,
        SolucaoDeProblemas
        // etc — adicione o que quiser
    }

    void Start()
    {
        CriarSegmentos();
        AtualizarBarra();
    }

    void OnEnable()
    {
        // Atualiza uma vez quando o objeto habilitar
        AtualizarBarra();
    }

    public void AtualizarValorDoPlacar()
    {
        if (sessionPlacar == null) return;

        int valor = 0;
        int maximo = 1; // evita divisão por zero

        switch (tipoValor)
        {
            case TipoValorPlacar.TomadaDeDecisao:
                valor = sessionPlacar.TomadaDeDecisao;
                maximo = sessionPlacar.TomadaDeDecisaoMax;
                break;

            case TipoValorPlacar.PensamentoCritico:
                valor = sessionPlacar.PensamentoCritico;
                maximo = sessionPlacar.PensamentoCriticoMax;
                break;

            case TipoValorPlacar.SolucaoDeProblemas:
                valor = sessionPlacar.SolucaoDeProblemas;
                maximo = sessionPlacar.SolucaoDeProblemasMax;
                break;
        }

        float proporcao = Mathf.Clamp01(valor / (float)maximo);
        int novoValor = Mathf.RoundToInt(proporcao * totalSegmentos);

        SetValor(novoValor);
    }

    public void CriarSegmentos()
    {
        foreach (Transform child in transform)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }

        segmentos.Clear();

        for (int i = 0; i < totalSegmentos; i++)
        {
            GameObject novo = Instantiate(prefabSegmento, transform);
            novo.name = $"Segmento_{i + 1}";
            float direcao = esquerdaParaDireita ? 1f : -1f;
            novo.transform.localPosition = new Vector3(i * offsetX * direcao, 0f, 0f);
            novo.transform.localScale = Vector3.one;
            segmentos.Add(novo);
        }
    }

    public void AtualizarBarra()
    {
        if (valorAtual == valorAnterior) return;

        bool aumento = valorAtual > valorAnterior;
        valorAnterior = valorAtual;

        for (int i = 0; i < segmentos.Count; i++)
        {
            bool deveAtivar = i < valorAtual;

            if (deveAtivar && !segmentos[i].activeSelf && aumento)
            {
                segmentos[i].SetActive(true);
                StartCoroutine(AnimarPop(segmentos[i].transform));
            }
            else
            {
                segmentos[i].SetActive(deveAtivar);
            }
        }
    }

    public void SetValor(int novoValor)
    {
        valorAtual = Mathf.Clamp(novoValor, 0, totalSegmentos);
        AtualizarBarra();
    }

    private IEnumerator AnimarPop(Transform t)
    {
        Vector3 escalaInicial = Vector3.one;
        Vector3 escalaMaxima = Vector3.one * popEscala;

        float metadeTempo = popDuracao / 2f;
        float tempo = 0f;

        while (tempo < metadeTempo)
        {
            tempo += Time.deltaTime;
            float tLerp = tempo / metadeTempo;
            t.localScale = Vector3.Lerp(escalaInicial, escalaMaxima, tLerp);
            yield return null;
        }

        tempo = 0f;
        while (tempo < metadeTempo)
        {
            tempo += Time.deltaTime;
            float tLerp = tempo / metadeTempo;
            t.localScale = Vector3.Lerp(escalaMaxima, escalaInicial, tLerp);
            yield return null;
        }

        t.localScale = Vector3.one;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (prefabSegmento == null || totalSegmentos <= 0)
            return;

        CriarSegmentos();
        AtualizarBarra();
    }
#endif
}
