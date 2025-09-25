using TMPro;
using UnityEngine;

public class CavasScreenGamePlay : CanvasScreen
{
    public enum GameState
    {
        QuestionDisplayed,
        AnswerSelected,
        ShowingFeedback,
        MovingPlayer,
        GameOver
    }

    public int currentQuestionIndex = 0;
    public float questionTime = 45f; // Tempo total para cada pergunta

    [System.Serializable]
    private struct QuestionUI
    {
        [SerializeField] private GameObject questionPanel; // painel que contem toda a pergunta
        [SerializeField] private TMP_Text timerText; //toda pergunta, tem 45 segundos para ser respondida

        [SerializeField] private TMP_Text questionTitle;
        [SerializeField] private TMP_Text questionDescription;

        [SerializeField] private TMP_Text answerA;
        [SerializeField] private TMP_Text answerB;
    }

    [SerializeField] private QuestionUI questionUI;

    [System.Serializable]
    private struct FeedbackUI
    {
        [SerializeField] private GameObject feedbackPanel; // painel que contem todo o feedback
        [SerializeField] private TMP_Text feedbackTitle;
    }
    [SerializeField] private FeedbackUI feedbackUI;
    [SerializeField] private int currentCasaIndex = 0; // índice da casa atual do jogador, 
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject[] casas; // lista de posicoes das casas do tabuleiro

    public override void TurnOn()
    {
        // sempre que abrirmos essta tela as configuracoes de jogo devem ser configuradas para o inicio.
        // jogador voltar para a casa 0.
        // carregar a primeira pergunta
        currentCasaIndex = 0;
        currentQuestionIndex = 0;
        MovePlayerToCasa(currentCasaIndex);
        base.TurnOn();
    }
    public void MovePlayerToCasa(int casaIndex)
    {
        if (casaIndex >= 0 && casaIndex < casas.Length)
        {
            player.transform.position = casas[casaIndex].transform.position;
        }
        else
        {
            Debug.LogError("Índice da casa inválido: " + casaIndex);
        }
    }
    // Am
    public void ClickOnAnswerA(int awnserIndex)
    {
    }
}
