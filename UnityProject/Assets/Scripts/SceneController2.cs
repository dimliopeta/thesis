using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController2 : MonoBehaviour
{
    [SerializeField] private GameObject hologram1;
    [SerializeField] private GameObject querySelectorCanvas;
    [SerializeField] private GameObject hologram2;
    [SerializeField] private GameObject queryImageGame;
    [SerializeField] private GameObject hologram3;
    [SerializeField] private GameObject hologram4;
    [SerializeField] private GameObject columnGame;
    [SerializeField] private GameObject hologram5;

    void OnEnable()
    {
        Messenger.AddListener(GameEvent.STEP_COMPLETED, ShowHologram1);
        Messenger.AddListener(GameEvent.HOLOGRAM_1_COMPLETED, ShowQuerySelector);
        Messenger.AddListener(GameEvent.QUERY_SELECTOR_DONE, ShowHologram2);
        Messenger.AddListener(GameEvent.HOLOGRAM_2_COMPLETED, ShowQueryImageGame);
        Messenger.AddListener(GameEvent.QUERY_IMAGE_GAME_COMPLETED, ShowHologram3);
        Messenger.AddListener(GameEvent.HOLOGRAM_3_COMPLETED, ShowHologram4AndImage);
        Messenger.AddListener(GameEvent.COLUMN_SELECTION_DONE, ShowFinalHologram);
    }

    void OnDisable()
    {
        Messenger.RemoveListener(GameEvent.STEP_COMPLETED, ShowHologram1);
        Messenger.RemoveListener(GameEvent.HOLOGRAM_1_COMPLETED, ShowQuerySelector);
        Messenger.RemoveListener(GameEvent.QUERY_SELECTOR_DONE, ShowHologram2);
        Messenger.RemoveListener(GameEvent.HOLOGRAM_2_COMPLETED, ShowQueryImageGame);
        Messenger.RemoveListener(GameEvent.QUERY_IMAGE_GAME_COMPLETED, ShowHologram3);
        Messenger.RemoveListener(GameEvent.HOLOGRAM_3_COMPLETED, ShowHologram4AndImage);
        Messenger.RemoveListener(GameEvent.COLUMN_SELECTION_DONE, ShowFinalHologram);
    }
    void Start()
    {
        // 🔻 Όλα ξεκινούν απενεργοποιημένα
        hologram1.SetActive(false);
        querySelectorCanvas.SetActive(false);
        hologram2.SetActive(false);
        queryImageGame.SetActive(false);
        hologram3.SetActive(false);
        hologram4.SetActive(false);
        columnGame.SetActive(false);
        hologram5.SetActive(false);
    }

    void ShowHologram1() => hologram1.SetActive(true);
    void ShowQuerySelector() => querySelectorCanvas.SetActive(true);
    void ShowHologram2() => hologram2.SetActive(true);
    void ShowQueryImageGame()
    {
        queryImageGame.SetActive(true);
        var challenges = ChallengeLoader.LoadQueryResultChallenges();
        var json = JsonHelper.ToJson(challenges);
        ChallengeContextHolder.Set("query_result_challenges", json);
        Managers.GameState.SetCheckpoint("query_result_challenges");

    }
    void ShowHologram3() => hologram3.SetActive(true);

    void ShowHologram4AndImage()
    {
        hologram4.SetActive(true);
        columnGame.SetActive(true);
        var challenges = ChallengeLoader.LoadTableMatchingChallenges();
        var json = JsonHelper.ToJson(challenges);
        ChallengeContextHolder.Set("table_matching_challenges", json);
        Managers.GameState.SetCheckpoint("table_matching_challenges");
    }


    void ShowFinalHologram() => hologram5.SetActive(true);
}
