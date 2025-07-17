using System;
using UnityEngine;
using System.Collections.Generic;


// ====================================
// ALL SERIALIZABLE DATA STRUCTURES FOR LOGGING
// ====================================

[Serializable]
public class BookPlacedData
{
    public string book;
    public bool correct;
    public int placedCount;

    public BookPlacedData(string book, bool correct, int placedCount)
    {
        this.book = book;
        this.correct = correct;
        this.placedCount = placedCount;
    }
}

[Serializable]
public class CubePlacedData
{
    public string cube;
    public string result;

    public CubePlacedData(string cube, string result)
    {
        this.cube = cube;
        this.result = result;
    }
}

[Serializable]
public class QuizAttemptData
{
    public string query;
    public string result;

    public QuizAttemptData(string query, string result)
    {
        this.query = query;
        this.result = result;
    }
}

[Serializable]
public class TutorialCompletedData
{
    public string tutorialName;

    public TutorialCompletedData(string tutorialName)
    {
        this.tutorialName = tutorialName;
    }
}

[System.Serializable]
public class TutorialSlideViewedData
{
    public string screenName;
    public int slideIndex;
    public string slideText;

    public TutorialSlideViewedData(string screenName, int slideIndex, string slideText)
    {
        this.screenName = screenName;
        this.slideIndex = slideIndex;
        this.slideText = slideText;
    }
}


[Serializable]
public class TutorialQuizData
{
    public string tutorialName;
    public string question;
    public string answer;
    public string result;

    public TutorialQuizData(string tutorialName, string question, string answer, string result)
    {
        this.tutorialName = tutorialName;
        this.question = question;
        this.answer = answer;
        this.result = result;
    }
}

[Serializable]
public class TutorialStepData
{
    public string tutorialName;
    public int stepIndex;
    public string queryTitle;

    public TutorialStepData(string tutorialName, int stepIndex, string queryTitle)
    {
        this.tutorialName = tutorialName;
        this.stepIndex = stepIndex;
        this.queryTitle = queryTitle;
    }
}

[Serializable]
public class LevelAdvanceData
{
    public int from;
    public int to;

    public LevelAdvanceData(int from, int to)
    {
        this.from = from;
        this.to = to;
    }
}

[System.Serializable]
public class NpcChatStartData
{
    public int messageId;

    public NpcChatStartData(int messageId)
    {
        this.messageId = messageId;
    }
}

[System.Serializable]
public class ColumnSelectionLogData
{
    public string gameType = "ColumnSelection";
    public string question;
    public List<string> selectedTables;
    public string result;

    public ColumnSelectionLogData(string question, List<string> selectedTables, string result)
    {
        this.question = question;
        this.selectedTables = selectedTables;
        this.result = result;
    }
}

[System.Serializable]
public class QueryImageLogData
{
    public string gameType = "QueryImage";
    public string selectedQuery;
    public string correctQuery;
    public string result;
    public int index;

    public QueryImageLogData(string selected, string correct, string result, int index)
    {
        this.selectedQuery = selected;
        this.correctQuery = correct;
        this.result = result;
        this.index = index;
    }
}


