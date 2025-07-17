using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    [Header("Table Elements")]
    public List<GameObject> headerBoxes;
    public List<GameObject> bookNameBoxes;
    public List<GameObject> bookDetailRows;

    [Header("Challenge GameObjects")]
    public GameObject membersChallenge;
    public GameObject selectTutorial;
    public GameObject selectMission;
    public GameObject lastHolo;


    [Header("Groups to Reveal")]
    public List<GameObject> cubeGroupsToReveal;

    private int placedCount = 0;
    private int revealedGroups = 0;
    private bool isListeningForCubes = false;

        void Start()
    {
        SetupEventListeners();
    
        // Default αρχικοποίηση χωρίς ResumeManager
        DeactivateList(headerBoxes);
        DeactivateList(bookNameBoxes);
        DeactivateList(bookDetailRows);
        DeactivateList(cubeGroupsToReveal);
    
        if (membersChallenge != null) membersChallenge.SetActive(false);
        if (selectTutorial != null) selectTutorial.SetActive(false);
        if (selectMission != null) selectMission.SetActive(false);
    }



    void SetupEventListeners()
    {
        Messenger.AddListener(GameEvent.TABLE_SHOWUP, ShowTableHeaders);
        Messenger.AddListener(GameEvent.ALL_BOOKS_PLACED, OnAllBooksPlaced);
        Messenger.AddListener(GameEvent.SHOW_SELECT, ShowSelectTutorial);
        Messenger.AddListener(GameEvent.SHOW_TABLE_CHALLENGE, ShowSelectMission);
        Messenger.AddListener(GameEvent.SHOW_LAST_HOLO, ShowLastHolo);

    }

    void OnDestroy()
    {
        Messenger.RemoveListener(GameEvent.TABLE_SHOWUP, ShowTableHeaders);
        Messenger.RemoveListener(GameEvent.ALL_BOOKS_PLACED, OnAllBooksPlaced);
        Messenger.RemoveListener(GameEvent.SHOW_SELECT, ShowSelectTutorial);
        Messenger.RemoveListener(GameEvent.SHOW_TABLE_CHALLENGE, ShowSelectMission);
        Messenger.RemoveListener(GameEvent.SHOW_LAST_HOLO, ShowLastHolo);

        if (isListeningForCubes)
            Messenger.RemoveListener(GameEvent.CUBE_CHALLENGE_RIGHT, OnCubePlacedCorrectly);
    }

    // === Table Related ===
    void ShowTableHeaders()
    {
        ActivateList(headerBoxes);
        ActivateList(bookNameBoxes);
    }

    // === Challenges Related ===
    void OnAllBooksPlaced()
    {
        if (membersChallenge != null)
            membersChallenge.SetActive(true);

        if (!isListeningForCubes)
        {
            Messenger.AddListener(GameEvent.CUBE_CHALLENGE_RIGHT, OnCubePlacedCorrectly);
            isListeningForCubes = true;
        }
    }

    void ShowSelectTutorial()
    {
        if (selectTutorial != null)
            selectTutorial.SetActive(true);
    }
    void ShowSelectMission()
    {
        if (selectMission != null)
            selectMission.SetActive(true);
        var challenges = ChallengeLoader.LoadQueryColorChallenges();
        var json = JsonHelper.ToJson(challenges);
        ChallengeContextHolder.Set("query_color_challenges", json);
        Managers.GameState.SetCheckpoint("query_color_challenges");

    }

    void ShowLastHolo()
    {
        Managers.GameState.ClearCheckpoint(); 
        if (lastHolo != null)
        {
            lastHolo.SetActive(true);
        }
    }

    // === Cube Challenge Progress ===
    void OnCubePlacedCorrectly()
    {
        placedCount++;
        if (placedCount % 3 == 0 && revealedGroups < cubeGroupsToReveal.Count)
        {
            cubeGroupsToReveal[revealedGroups].SetActive(true);
            revealedGroups++;
        }
    }

    void DeactivateList(List<GameObject> list)
    {
        foreach (var obj in list)
            if (obj != null) obj.SetActive(false);
    }

    void ActivateList(List<GameObject> list)
    {
        foreach (var obj in list)
            if (obj != null) obj.SetActive(true);
    }
}
