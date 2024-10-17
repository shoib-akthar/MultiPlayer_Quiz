using System.Collections.Generic;

[System.Serializable]
public class QuizQuestion
{
    public string questionText;
    public string[] answers;
    public int correctAnswer;
}

[System.Serializable]
public class QuizData
{
    public List<QuizQuestion> questions;
}
