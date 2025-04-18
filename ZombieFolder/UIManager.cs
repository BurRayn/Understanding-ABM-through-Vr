using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainPanel;           // Panel with sliders
    public GameObject modeSelectionPanel;  // Panel with Observe/POV buttons
    public GameObject povSelectionPanel;   // Panel with Human/Zombie buttons
    public GameObject minimizedPanel;      // MinimizedPanel

    [Header("UI Controls")]
    public Slider zombieSlider;
    public Slider humanSlider;
    public Button startButton;
    public Button observeButton;
    public Button povViewButton;
    public Button humanPovButton;
    public Button zombiePovButton;
    public Button exitButton;
    public TextMeshProUGUI zombieCountText;
    public TextMeshProUGUI humanCountText;

  
    [Header("Core")]
    public GameObject xrRig; // Add this - reference to your XR Rig
    public GameManager gameManager;


    // Game settings
    private int zombieCount = 1;
    private int humanCount = 1;
    private enum ViewMode { None, Observe, HumanPOV, ZombiePOV }
    private ViewMode currentMode = ViewMode.None;

    void Start()
    {
        // Initialize UI
        InitializeSliders();
        InitializeButtons();

        // Start with only main panel visible
        ShowMainPanelOnly();
    }

    void InitializeSliders()
    {
        if (zombieSlider != null)
        {
            zombieSlider.onValueChanged.AddListener(UpdateZombieCount);
            UpdateZombieCount(zombieSlider.value);
        }

        if (humanSlider != null)
        {
            humanSlider.onValueChanged.AddListener(UpdateHumanCount);
            UpdateHumanCount(humanSlider.value);
        }
    }

    void InitializeButtons()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (observeButton != null)
            observeButton.onClick.AddListener(OnObserveButtonClicked);

        if (povViewButton != null)
            povViewButton.onClick.AddListener(OnPOVViewButtonClicked);

        if (humanPovButton != null)
            humanPovButton.onClick.AddListener(OnHumanPOVButtonClicked);

        if (zombiePovButton != null)
            zombiePovButton.onClick.AddListener(OnZombiePOVButtonClicked);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    // UI Navigation Methods
    void ShowMainPanelOnly()
    {
        mainPanel.SetActive(true);
        modeSelectionPanel.SetActive(false);
        povSelectionPanel.SetActive(false);
    }

    void ShowModeSelectionOnly()
    {
        mainPanel.SetActive(false);
        modeSelectionPanel.SetActive(true);
        povSelectionPanel.SetActive(false);
    }

    void ShowPOVSelectionOnly()
    {
        mainPanel.SetActive(false);
        modeSelectionPanel.SetActive(false);
        povSelectionPanel.SetActive(true);
    }

    // Button Event Handlers
    void OnStartButtonClicked()
    {
        // Move to the mode selection screen
        ShowModeSelectionOnly();
    }

    void OnObserveButtonClicked()
    {
        currentMode = ViewMode.Observe;
        // Start the simulation in observe mode
        if (gameManager != null)
        {
            gameManager.StartSimulation(zombieCount, humanCount, GameManager.ViewMode.Observe);
        }
    }

    void OnPOVViewButtonClicked()
    {
        // Show POV selection screen
        ShowPOVSelectionOnly();
    }

    void OnHumanPOVButtonClicked()
    {
        currentMode = ViewMode.HumanPOV;
        // Start simulation with human POV
        if (gameManager != null)
        {
            gameManager.StartSimulation(zombieCount, humanCount, GameManager.ViewMode.HumanPOV);
        }
    }

    void OnZombiePOVButtonClicked()
    {
        currentMode = ViewMode.ZombiePOV;
        // Start simulation with zombie POV
        if (gameManager != null)
        {
            gameManager.StartSimulation(zombieCount, humanCount, GameManager.ViewMode.ZombiePOV);
        }
    }

    void OnExitButtonClicked()
    {
        // Reset to main menu
        ShowMainPanelOnly();

        // Reset simulation if necessary
        if (gameManager != null)
        {
            gameManager.ResetSimulation();
        }
    }

    // Slider handlers
    void UpdateZombieCount(float value)
    {
        zombieCount = Mathf.RoundToInt(value);

        if (zombieCountText != null)
        {
            zombieCountText.text = "Zombie #: " + zombieCount;
        }

        if (gameManager != null)
        {
            gameManager.SetZombieCount(zombieCount);
        }
    }

    void UpdateHumanCount(float value)
    {
        humanCount = Mathf.RoundToInt(value);

        if (humanCountText != null)
        {
            humanCountText.text = "Human #: " + humanCount;
        }

        if (gameManager != null)
        {
            gameManager.SetHumanCount(humanCount);
        }
    }
    public void OnActAsZombieButtonClicked()
    {
        if (xrRig != null)
        {
            // Set the tag to "zombie"
            xrRig.tag = "zombie";
            Debug.Log("Player is now a zombie! Humans will run away.");

            // Optional: Disable standard button UI and show a "Return to normal" button
            // mainPanel.SetActive(false);
            // zombieControlPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("XR Rig reference is missing!");
        }
    }
    public void Minimize()
    {
        mainPanel.SetActive(false);   // Hide the Main Panel
        minimizedPanel.SetActive(true); // Show the Minimized Panel
    }

    // Method to maximize the MinimizedPanel and show the MainPanel
    public void Maximize()
    {
        minimizedPanel.SetActive(false); // Hide the Minimized Panel
        mainPanel.SetActive(true);       // Show the Main Panel
    }
}
