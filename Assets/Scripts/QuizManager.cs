using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizManager : MonoBehaviourPun
{
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;
    public List<QuizQuestion> questions;
    public Button nextButton;
    public Button previousButton;
    public Button submitButton;
    public Button restartButton;
    public Button spectatorButton; 
    public Image progressBar;
    public TextMeshProUGUI progressText;
    public GameObject leaderboardPanel; 
    public TextMeshProUGUI leaderboardText; 

    private int currentQuestionIndex = 0;
    private int selectedAnswerIndex = -1;
    private bool isSubmitted = false; 
    private bool isSpectator = false; 

    private Dictionary<int, int> playerScores = new Dictionary<int, int>();
    public TextAsset quizQuestionsJson; 
    private List<bool> questionAnswered; 

    public TextMeshProUGUI roomInfoText;
    public GameObject spectatePanel; 
    public Button spectateButton; 

    void Awake()
    {
        LoadQuestionsFromJson();
    }

    private void Start()
    {
        AssignPlayerNames();
        questionAnswered = new List<bool>(new bool[questions.Count]);
        if (questions != null && questions.Count > 0)
        {
            DisplayQuestion(currentQuestionIndex);
        }
        else
        {
            Debug.LogError("Questions list is null or empty. Cannot start the quiz.");
        }

        nextButton.onClick.AddListener(NextQuestion);
        previousButton.onClick.AddListener(PreviousQuestion);
        submitButton.onClick.AddListener(SubmitAnswer);
        restartButton.onClick.AddListener(RestartQuiz);
        spectateButton.onClick.AddListener(ToggleSpectatorMode);

        leaderboardPanel.SetActive(false);
        restartButton.gameObject.SetActive(false);
        spectatePanel.SetActive(false);
        DisplayRoomInfo(PhotonNetwork.CurrentRoom.Name);
    }

    private void DisplayRoomInfo(string roomName)
    {
        string roomInfo = $"Room Name: {roomName}";
        roomInfoText.text = roomInfo; 
        roomInfoText.gameObject.SetActive(true); 
    }

    private void AssignPlayerNames()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!player.CustomProperties.ContainsKey("PlayerName"))
            {
                string playerName = "Player " + (player.ActorNumber);
                ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable
                {
                    { "PlayerName", playerName },
                    { "Score", 0 }  
                };
                player.SetCustomProperties(playerProps);
                playerScores[player.ActorNumber] = 0;
            }
        }
    }

    private void LoadQuestionsFromJson()
    {
        if (quizQuestionsJson != null)
        {
            string jsonContent = quizQuestionsJson.text; 
            QuizData quizData = JsonUtility.FromJson<QuizData>(jsonContent);
            questions = quizData.questions;
            Debug.Log("Questions loaded successfully: " + questions.Count + " questions found.");
        }
        else
        {
            Debug.LogError("Quiz Questions JSON file is not assigned.");
        }
    }

    [PunRPC]
    public void DisplayQuestion(int index)
    {
        currentQuestionIndex = index;
        selectedAnswerIndex = -1; 
        isSubmitted = false; 
        QuizQuestion question = questions[index];
        questionText.text = question.questionText;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = question.answers[i];
            answerButtons[i].image.color = Color.black; 
            answerButtons[i].interactable = !questionAnswered[index]; 
            answerButtons[i].onClick.RemoveAllListeners(); 
            int answerIndex = i; 
            answerButtons[i].onClick.AddListener(() => photonView.RPC("AnswerSelected", RpcTarget.All, answerIndex));
        }

        progressBar.fillAmount = (float)(currentQuestionIndex + 1) / questions.Count;
        progressText.text = $"Question {currentQuestionIndex + 1}/{questions.Count}";

        previousButton.interactable = currentQuestionIndex > 0;
        nextButton.interactable = currentQuestionIndex < questions.Count - 1;
        submitButton.interactable = !questionAnswered[index]; 
    }

    [PunRPC]
    public void AnswerSelected(int answerIndex)
    {
        selectedAnswerIndex = answerIndex;
        Debug.Log("Selected Answer: " + answerIndex);

        foreach (Button button in answerButtons)
        {
            button.image.color = Color.black;
        }

        answerButtons[answerIndex].image.color = Color.yellow;
        submitButton.interactable = true; 
    }

    [PunRPC]
    public void SubmitAnswer()
    {
        if (selectedAnswerIndex == -1 || isSubmitted)
        {
            Debug.LogError("No answer selected or already submitted!");
            return;
        }

        bool isCorrect = selectedAnswerIndex == questions[currentQuestionIndex].correctAnswer;
        Color resultColor = isCorrect ? Color.green : Color.red;

        answerButtons[selectedAnswerIndex].image.color = resultColor;

        if (!isCorrect)
        {
            int correctAnswerIndex = questions[currentQuestionIndex].correctAnswer;
            answerButtons[correctAnswerIndex].image.color = Color.green; 
        }

        isSubmitted = true; 
        questionAnswered[currentQuestionIndex] = true; 

        foreach (Button button in answerButtons)
        {
            button.interactable = false;
        }

        if (isCorrect)
        {
            int currentScore = (int)PhotonNetwork.LocalPlayer.CustomProperties["Score"];
            currentScore++;
            ExitGames.Client.Photon.Hashtable newProps = new ExitGames.Client.Photon.Hashtable { { "Score", currentScore } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(newProps);
        }

        if (currentQuestionIndex == questions.Count - 1)
        {
            ShowLeaderboard();
        }
    }

    public void NextQuestion()
    {
        if (currentQuestionIndex < questions.Count - 1)
        {
            photonView.RPC("DisplayQuestion", RpcTarget.All, currentQuestionIndex + 1);
        }
    }

    public void PreviousQuestion()
    {
        if (currentQuestionIndex > 0)
        {
            photonView.RPC("DisplayQuestion", RpcTarget.All, currentQuestionIndex - 1);
        }
    }

    private void ShowLeaderboard()
    {
        leaderboardPanel.SetActive(true); 
        leaderboardText.text = "Final Scores:\n";

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            string playerName = (string)player.CustomProperties["PlayerName"];
            int playerScore = (int)player.CustomProperties["Score"];
            leaderboardText.text += $"{playerName}: {playerScore} points\n";
        }

        restartButton.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(false); 
        submitButton.gameObject.SetActive(false); 
        previousButton.gameObject.SetActive(false); 
    }

    private void RestartQuiz()
    {
        currentQuestionIndex = 0;
        selectedAnswerIndex = -1;
        isSubmitted = false;

        questionAnswered = new List<bool>(new bool[questions.Count]);

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            ExitGames.Client.Photon.Hashtable resetProps = new ExitGames.Client.Photon.Hashtable { { "Score", 0 } };
            player.SetCustomProperties(resetProps);
        }

        leaderboardPanel.SetActive(false);
        restartButton.gameObject.SetActive(false);

        nextButton.gameObject.SetActive(true);
        submitButton.gameObject.SetActive(true);
        previousButton.gameObject.SetActive(true);

        photonView.RPC("DisplayQuestion", RpcTarget.All, 0);
    }

    public void ToggleSpectatorMode()
    {
        isSpectator = !isSpectator; 

        if (isSpectator)
        {
            spectatePanel.SetActive(true);
            spectateButton.GetComponentInChildren<TextMeshProUGUI>().text = "Play";
        }
        else
        {
            spectatePanel.SetActive(false);
            spectateButton.GetComponentInChildren<TextMeshProUGUI>().text = "Spectate";
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}
