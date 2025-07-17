using UnityEngine;
using TMPro;
using System.Collections;

public class StepController : MonoBehaviour
{
    [Header("Step Objects")] 
    [SerializeField] private GameObject rentalsTable;
    [SerializeField] private GameObject bookCloneCube;
    [SerializeField] private GameObject memberCloneCube;
    [SerializeField] private GameObject bookIdCube;
    [SerializeField] private GameObject memberIdCube;
    [SerializeField] private GameObject bookIdClone;
    [SerializeField] private GameObject memberIdClone;
    [SerializeField] private GameObject bookIdValueCube;
    [SerializeField] private GameObject memberIdValueCube;

    [Header("Target Positions")]
    [SerializeField] private Transform bookTargetPosition;
    [SerializeField] private Transform memberTargetPosition;

    [Header("Canvas Movement")]
    [SerializeField] private Transform bookCanvas;
    [SerializeField] private Transform memberCanvas;
    [SerializeField] private Transform bookCanvasNewPos;
    [SerializeField] private Transform memberCanvasNewPos;

    [Header("Text Display")]
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private string[] stepTexts;

    private int currentStep = 0;

    private void Start()
    {
        if (stepTexts != null && stepTexts.Length > 0)
        {
            displayText.text = stepTexts[0];
        }
    }

    public void Interact(Transform caller)
    {
        if (currentStep >= stepTexts.Length) return;

        displayText.text = stepTexts[currentStep];

        switch (currentStep)
        {
            case 1:
                rentalsTable.SetActive(true);
                break;
            case 2:
                bookCloneCube.SetActive(true);
                memberCloneCube.SetActive(true);
                StartCoroutine(MoveToTarget(bookCloneCube.transform, bookTargetPosition.position));
                StartCoroutine(MoveToTarget(memberCloneCube.transform, memberTargetPosition.position));
                break;
            case 3:
                StartCoroutine(FlashAndDisappear(bookCloneCube));
                StartCoroutine(FlashAndDisappear(memberCloneCube));
                break;
            case 4:
                StartCoroutine(MoveCanvas(bookCanvas, bookCanvasNewPos.position));
                StartCoroutine(MoveCanvas(memberCanvas, memberCanvasNewPos.position));
                bookIdCube.SetActive(true);
                memberIdCube.SetActive(true);
                bookIdValueCube.SetActive(true);
                memberIdValueCube.SetActive(true);
                break;
            case 6:
                bookIdClone.SetActive(true);
                memberIdClone.SetActive(true);
                StartCoroutine(MoveToTarget(bookIdClone.transform, bookTargetPosition.position));
                StartCoroutine(MoveToTarget(memberIdClone.transform, memberTargetPosition.position));
                break;
            case 7:
            Debug.Log("👉 STEP_COMPLETED fired");
                Messenger.Broadcast(GameEvent.STEP_COMPLETED);
                StartCoroutine(FlashGreen(bookIdClone));
                StartCoroutine(FlashGreen(memberIdClone));
                break;
        }

        currentStep++;
    }

    private IEnumerator MoveToTarget(Transform obj, Vector3 target)
    {
        float time = 0f;
        Vector3 start = obj.position;
        while (time < 1f)
        {
            time += Time.deltaTime * 1.5f;
            obj.position = Vector3.Lerp(start, target, time);
            yield return null;
        }
    }

    private IEnumerator MoveCanvas(Transform obj, Vector3 target)
    {
        float time = 0f;
        Vector3 start = obj.position;
        while (time < 1f)
        {
            time += Time.deltaTime;
            obj.position = Vector3.Lerp(start, target, time);
            yield return null;
        }
    }

    private IEnumerator FlashAndDisappear(GameObject obj)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        Color original = rend.material.color;

        for (int i = 0; i < 3; i++)
        {
            rend.material.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            rend.material.color = original;
            yield return new WaitForSeconds(0.2f);
        }

        obj.SetActive(false);
    }

    private IEnumerator FlashGreen(GameObject obj)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        Color original = rend.material.color;

        for (int i = 0; i < 3; i++)
        {
            rend.material.color = Color.green;
            yield return new WaitForSeconds(0.2f);
            rend.material.color = original;
            yield return new WaitForSeconds(0.2f);
        }
    }
}
