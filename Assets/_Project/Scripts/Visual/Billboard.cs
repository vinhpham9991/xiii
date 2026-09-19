using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCam != null)
        {
            // Face the camera perfectly (to keep 2D aspect ratio)
            transform.forward = mainCam.transform.forward;
        }
    }
}
