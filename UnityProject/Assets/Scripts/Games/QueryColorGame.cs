using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QueryColorGame : MonoBehaviour
{
    [System.Serializable]
    public class SQLQuery
    {
        public string queryText;
        public List<CellInteraction> correctCells;
    }

    [System.Serializable]
    public class QueryGroup
    {
        public GameObject table;
        public List<SQLQuery> queries;
    }

    public List<QueryGroup> queryGroups;
    public List<TextMeshProUGUI> slotTexts;

    public Sprite defaultSprite;
    public Sprite highlightSprite;
    public Sprite wrongSprite;

    private int currentGroupIndex = -1;
    private int activeQueryIndex = 0;
    private bool isFinished = false;


    private Dictionary<TextMeshProUGUI, List<CellInteraction>> slotToCells = new();
    private Dictionary<TextMeshProUGUI, int> slotToQueryIndex = new();
    private HashSet<CellInteraction> selectedCells = new();
    private List<SQLQuery> currentQueries;

    private void Start()
    {
        ActivateNextGroup();
    }

    void ActivateNextGroup()
    {
        if (currentGroupIndex >= 0 && currentGroupIndex < queryGroups.Count)
        {
            bool isLastGroup = currentGroupIndex == queryGroups.Count - 1;
            if (!isLastGroup)
            {
                queryGroups[currentGroupIndex].table.SetActive(false);
            }
        }


        currentGroupIndex++;

        if (currentGroupIndex >= queryGroups.Count)
        {
            Debug.Log("✅ Όλα τα groups ολοκληρώθηκαν.");
             isFinished = true;
            return;
        }

        var group = queryGroups[currentGroupIndex];
        currentQueries = group.queries;
        activeQueryIndex = 0;
        slotToCells.Clear();
        slotToQueryIndex.Clear();
        selectedCells.Clear();

        group.table.SetActive(true);

        foreach (var cell in group.table.GetComponentsInChildren<CellInteraction>())
        {
            cell.Init(this);
        }

        RefreshQuerySlots();
    }

    void SetSlotContent(TextMeshProUGUI textUI, SQLQuery query, int queryIndex)
    {
        textUI.text = query.queryText;
        textUI.fontMaterial = new Material(textUI.fontMaterial);
        textUI.fontMaterial.SetColor(ShaderUtilities.ID_FaceColor, Color.gray);

        slotToCells[textUI] = query.correctCells;
        slotToQueryIndex[textUI] = queryIndex;
    }

    void RefreshQuerySlots()
    {
        for (int i = 0; i < slotTexts.Count; i++)
        {
            int queryIndex = activeQueryIndex + i;

            if (queryIndex < currentQueries.Count)
            {
                SetSlotContent(slotTexts[i], currentQueries[queryIndex], queryIndex);
                SetTextAlpha(slotTexts[i], 1f);

                // Χρωματισμός ενεργής query (πάντα πρώτο slot)
                slotTexts[i].fontMaterial = new Material(slotTexts[i].fontMaterial);
                Color color = (i == 0) ? Color.green : Color.gray;
                slotTexts[i].fontMaterial.SetColor(ShaderUtilities.ID_FaceColor, color);
            }
            else
            {
                slotTexts[i].text = "";
                SetTextAlpha(slotTexts[i], 0f);
            }
        }

        selectedCells.Clear();
        UnhighlightAllCells();
    }

    public void CheckCell(CellInteraction cell)
    {
        if (isFinished)
        return;   
        int level = Managers.Mission.curLevel;
        if (activeQueryIndex >= currentQueries.Count)
            return;

        var currentText = slotTexts[0]; // Πάντα ενεργό slot είναι το πρώτο
        if (!slotToQueryIndex.TryGetValue(currentText, out int queryIndex))
            return;

        var correctCells = currentQueries[queryIndex].correctCells;

        if (correctCells.Contains(cell))
        {
            cell.SetSprite(highlightSprite);
            selectedCells.Add(cell);

            if (selectedCells.Count == correctCells.Count)
            {
                StartCoroutine(HandleQuerySuccess(currentText));
                GameLogger.Log("QUIZ_ATTEMPT",
                new QuizAttemptData(currentText.text, "success"), level);
                ScoreManager.Instance.AddScore("QUIZ_ATTEMPT", 50);
                Messenger<int>.Broadcast(GameEvent.SCORE_FEEDBACK, 50);
            }
        }
        else
        {
            StartCoroutine(cell.FlashWrong(wrongSprite, defaultSprite, 2.5f));
            GameLogger.Log("QUIZ_ATTEMPT",
            new QuizAttemptData(currentText.text, "fail"), level);
            ScoreManager.Instance.AddScore("QUIZ_ATTEMPT", -10);
            Messenger<int>.Broadcast(GameEvent.SCORE_FEEDBACK, -10);
        }
    }

    IEnumerator HandleQuerySuccess(TextMeshProUGUI completedText)
    {
        Messenger.Broadcast("RIGHT_ANSWER");
        Messenger<string, bool>.Broadcast(GameEvent.SHOW_MESSAGE, "Success!", false);
       //completedText.color = Color.black;

        yield return new WaitForSeconds(0.6f);

        activeQueryIndex++;

        if (activeQueryIndex >= currentQueries.Count)
        {
            yield return new WaitForSeconds(0.6f);
            bool isLastGroup = currentGroupIndex == queryGroups.Count - 1;

            if (isLastGroup)
            {
                Messenger.Broadcast(GameEvent.SHOW_LAST_HOLO);
            }
            ActivateNextGroup();
        }
        else
        {
            RefreshQuerySlots();
        }
    }

    void SetTextAlpha(TextMeshProUGUI text, float alpha)
    {
        Color c = text.color;
        text.color = new Color(c.r, c.g, c.b, alpha);
    }

    void UnhighlightAllCells()
    {
        if (currentGroupIndex >= 0 && currentGroupIndex < queryGroups.Count)
        {
            var currentTable = queryGroups[currentGroupIndex].table;
            var allCells = currentTable.GetComponentsInChildren<CellInteraction>();
            foreach (var cell in allCells)
            {
                cell.SetSprite(defaultSprite);
            }
        }
    }
    
}
