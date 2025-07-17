using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [SerializeField] Transform target;          // Το target (ο παίκτης)
    public float rotSpeed = 2.0f;               // Ταχύτητα περιστροφής
    private float rotY;                         // Γωνία περιστροφής γύρω από τον παίκτη (Y)
    private float rotX;                         // Γωνία περιστροφής πάνω-κάτω (X)
    private Vector3 thirdPersonOffset;          // Offset για την 3rd person
    private bool isFirstPerson = false;         // Ελέγχει αν η κάμερα είναι σε 1st person ή 3rd person
    private bool isCursorLocked = true;
    
    public float thirdPersonDistance = 4f;      // Απόσταση της κάμερας στην 3rd person
    public float thirdPersonHeight = 2f;        // Ύψος της κάμερας στην 3rd person
    public Vector3 firstPersonOffset = new Vector3(0, 1.6f, 0);  // Offset για την 1st person (κεφάλι παίκτη)

    void Start()
    {
        rotY = transform.eulerAngles.y;
        rotX = 0f;
        thirdPersonOffset = new Vector3(0, thirdPersonHeight, -thirdPersonDistance);
    }

    void Update()
    {
        if (Managers.GameState.IsPaused|| Managers.GameState.IsChatOpen)
        {
            if (isCursorLocked)
            {
                isCursorLocked = !isCursorLocked;
                LockCursor(isCursorLocked);
            }
            return;
        }
        // Εναλλαγή μεταξύ 1st και 3rd person
            if (Input.GetKeyDown(KeyCode.C))
            {
                isFirstPerson = !isFirstPerson;
                if (Messenger.HasListener("CAMERA_TOGGLE"))
                {
                    Messenger.Broadcast("CAMERA_TOGGLE");
                }
            }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isCursorLocked = !isCursorLocked;
            LockCursor(isCursorLocked);
            if (Messenger.HasListener("CURSOR_LOCK"))
            {
                Messenger.Broadcast("CURSOR_LOCK");
            }
        }

        // 🔹 Αν το ποντίκι είναι ξεκλείδωτο, δεν κουνάμε την κάμερα
        if (!isCursorLocked) return;

        // Λαμβάνουμε την είσοδο του ποντικιού για περιστροφή
        float mouseX = Input.GetAxis("Mouse X") * rotSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotSpeed;

        rotY += mouseX;  // Περιστροφή γύρω από τον παίκτη (Y άξονας)

        if (isFirstPerson)
        {
            rotX -= mouseY;  // Περιστροφή πάνω-κάτω στην 1st person
            rotX = Mathf.Clamp(rotX, -80f, 80f);  // Περιορίζουμε την κάμερα να μην γυρίζει ανάποδα
        }
    }

    void LateUpdate()
    {
        if (isFirstPerson)
        {
            // 1st Person: Η κάμερα είναι στο κεφάλι του παίκτη και περιστρέφεται ελεύθερα με το ποντίκι
            transform.position = target.position + firstPersonOffset;
            transform.rotation = Quaternion.Euler(rotX, rotY, 0);
        }
        else
        {
            // 3rd Person: Η κάμερα είναι πίσω και πάνω από τον παίκτη
            Quaternion rotation = Quaternion.Euler(0, rotY, 0);
            transform.position = target.position + rotation * thirdPersonOffset;
            transform.LookAt(target.position + Vector3.up * 1.5f);
        }
    }

    public bool IsFirstPersonView()
    {
        return isFirstPerson;
    }
    

    private void LockCursor(bool lockCursor)
    {
        if (Managers.GameState.IsPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockCursor;
    }

}
