using UnityEngine;
using System.Collections.Generic;

public class BookDropZone : MonoBehaviour
{
    [System.Serializable]
    public class BookData
    {
        public string bookName;
        public Transform snapPosition;
        public GameObject detailRow;
    }

    [SerializeField] private List<BookData> booksData;
    [SerializeField] private int totalBooksToPlace = 4;

    private HashSet<string> placedBooks = new HashSet<string>();
    private int placedCount = 0;

    


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"BookDropZone: Trigger entered by {other.name}");

        DragAndDropCube dragCube = other.GetComponent<DragAndDropCube>();
        if (dragCube != null && !dragCube.IsPlacedCorrectly())
        {
            string name = dragCube.gameObject.name;
            Debug.Log($"BookDropZone: DragCube detected: {name}");

            foreach (BookData data in booksData)
            {
                Debug.Log($"Checking if '{name}' contains '{data.bookName}'");

                if (name.Contains(data.bookName))
                {
                    Debug.Log($"✔ Match found for book: {data.bookName}");

                    other.transform.position = data.snapPosition.position;

                    if (data.detailRow != null)
                    {
                        data.detailRow.SetActive(true);
                        Debug.Log($"Detail row activated for {data.bookName}");
                    }

                    dragCube.MarkAsPlaced();

                    if (!placedBooks.Contains(data.bookName))
                    {
                        placedBooks.Add(data.bookName);
                        placedCount++;
                        int level = Managers.Mission.curLevel;
                        Debug.Log($"📚 Book placed: {data.bookName} ({placedCount}/{totalBooksToPlace})");
                        GameLogger.Log("BOOK_PLACED",
                        new BookPlacedData(data.bookName, true, placedCount), level);

                        if (placedCount >= totalBooksToPlace)
                        {
                            Debug.Log(" Όλα τα βιβλία τοποθετήθηκαν σωστά!");
                            Messenger.Broadcast(GameEvent.ALL_BOOKS_PLACED);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"⚠ Book '{data.bookName}' έχει ήδη τοποθετηθεί.");
                    }

                    return;
                }
            }

            Debug.LogWarning($"❌ BookDropZone: Δεν βρέθηκε αντιστοίχιση για '{name}'");
        }
        else
        {
            Debug.Log($"BookDropZone: Either no DragAndDropCube OR already placed: {other.name}");
        }
    }
}
