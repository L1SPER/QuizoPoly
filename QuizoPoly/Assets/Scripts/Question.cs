using System;
using System.Collections.Generic;

[Serializable]
public class Question
{
    public int id;
    public string category;
    public int difficulty;
    public string text;
    public string[] choices;
    public int correctAnswer;  // 0=A, 1=B, 2=C, 3=D
}

[Serializable]
public class QuestionDatabase
{
    public List<Question> questions;
}