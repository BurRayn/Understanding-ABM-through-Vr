using UnityEngine;

public class PanelManager : MonoBehaviour
{
    // Reference to the Main Panel and Minimized Panel
    public GameObject MainPanel;
    public GameObject MinimizedPanel;

    // Method to minimize the MainPanel and show the MinimizedPanel
    public void Minimize()
    {
        MainPanel.SetActive(false);   // Hide the Main Panel
        MinimizedPanel.SetActive(true); // Show the Minimized Panel
    }

    // Method to maximize the MinimizedPanel and show the MainPanel
    public void Maximize()
    {
        MinimizedPanel.SetActive(false); // Hide the Minimized Panel
        MainPanel.SetActive(true);       // Show the Main Panel
    }
}
