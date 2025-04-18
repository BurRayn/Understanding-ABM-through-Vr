using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;  // if you use the new Input System
using UnityEngine.XR;

public class CameraToggle : MonoBehaviour
{
    // Reference to your Cinemachine virtual camera (Bird POV)
    public CinemachineVirtualCamera povCam;

    // Reference to the XR camera (the actual main camera)
    public GameObject xrCameraObject;

    // How high the virtual camera's priority should be when active
    public int activePriority = 10;
    // When inactive, set a lower priority (or you could disable the camera)
    public int inactivePriority = 0;

    // Keep a reference to the XR tracking component (usually TrackedPoseDriver)
    private UnityEngine.SpatialTracking.TrackedPoseDriver trackedPoseDriver;

    private bool isPOVActive = false;

    void Start()
    {
        // Try to get the TrackedPoseDriver on the XR camera object
        if (xrCameraObject != null)
        {
            trackedPoseDriver = xrCameraObject.GetComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>();
        }
        // Ensure the bird POV camera starts with a low priority
        if (povCam != null)
        {
            povCam.Priority = inactivePriority;
        }
    }

    // Call this method from your UI button's OnClick event.
    public void TogglePOV()
    {
        isPOVActive = !isPOVActive;
        if (isPOVActive)
        {
            // Activate the bird POV by raising the virtual camera's priority.
            povCam.Priority = activePriority;
            // Disable XR head tracking so that Cinemachine can override the view.
            if (trackedPoseDriver != null)
            {
                trackedPoseDriver.enabled = false;
            }
        }
        else
        {
            // Deactivate bird POV by lowering its priority.
            povCam.Priority = inactivePriority;
            // Re-enable XR head tracking.
            if (trackedPoseDriver != null)
            {
                trackedPoseDriver.enabled = true;
            }
        }
    }
}
