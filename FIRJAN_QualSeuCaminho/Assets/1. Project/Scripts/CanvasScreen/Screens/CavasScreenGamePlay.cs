using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public class QuestionUI
    {
        public GameObject questionPanel; // painel que contem toda a pergunta
        public TMP_Text timerText; //toda pergunta, tem 45 segundos para ser respondida

        public TMP_Text questionTitle;
        public TMP_Text questionDescription;

        public TMP_Text answerA;
        public TMP_Text answerB;
    }

    [SerializeField] private QuestionUI questionUI;
    public Button answerAButton;
    public Button answerBButton;
    private UnityEngine.Events.UnityAction onAnswerACallback;
    private UnityEngine.Events.UnityAction onAnswerBCallback;
    private float timerRemaining;
    private bool questionActive = false;
    [SerializeField] private GameState currentState = GameState.QuestionDisplayed;
    private System.Random rng = new System.Random();
    private int currentDisplayAIndex = 0; // index in answers shown as A
    private int currentDisplayBIndex = 1; // index in answers shown as B

    [System.Serializable]
    public class FeedbackUI
    {
        public GameObject feedbackPanel; // painel que contem todo o feedback
        public TMP_Text feedbackTitle;
    }
    [SerializeField] private FeedbackUI feedbackUI;
    [SerializeField] private float feedbackDuration = 2f; // tempo fixo que o feedback permanece visível antes da movimentação
    [SerializeField] private float gameOverDelay = 1.5f; // tempo a aguardar no estado GameOver antes de chamar a próxima tela
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
        // subscribe to gamedata updates and localization
        if (GameDataLoader.instance != null)
        {
            GameDataLoader.instance.OnGameDataUpdated += OnGameDataUpdated;
        }
        // wire up answer buttons safely (remove previous then add to avoid duplicates)
        if (answerAButton != null)
        {
            if (onAnswerACallback == null) onAnswerACallback = new UnityEngine.Events.UnityAction(() => ClickOnAnswerA(0));
            answerAButton.onClick.RemoveListener(onAnswerACallback);
            answerAButton.onClick.AddListener(onAnswerACallback);
        }
        if (answerBButton != null)
        {
            if (onAnswerBCallback == null) onAnswerBCallback = new UnityEngine.Events.UnityAction(() => ClickOnAnswerB(0));
            answerBButton.onClick.RemoveListener(onAnswerBCallback);
            answerBButton.onClick.AddListener(onAnswerBCallback);
        }
        StartQuestion(currentQuestionIndex);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (GameDataLoader.instance != null)
        {
            GameDataLoader.instance.OnGameDataUpdated -= OnGameDataUpdated;
        }
        // remove button listeners
        if (answerAButton != null && onAnswerACallback != null)
        {
            answerAButton.onClick.RemoveListener(onAnswerACallback);
        }
        if (answerBButton != null && onAnswerBCallback != null)
        {
            answerBButton.onClick.RemoveListener(onAnswerBCallback);
        }
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
    private void OnGameDataUpdated()
    {
        // refresh the current question display (texts may have changed)
        StartQuestion(currentQuestionIndex);
    }

    private void StartQuestion(int questionIndex)
    {
        // If player is already on the final casa, do not show a question — go to GameOver flow
        if (casas != null && casas.Length > 0 && currentCasaIndex == casas.Length - 1)
        {
            currentState = GameState.GameOver;
            // ensure UI panels are hidden appropriately
            if (questionUI != null && questionUI.questionPanel != null)
                questionUI.questionPanel.SetActive(false);
            if (feedbackUI != null && feedbackUI.feedbackPanel != null)
                feedbackUI.feedbackPanel.SetActive(false);

            StartCoroutine(HandleFinalCasaThenNext());
            return;
        }
        var gd = GameDataLoader.instance?.loadedData;
        if (gd == null || gd.questions == null || gd.questions.Count == 0) return;
        if (questionIndex < 0 || questionIndex >= gd.questions.Count) return;

        var q = gd.questions[questionIndex];

        // We expect each question to have 3 answers: first(3 casas), second(2), third(1)
        // We'll pick two options to show (A and B). If q.answers has less than 3, fallbacks apply.
        int answersCount = q.answers?.Count ?? 0;
        if (answersCount < 1) return;

        // Always ensure we have indices 0..2 available; clamp
        int idx0 = 0;
        int idx1 = Mathf.Min(1, answersCount - 1);
        int idx2 = Mathf.Min(2, answersCount - 1);

        // Shuffle whether the 3-casa answer (assumed at idx0) goes to A or B
        bool threeOnA = rng.Next(0, 2) == 0;
        if (threeOnA)
        {
            currentDisplayAIndex = idx0; // 3 casas
            currentDisplayBIndex = idx1; // 2 casas
        }
        else
        {
            currentDisplayAIndex = idx1; // 2 casas
            currentDisplayBIndex = idx0; // 3 casas
        }

        // Set UI texts (guarding for nulls)
        if (questionUI.questionTitle != null) questionUI.questionTitle.text = q.title;
        if (questionUI.questionDescription != null) questionUI.questionDescription.text = q.description;
        if (questionUI.answerA != null) questionUI.answerA.text = q.answers[currentDisplayAIndex].text;
        if (questionUI.answerB != null) questionUI.answerB.text = q.answers[currentDisplayBIndex].text;

        // timer
        timerRemaining = questionTime;
        questionActive = true;
    }

    private void Update()
    {
        // State machine tick
        switch (currentState)
        {
            case GameState.QuestionDisplayed:
                if (!questionActive) return;
                feedbackUI.feedbackPanel.SetActive(false);
                timerRemaining -= Time.deltaTime;
                if (questionUI.timerText != null) questionUI.timerText.text = Mathf.CeilToInt(timerRemaining).ToString();
                // wire up answer buttons safely
                if (timerRemaining <= 0f)
                {
                    questionActive = false;
                    TransitionToShowingFeedbackTimeout();
                }
                break;
            case GameState.AnswerSelected:
                // waiting for small delay or animation (handled by coroutine)
                break;
            case GameState.ShowingFeedback:
                // Showing feedback handled by coroutine
                break;
            case GameState.MovingPlayer:
                // movement handled separately (could be animated)
                break;
            case GameState.GameOver:
                questionUI.questionPanel.SetActive(false);
                feedbackUI.feedbackPanel.SetActive(false);
                break;
        }
    }

    private void HandleAnswerTimeout()
    {
        var gd = GameDataLoader.instance?.loadedData;
        if (gd == null) return;
        var q = gd.questions[currentQuestionIndex];
        int answersCount = q.answers?.Count ?? 0;
        int idxTimeout = Mathf.Min(2, Mathf.Max(0, answersCount - 1));
        TransitionToAnswerSelected(q, idxTimeout);
    }

    private void ApplyAnswerEffects(Question q, int answerIndex)
    {
        if (q == null || q.answers == null || answerIndex < 0 || answerIndex >= q.answers.Count) return;
        int moveBy = q.answers[answerIndex].casas;
        // compute target index but don't teleport yet
        int startIndex = currentCasaIndex;
        int targetIndex = Mathf.Clamp(startIndex + moveBy, 0, this.casas.Length - 1);

        // show feedback (simple) and disable question UI
        if (feedbackUI.feedbackPanel != null) feedbackUI.feedbackPanel.SetActive(true);
        if (feedbackUI.feedbackTitle != null) feedbackUI.feedbackTitle.text = q.answers[answerIndex].feedback;
        if (questionUI.questionPanel != null) questionUI.questionPanel.SetActive(false);

        // disable answer buttons while moving
        if (answerAButton != null) answerAButton.interactable = false;
        if (answerBButton != null) answerBButton.interactable = false;

        // award points according to casas moved: 3 casas -> +3, 2 casas -> +2, 1 casa -> +0
        try
        {
            if (moveBy >= 3)
            {
                SessionPlacar.AddPoints(3);
            }
            else if (moveBy == 2)
            {
                SessionPlacar.AddPoints(2);
            }
            else
            {
                SessionPlacar.AddPoints(0);
                // 1 casa or other: 0 points (no-op)
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Erro ao atualizar SessionPlacar: " + ex.Message);
        }

        currentState = GameState.ShowingFeedback;
        // show feedback for a fixed duration, then start movement
        StartCoroutine(ShowFeedbackThenMove(startIndex, targetIndex));
    }

    private System.Collections.IEnumerator MovePlayerStepwise(int startIndex, int targetIndex)
    {
        if (startIndex == targetIndex)
        {
            // nothing to move, small pause
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            for (int idx = startIndex + 1; idx <= targetIndex; idx++)
            {
                Vector3 from = player.transform.position;
                Vector3 to = casas[idx].transform.position;
                float duration = 0.35f;
                float t = 0f;
                while (t < duration)
                {
                    t += Time.deltaTime;
                    float v = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duration));
                    player.transform.position = Vector3.Lerp(from, to, v);
                    yield return null;
                }
                player.transform.position = to;
                // small pause between steps
                yield return new WaitForSeconds(0.06f);
            }
        }

        // finalize position and update indices
        currentCasaIndex = targetIndex;

        // hide feedback and re-enable question UI
        if (feedbackUI.feedbackPanel != null) feedbackUI.feedbackPanel.SetActive(false);
        if (questionUI.questionPanel != null) questionUI.questionPanel.SetActive(true);

        // re-enable buttons
        if (answerAButton != null) answerAButton.interactable = true;
        if (answerBButton != null) answerBButton.interactable = true;

        // if player reached final casa, go to next screen
        if (currentCasaIndex == casas.Length - 1)
        {
            currentState = GameState.GameOver;
            // small delay to show final state / celebration
            if (gameOverDelay > 0f)
                yield return new WaitForSeconds(gameOverDelay);
            CallNextScreen();
            yield break;
        }

        // advance to next question and show it
        currentQuestionIndex = (currentQuestionIndex + 1) % (GameDataLoader.instance.loadedData.questions.Count);
        currentState = GameState.QuestionDisplayed;
        StartQuestion(currentQuestionIndex);
    }

    private System.Collections.IEnumerator ShowFeedbackThenMove(int startIndex, int targetIndex)
    {
        // wait fixed feedback duration
        if (feedbackDuration > 0f)
            yield return new WaitForSeconds(feedbackDuration);

        // after fixed feedback time, start moving
        currentState = GameState.MovingPlayer;
        yield return StartCoroutine(MovePlayerStepwise(startIndex, targetIndex));
    }

    private System.Collections.IEnumerator NextQuestionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (feedbackUI.feedbackPanel != null) feedbackUI.feedbackPanel.SetActive(false);
        StartQuestion(currentQuestionIndex);
    }

    private System.Collections.IEnumerator HandleFinalCasaThenNext()
    {
        if (gameOverDelay > 0f)
            yield return new WaitForSeconds(gameOverDelay);
        CallNextScreen();
    }

    public void ClickOnAnswerA(int awnserIndex)
    {
        if (!questionActive) return;
        questionActive = false;
        var gd = GameDataLoader.instance?.loadedData;
        if (gd == null) return;
        var q = gd.questions[currentQuestionIndex];
        TransitionToAnswerSelected(q, currentDisplayAIndex);
    }

    public void ClickOnAnswerB(int awnserIndex)
    {
        if (!questionActive) return;
        questionActive = false;
        var gd = GameDataLoader.instance?.loadedData;
        if (gd == null) return;
        var q = gd.questions[currentQuestionIndex];
        TransitionToAnswerSelected(q, currentDisplayBIndex);
    }

    private void TransitionToAnswerSelected(Question q, int answerIndex)
    {
        currentState = GameState.AnswerSelected;
        // immediate apply effects, then move to showing feedback
        ApplyAnswerEffects(q, answerIndex);
    }

    private void TransitionToShowingFeedbackTimeout()
    {
        // timeout selects third option (index 2 or last)
        var gd = GameDataLoader.instance?.loadedData;
        if (gd == null) return;
        var q = gd.questions[currentQuestionIndex];
        int answersCount = q.answers?.Count ?? 0;
        int idxTimeout = Mathf.Min(2, Mathf.Max(0, answersCount - 1));
        TransitionToAnswerSelected(q, idxTimeout);
    }
}
