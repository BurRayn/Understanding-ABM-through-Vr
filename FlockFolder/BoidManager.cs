using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoidManager : MonoBehaviour
{
    public GameObject birdGroupPrefab;
    public Slider birdCountSlider;
    public List<GameObject> birdGroups = new List<GameObject>();

    public static BoidManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        birdCountSlider.onValueChanged.AddListener(AdjustBirdGroupCount);
        AdjustBirdGroupCount(birdCountSlider.value);
    }

    // Method to adjust the number of bird groups
    public void AdjustBirdGroupCount(float count)
    {
        int targetCount = Mathf.RoundToInt(count);

        while (birdGroups.Count < targetCount)
        {
            SpawnBirdGroup(birdGroupPrefab);
        }

        while (birdGroups.Count > targetCount)
        {
            RemoveBirdGroup(birdGroups[birdGroups.Count-1]);
        }
    }

    // Method to spawn a new group of birds
     public void SpawnBirdGroup(GameObject birdGroupPrefab)
    {
        GameObject newGroup = Instantiate(birdGroupPrefab, GetRandomPosition(), Quaternion.identity);
        birdGroups.Add(newGroup);
    }

    // Method to remove a group of birds
    public void RemoveBirdGroup(GameObject birdGroupPrefab)
    {
        if (birdGroups.Count > 0)
        {
            GameObject groupToRemove = birdGroups[birdGroups.Count - 1];
            birdGroups.Remove(groupToRemove);
            Destroy(groupToRemove);
        }
    }

    // Helper method to get a random spawn position (customize this as needed)
    private Vector3 GetRandomPosition()
    {
        return new Vector3(Random.Range(-5f, 5f), Random.Range(0.5f, 5f), Random.Range(-5f, 5f));
    }
}
