using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SegmentedBar))]
public class SegmentedBarEditor : Editor
{
    private int novoValor = 0;

    public override void OnInspectorGUI()
    {
        // Referência ao script alvo
        SegmentedBar barra = (SegmentedBar)target;

        // Desenha o inspector padrão
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("== Ferramentas de Debug ==", EditorStyles.boldLabel);

        // Botão para reconstruir a barra
        if (GUILayout.Button("Recriar Segmentos"))
        {
            barra.CriarSegmentos();
            barra.AtualizarBarra();
        }

        EditorGUILayout.Space(5);

        // Campo para testar valor manualmente
        novoValor = EditorGUILayout.IntSlider("Novo Valor", novoValor, 0, barra.totalSegmentos);

        if (GUILayout.Button("Aplicar Valor (Testar Pop)"))
        {
            // Garante que a ação pode ser desfeita no editor
            Undo.RecordObject(barra, "Alterar Valor SegmentedBar");

            // Aplica o novo valor
            barra.SetValor(novoValor);

            // Marca o objeto como sujo para atualizar a visualização no Editor
            EditorUtility.SetDirty(barra);
        }
    }
}
