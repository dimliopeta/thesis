using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDropCube : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum InteractionMode { BookDropZone, CubeChallenge }

    private Transform objectTransform;
    private Canvas canvas;
    private Camera mainCamera;

    private Vector3 initialPosition;
    private float initialX;
    private bool isPlacedCorrectly = false;

    [SerializeField] private Transform rightPosition;
    private InteractionMode mode;

    public InteractionMode GetMode() => mode;

    private void Awake()
    {
        objectTransform = transform;
        canvas = GetComponentInChildren<Canvas>();
        mainCamera = Camera.main;
        mode = (rightPosition != null) ? InteractionMode.CubeChallenge : InteractionMode.BookDropZone;
    }

    void Start()
    {
        initialPosition = objectTransform.position;
        initialX = objectTransform.position.x;

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlacedCorrectly) return;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPlacedCorrectly) return;

        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                mainCamera,
                out Vector3 worldPosition))
            {
                worldPosition.x = initialX;
                objectTransform.position = worldPosition;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isPlacedCorrectly) return;
        int level = Managers.Mission.curLevel;

        if (mode == InteractionMode.CubeChallenge)
        {
            if (Vector3.Distance(objectTransform.position, rightPosition.position) < 1f)
            {
                objectTransform.position = rightPosition.position;
                isPlacedCorrectly = true;

                StartCoroutine(FlashGreen());
                GameLogger.Log("CUBE_PLACED", new CubePlacedData(gameObject.name, "correct"), level);
                Messenger.Broadcast(GameEvent.CUBE_CHALLENGE_RIGHT);
                ScoreManager.Instance.AddScore("CUBE_PLACED", 10);
                Messenger<int>.Broadcast(GameEvent.SCORE_FEEDBACK, 10);
                return;
            }

            objectTransform.position = initialPosition;
            GameLogger.Log("CUBE_PLACED", new CubePlacedData(gameObject.name, "wrong"), level);
             ScoreManager.Instance.AddScore("CUBE_PLACED", -10);
            Messenger<int>.Broadcast(GameEvent.SCORE_FEEDBACK, -10);
            StartCoroutine(FlashRed()); // 👈 Προστέθηκε εδώ

        }
    }

    public bool IsPlacedCorrectly() => isPlacedCorrectly;

    public void MarkAsPlaced()
    {
        isPlacedCorrectly = true;
        GetComponent<Renderer>().material.color = Color.green;

        if (mode == InteractionMode.BookDropZone)
        {
            Messenger.Broadcast(GameEvent.RIGHT_ANSWER);
        }
        else
        {
            Messenger.Broadcast(GameEvent.CUBE_CHALLENGE_RIGHT);
        }
    }

    IEnumerator FlashGreen()
    {
        Renderer rend = GetComponent<Renderer>();
        Color originalColor = rend.material.color;
        rend.material.color = Color.green;
        yield return new WaitForSeconds(0.5f);
        rend.material.color = originalColor;
    }

        IEnumerator FlashRed()
    {
        Renderer rend = GetComponent<Renderer>();
        Color originalColor = rend.material.color;
        rend.material.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        rend.material.color = originalColor;
    }
    
}
