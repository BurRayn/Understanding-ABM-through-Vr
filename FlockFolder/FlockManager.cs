using UnityEngine;
using UnityEngine.UI;
using System;

public class FlockManager : MonoBehaviour
{
    [Range(0, 100)] public float cohesionWeight = 1f;
    [Range(0, 100)] public float alignmentWeight = 1f;
    [Range(0, 100)] public float separationWeight = 1f;

    public Slider cohesionSlider;
    public Slider alignmentSlider;
    public Slider separationSlider;

    // Events to notify BoidBehavior of changes
    public static event Action<float> OnCohesionChanged;
    public static event Action<float> OnAlignmentChanged;
    public static event Action<float> OnSeparationChanged;

    void Start()
    {
        // Initialize slider values to match initial weights
        cohesionSlider.value = cohesionWeight;
        alignmentSlider.value = alignmentWeight;
        separationSlider.value = separationWeight;

        // Add listeners to update values dynamically and trigger events
        cohesionSlider.onValueChanged.AddListener(UpdateCohesion);
        alignmentSlider.onValueChanged.AddListener(UpdateAlignment);
        separationSlider.onValueChanged.AddListener(UpdateSeparation);
    }

    void UpdateCohesion(float value)
    {
        cohesionWeight = value;
        OnCohesionChanged?.Invoke(value); // Trigger the event
    }

    void UpdateAlignment(float value)
    {
        alignmentWeight = value;
        OnAlignmentChanged?.Invoke(value); // Trigger the event
    }

    void UpdateSeparation(float value)
    {
        separationWeight = value;
        OnSeparationChanged?.Invoke(value); // Trigger the event
    }
}
