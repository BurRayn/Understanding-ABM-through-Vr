using UnityEngine;
using System.Collections.Generic;
using Unity.XR.CoreUtils; // Added for XROrigin
using UnityEngine.XR.Interaction.Toolkit; // Added for movement components
using Unity.VisualScripting;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class GameManager : MonoBehaviour
{
    public enum ViewMode { None, Observe, HumanPOV, ZombiePOV, ControlZombie }

    [Header("Prefab References")]
    public GameObject zombiePrefab;
    public GameObject humanPrefab;

    [Header("Spawn Settings")]
    public Transform spawnArea;
    public float spawnRadius = 10f;

    [Header ("PlayerZombie")]
    private GameObject playerControlledZombie;

    [Header("Camera Settings")]
    public Transform defaultCameraPosition;            // Default camera position for observe mode
    public float smoothTransitionTime = 0.5f;          // Time for smooth transition between views
    public Vector3 povPositionOffset = new Vector3(0, 0.2f, 0); // Offset for head position

    private List<GameObject> zombies = new List<GameObject>();
    private List<GameObject> humans = new List<GameObject>();
    private List<Component> movementComponents = new List<Component>();
    private bool movementDisabled = false;
    private bool isTransitioning = false;

    private int zombieCount = 1;
    private int humanCount = 1;
    private ViewMode currentViewMode = ViewMode.None;
    private ViewMode targetViewMode = ViewMode.None;

    // References to persistent objects
    private GameObject remy; // Persistent human
    private GameObject staticZombie; // Persistent zombie

    // Reference to the XR Origin
    private XROrigin xrOrigin;
    private Camera xrCamera;

    // For smooth transitions
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Transform targetParent;
    private float transitionStartTime;

    void Awake()
    {
        // Find the persistent agents by name
        remy = GameObject.Find("Remy"); // Make sure this matches your human's exact name
        staticZombie = GameObject.Find("ZombieI"); // Make sure this matches your zombie's exact name

        // Find XR Origin in the scene
        xrOrigin = FindObjectOfType<XROrigin>();
        if (xrOrigin != null)
        {
            xrCamera = xrOrigin.Camera;
            Debug.Log("Found XR Origin and Camera: " + xrCamera.name);
        }
        else
        {
            Debug.LogError("Could not find XROrigin in the scene. Make sure it exists!");
        }

        // Find movement components
        FindMovementComponents();

        // Add debug logging
        if (remy != null)
            Debug.Log("Found persistent human: " + remy.name);
        else
            Debug.LogError("Could not find persistent human named 'Remy'");

        if (staticZombie != null)
            Debug.Log("Found persistent zombie: " + staticZombie.name);
        else
            Debug.LogError("Could not find persistent zombie named 'Zombie_Static'");
    }

    void Start()
    {
        // Add initial persistent models to lists if they exist
        if (remy != null && !humans.Contains(remy))
        {
            humans.Add(remy);
            remy.tag = "Human";
        }

        if (staticZombie != null && !zombies.Contains(staticZombie))
        {
            zombies.Add(staticZombie);
            staticZombie.tag = "zombie";
        }

        // Initialize the persistent objects with their targets
        if (remy != null)
        {
            HumanController humanController = remy.GetComponent<HumanController>();
            if (humanController != null)
            {
                humanController.Initialize(zombies);
            }
        }

        if (staticZombie != null)
        {
            ZombieController zombieController = staticZombie.GetComponent<ZombieController>();
            if (zombieController != null)
            {
                zombieController.Initialize(humans);
            }
        }
    }

    void Update()
    {
        // Handle smooth transitions between view modes
        if (isTransitioning)
        {
            float elapsed = Time.time - transitionStartTime;
            float t = Mathf.Clamp01(elapsed / smoothTransitionTime);

            // Apply smooth interpolation
            xrOrigin.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            xrOrigin.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            // When transition is complete
            if (t >= 1.0f)
            {
                isTransitioning = false;

                // Set the final parent after the transition
                if (targetParent != null)
                {
                    xrOrigin.transform.SetParent(targetParent);

                    // Ensure local scale is preserved
                    xrOrigin.transform.localScale = Vector3.one;
                }

                currentViewMode = targetViewMode;
                Debug.Log("View transition completed to " + currentViewMode);
            }
        }
    }

    private void FindMovementComponents()
    {
        // Clear previous list
        movementComponents.Clear();

        // Find common XR movement components
        movementComponents.AddRange(FindObjectsOfType<ActionBasedContinuousMoveProvider>());
        movementComponents.AddRange(FindObjectsOfType<ActionBasedContinuousTurnProvider>());
        movementComponents.AddRange(FindObjectsOfType<ActionBasedSnapTurnProvider>());
        movementComponents.AddRange(FindObjectsOfType<TeleportationProvider>());
        movementComponents.AddRange(FindObjectsOfType<CharacterController>());

        // Add any custom movement scripts you have - replace with your actual class names
        // Example: movementComponents.AddRange(FindObjectsOfType<YourCustomPlayerMovement>());

        Debug.Log($"Found {movementComponents.Count} movement components to manage");
    }

    private void SetUserMovementEnabled(bool enabled)
    {
        foreach (Component component in movementComponents)
        {
            if (component is MonoBehaviour mono)
            {
                mono.enabled = enabled;
            }
            else if (component is CharacterController cc)
            {
                cc.enabled = enabled;
            }
            // Add other component types as needed
        }

        movementDisabled = !enabled;
        Debug.Log($"User movement is now {(enabled ? "enabled" : "disabled")}");
    }

    public void SetZombieCount(int count)
    {
        zombieCount = count;
    }

    public void SetHumanCount(int count)
    {
        humanCount = count;
    }

    public void StartSimulation(int zombieNum, int humanNum, ViewMode viewMode)
    {
        // Reset any existing simulation
        ResetSimulation();

        // Set parameters
        zombieCount = zombieNum;
        humanCount = humanNum;
        targetViewMode = viewMode;

        // Spawn agents
        SpawnAgents();

        // If we're in control zombie mode, create or assign a special zombie
        if (viewMode == ViewMode.ControlZombie)
        {
        }

        // Set camera based on view mode
        SetupCameraForViewMode();

        // Manage user movement appropriately
        ManageUserMovement(viewMode);
    }

    // New method to handle player movement controls
    private void ManageUserMovement(ViewMode viewMode)
    {
        // Disable XR movement for POV modes
        if (viewMode == ViewMode.HumanPOV || viewMode == ViewMode.ZombiePOV)
        {
            SetUserMovementEnabled(false);
        }
        // For ControlZombie, we need XR input but not locomotion
        else if (viewMode == ViewMode.ControlZombie)
        {
            SetUserMovementEnabled(false);
            // Any special input handling for zombie control could go here
        }
        // Otherwise enable normal movement
        else
        {
            SetUserMovementEnabled(true);
        }
    }


    private void SpawnAgents()
    {
        // Spawn humans first
        for (int i = 0; i < humanCount; i++)
        {
            Vector3 randomPos = GetRandomPosition();
            GameObject human = Instantiate(humanPrefab, randomPos, Quaternion.identity);
            human.tag = "Human";
            humans.Add(human);
        }

        // Spawn zombies
        for (int i = 0; i < zombieCount; i++)
        {
            Vector3 randomPos = GetRandomPosition();
            GameObject zombie = Instantiate(zombiePrefab, randomPos, Quaternion.identity);
            zombie.tag = "zombie";
            zombies.Add(zombie);

            // Initialize zombie with human targets
            ZombieController zombieController = zombie.GetComponent<ZombieController>();
            if (zombieController != null)
            {
                zombieController.Initialize(humans);
            }
        }

        // Initialize humans with zombie threats
        foreach (GameObject human in humans)
        {
            HumanController humanController = human.GetComponent<HumanController>();
            if (humanController != null)
            {
                humanController.Initialize(zombies);
            }
        }
    }

    private void SetupCameraForViewMode()
    {
        // Check if XR Origin was found
        if (xrOrigin == null)
        {
            Debug.LogError("XR Origin not found! Cannot change camera view.");
            return;
        }

        // Store starting position for smooth transition
        startPosition = xrOrigin.transform.position;
        startRotation = xrOrigin.transform.rotation;
        targetParent = null;  // Default: no parent

        // First reset any previous parenting
        xrOrigin.transform.SetParent(null);

        switch (targetViewMode)
        {
            case ViewMode.Observe:
                // Move XR Origin to the default observation position
                if (defaultCameraPosition != null)
                {
                    targetPosition = defaultCameraPosition.position;
                    targetRotation = defaultCameraPosition.rotation;
                    Debug.Log("Setting XR Origin to Observe position");
                }
                else
                {
                    Debug.LogWarning("Default camera position not set for Observe mode!");
                    targetPosition = xrOrigin.transform.position;
                    targetRotation = xrOrigin.transform.rotation;
                }
                break;

            case ViewMode.HumanPOV:
                // Find human head for POV
                Transform humanHead = null;

                // Try to use Remy first
                if (remy != null)
                {
                    humanHead = FindHeadOrEyes(remy);
                    Debug.Log("Using Remy's head for POV: " + humanHead.name);
                }
                // Fallback to any other human
                else if (humans.Count > 0)
                {
                    humanHead = FindHeadOrEyes(humans[0]);
                    Debug.Log("Using another human's head for POV: " + humanHead.name);
                }

                if (humanHead != null)
                {
                    // Calculate position accounting for XR camera offset
                    targetPosition = humanHead.position - xrCamera.transform.localPosition + povPositionOffset;
                    targetRotation = humanHead.rotation;
                    targetParent = humanHead;

                    Debug.Log("Setting up XR Origin for Human POV");
                }
                else
                {
                    Debug.LogError("No human head found for POV!");
                    // Keep current position
                    targetPosition = xrOrigin.transform.position;
                    targetRotation = xrOrigin.transform.rotation;
                }
                break;

            case ViewMode.ZombiePOV:
                // Find zombie head for POV
                Transform zombieHead = null;

                // Try to use static zombie first
                if (staticZombie != null)
                {
                    zombieHead = FindHeadOrEyes(staticZombie);
                    Debug.Log("Using static zombie's head for POV: " + zombieHead.name);
                }
                // Fallback to any other zombie
                else if (zombies.Count > 0)
                {
                    zombieHead = FindHeadOrEyes(zombies[0]);
                    Debug.Log("Using another zombie's head for POV: " + zombieHead.name);
                }

                if (zombieHead != null)
                {
                    // Calculate position accounting for XR camera offset
                    targetPosition = zombieHead.position - xrCamera.transform.localPosition + povPositionOffset;
                    targetRotation = zombieHead.rotation;
                    targetParent = zombieHead;

                    Debug.Log("Setting up XR Origin for Zombie POV");
                }
                else
                {
                    Debug.LogError("No zombie head found for POV!");
                    // Keep current position
                    targetPosition = xrOrigin.transform.position;
                    targetRotation = xrOrigin.transform.rotation;
                }
                break;

          

            default:
                // Keep current position
                targetPosition = xrOrigin.transform.position;
                targetRotation = xrOrigin.transform.rotation;
                break;
        }

        // Start the transition
        transitionStartTime = Time.time;
        isTransitioning = true;
        Debug.Log("Starting view transition to " + targetViewMode);
    }

    // Helper function to find a suitable transform for camera attachment
    private Transform FindHeadOrEyes(GameObject character)
    {
        // Check if we can find specific named transforms
        Transform[] allTransforms = character.GetComponentsInChildren<Transform>();

        // Look for common names that might be good camera points
        string[] headNames = { "Head", "head", "Eyes", "eyes", "Face", "face" };

        foreach (string name in headNames)
        {
            foreach (Transform t in allTransforms)
            {
                if (t.name.Contains(name))
                    return t;
            }
        }

        // If no special transform found, just use the main transform
        Debug.LogWarning("No head/eye transform found in " + character.name + ". Using main transform.");
        return character.transform;
    }

    private Vector3 GetRandomPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 randomPos = new Vector3(randomCircle.x, 0, randomCircle.y);

        if (spawnArea != null)
            randomPos += spawnArea.position;

        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(randomPos, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
            return hit.position;

        return randomPos;
    }

    public void ResetSimulation()
    {
        // End any transitions
        isTransitioning = false;

        // Remove XROrigin parenting
        if (xrOrigin != null)
        {
            xrOrigin.transform.SetParent(null);
        }

        // Re-enable movement
        SetUserMovementEnabled(true);

        // Destroy all spawned agents EXCEPT our persistent ones
        foreach (GameObject zombie in new List<GameObject>(zombies))
        {
            if (zombie != null && zombie != staticZombie)
                Destroy(zombie);
        }

        foreach (GameObject human in new List<GameObject>(humans))
        {
            if (human != null && human != remy)
                Destroy(human);
        }

        // Clear lists but re-add our persistent objects
        zombies.Clear();
        humans.Clear();

        // Re-add persistent objects to the lists if they exist
        if (staticZombie != null)
            zombies.Add(staticZombie);

        if (remy != null)
            humans.Add(remy);

        // Re-initialize the persistent objects
        if (remy != null)
        {
            HumanController humanController = remy.GetComponent<HumanController>();
            if (humanController != null)
            {
                humanController.Initialize(zombies);
            }
        }

        if (staticZombie != null)
        {
            ZombieController zombieController = staticZombie.GetComponent<ZombieController>();
            if (zombieController != null)
            {
                zombieController.Initialize(humans);
            }
        }

        // Reset view mode
        currentViewMode = ViewMode.None;
        targetViewMode = ViewMode.None;

        // Reset XR Origin position if we have a default position
        if (defaultCameraPosition != null && xrOrigin != null)
        {
            xrOrigin.transform.position = defaultCameraPosition.position;
            xrOrigin.transform.rotation = defaultCameraPosition.rotation;
        }
   

    }
   

}