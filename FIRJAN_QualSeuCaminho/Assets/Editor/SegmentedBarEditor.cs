using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SegmentedBar))]
public class SegmentedBarEditor : Editor
{
    SerializedProperty propPrefab;
    SerializedProperty propTotal;
    SerializedProperty propValor;
    SerializedProperty propOffset;
    SerializedProperty propDirecao;

    void OnEnable()
    {
        propPrefab = serializedObject.FindProperty("prefabSegmento");
        propTotal = serializedObject.FindProperty("totalSegmentos");
        propValor = serializedObject.FindProperty("valorAtual");
        propOffset = serializedObject.FindProperty("offsetX");
        propDirecao = serializedObject.FindProperty("esquerdaParaDireita");
    }

    public override void OnInspectorGUI()
    {
        // Atualiza o serializedObject antes de ler propriedades
        serializedObject.Update();

        // Desenha propriedades normais
        EditorGUILayout.PropertyField(propPrefab);
        EditorGUILayout.PropertyField(propTotal);
        EditorGUILayout.PropertyField(propValor);
        EditorGUILayout.PropertyField(propOffset);
        EditorGUILayout.PropertyField(propDirecao);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Controles", EditorStyles.boldLabel);

        // Se algum campo foi alterado no inspector, aplica e chama os métodos necessários
        if (serializedObject.hasModifiedProperties)
        {
            // Aplica as mudanças primeiro
            serializedObject.ApplyModifiedProperties();

            // Garante que o componente atualize sua estrutura
            SegmentedBar bar = (SegmentedBar)target;
            bar.CriarOuAtualizarSegmentos();
            bar.AtualizarBarra();
        }

        // Campo interativo extra para atualizar valor (usa a propriedade atual)
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Atualizar Valor (usar valorAtual)"))
        {
            SegmentedBar bar = (SegmentedBar)target;
            bar.SetValor(bar.valorAtual);
            EditorUtility.SetDirty(bar);
        }
        if (GUILayout.Button("Recriar Segmentos"))
        {
            SegmentedBar bar = (SegmentedBar)target;
            Undo.RecordObject(bar, "Recriar Segmentos");
            bar.CriarOuAtualizarSegmentos();
            bar.AtualizarBarra();
            EditorUtility.SetDirty(bar);
        }
        EditorGUILayout.EndHorizontal();

        // Se não houve Apply ainda (por exemplo, usuário só abriu e não mexeu), garantimos que propriedades visuais estejam sempre desenhadas
        // Note: não chamamos Apply novamente para não sobrescrever valores ainda não confirmados.
    }
}
