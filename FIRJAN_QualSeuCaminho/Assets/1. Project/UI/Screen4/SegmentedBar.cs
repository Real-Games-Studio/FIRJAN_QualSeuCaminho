using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class SegmentedBar : MonoBehaviour
{
    [Header("Configurações Gerais")]
    [Tooltip("Prefab do segmento (ex: quadrado, coração, etc.)")]
    public GameObject prefabSegmento;

    [Tooltip("Número total de segmentos que compõem a barra.")]
    [Min(1)]
    public int totalSegmentos = 10;

    [Tooltip("Valor atual (quantos segmentos ativos).")]
    [Range(0, 100)]
    public int valorAtual = 5;

    [Tooltip("Espaçamento entre cada segmento no eixo X.")]
    public float offsetX = 30f;

    [Tooltip("Direção positiva (true = direita, false = esquerda).")]
    public bool esquerdaParaDireita = true;

    private List<GameObject> segmentos = new List<GameObject>();
    private int valorAnterior = -1;
    private int totalAnterior = -1;
    private GameObject prefabAnterior;

    void Start()
    {
        CriarOuAtualizarSegmentos();
        AtualizarBarra();
    }

    /// <summary>
    /// Cria/atualiza os segmentos conforme necessário:
    /// - se o prefab mudou -> recria tudo
    /// - se total aumentou -> instancia novos
    /// - se total diminuiu -> destrói extras
    /// Reposiciona todos os segmentos
    /// </summary>
    public void CriarOuAtualizarSegmentos()
    {
        if (prefabSegmento == null || totalSegmentos <= 0)
            return;

        // Caso o prefab tenha mudado, recria tudo
        if (prefabSegmento != prefabAnterior)
        {
            // destroy all children
            foreach (Transform child in transform)
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
            segmentos.Clear();

            // instantiate new set
            for (int i = 0; i < totalSegmentos; i++)
            {
                GameObject novoSegmento = Instantiate(prefabSegmento, transform);
                novoSegmento.name = $"Segmento_{i + 1}";
                segmentos.Add(novoSegmento);
            }

            prefabAnterior = prefabSegmento;
            totalAnterior = totalSegmentos;
            ReposicionarSegmentos();
            return;
        }

        // Se total mudou em relação ao que já instanciamos
        if (totalSegmentos != totalAnterior)
        {
            // Se aumentou: instancia os adicionais
            if (totalSegmentos > segmentos.Count)
            {
                int inicio = segmentos.Count;
                for (int i = inicio; i < totalSegmentos; i++)
                {
                    GameObject novoSegmento = Instantiate(prefabSegmento, transform);
                    novoSegmento.name = $"Segmento_{i + 1}";
                    segmentos.Add(novoSegmento);
                }
            }
            // Se diminuiu: destrói os excedentes
            else if (totalSegmentos < segmentos.Count)
            {
                for (int i = segmentos.Count - 1; i >= totalSegmentos; i--)
                {
                    var go = segmentos[i];
                    segmentos.RemoveAt(i);
                    if (Application.isPlaying)
                        Destroy(go);
                    else
                        DestroyImmediate(go);
                }
            }

            totalAnterior = totalSegmentos;
            ReposicionarSegmentos();
        }
        else
        {
            // mesmo total — apenas garante reposicionamento caso offset/direção tenham mudado
            ReposicionarSegmentos();
        }
    }

    /// <summary>
    /// Reposiciona os segmentos com base em offsetX e direção.
    /// </summary>
    private void ReposicionarSegmentos()
    {
        float direcao = esquerdaParaDireita ? 1f : -1f;
        for (int i = 0; i < segmentos.Count; i++)
        {
            if (segmentos[i] == null) continue;
            segmentos[i].transform.localPosition = new Vector3(i * offsetX * direcao, 0f, 0f);
            segmentos[i].name = $"Segmento_{i + 1}";
        }
    }

    /// <summary>
    /// Atualiza visibilidade/estado dos segmentos conforme valorAtual.
    /// Não recria nada aqui — só ativa/desativa para performance.
    /// </summary>
    public void AtualizarBarra()
    {
        int ativoAte = Mathf.Clamp(valorAtual, 0, totalSegmentos);

        // evita work inútil
        if (segmentos == null) segmentos = new List<GameObject>();
        if (ativoAte == valorAnterior && segmentos.Count == totalAnterior) return;

        valorAnterior = ativoAte;

        for (int i = 0; i < segmentos.Count; i++)
        {
            var go = segmentos[i];
            if (go == null) continue;
            bool deveAtivar = i < ativoAte;
            if (go.activeSelf != deveAtivar)
                go.SetActive(deveAtivar);
        }
    }

    /// <summary>
    /// Ajusta o valor atual e atualiza a barra.
    /// </summary>
    public void SetValor(int novoValor)
    {
        valorAtual = Mathf.Clamp(novoValor, 0, totalSegmentos);
        AtualizarBarra();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Em editor: tenta manter as coisas consistentes sem precisar entrar em play
        CriarOuAtualizarSegmentos();
        AtualizarBarra();
    }
#endif
}
