using System;
using System.Collections.Generic;
using UnityEngine;

// ============================
// Challenge Types per Mini-Game
// ============================

[Serializable]
public class TableMatchingContext
{
    public string question;               
    public string[] availableTables;      
    public string[] correctJoinPath;      
}


[Serializable]
public class QueryResultChallengeContext
{
    public string correctQuery;
    public List<string> options;
    public string explanation;
}

[Serializable]
public class QueryColorGameContext
{
    public string query;

    [Serializable]
    public class Row
    {
        public string Book_Title;
        public string Author;
        public string Genre;
        public string Publication_Year;

    }

    public Row[] tableData;
    public int[] correctRows;
}



// ============================
// Challenge Holder
// ============================

public static class ChallengeContextHolder
{
    public static string GameName;
    public static string GameContextJson;

    public static void Set(string gameName, string gameContextJson)
    {
        GameName = gameName;
        GameContextJson = gameContextJson;
    }
    public static void Set<T>(string gameName, T challengeObject)
    {
        GameName = gameName;
        GameContextJson = JsonUtility.ToJson(challengeObject);
    }
    

    public static void Clear()
    {
        GameName = null;
        GameContextJson = null;
    }
}
