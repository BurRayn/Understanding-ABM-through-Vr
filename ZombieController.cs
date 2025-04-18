// For your existing ZombieController.cs script
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;

public class ZombieController : MonoBehaviour
{
    private NavMeshAgent agent;
    private List<GameObject> targets = new List<GameObject>();

    [Header("Zombie Settings")]
    public float detectionRange = 20f;
    public float moveSpeed = 3.5f;
    public float catchDistance = 1.5f;

    [Header("ActZombie")]
    public bool isPlayerControlled = false;
    public float turnSpeed = 120f;


    private GameObject currentTarget;
    private bool hasTarget = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }
        agent.speed = moveSpeed;
    }

    void Start()
    {
        // Find all humans in the scene on start
        GameObject[] humanObjects = GameObject.FindGameObjectsWithTag("Human");
        foreach (GameObject human in humanObjects)
        {
            targets.Add(human);
        }
        FindClosestTarget();
    }

    public void Initialize(List<GameObject> humanTargets)
    {
        UpdateTargets(humanTargets);
    }

    public void UpdateTargets(List<GameObject> humanTargets)
    {
        targets.Clear();
        targets.AddRange(humanTargets);
        FindClosestTarget();
    }

    void Update()
    {
        if (targets.Count == 0)
        {
            // No targets available, try to find some
            GameObject[] humanObjects = GameObject.FindGameObjectsWithTag("Human");
            foreach (GameObject human in humanObjects)
            {
                if (!targets.Contains(human))
                    targets.Add(human);
            }

            if (targets.Count > 0)
                FindClosestTarget();

            return;
        }

        // Check if current target is still valid
        if (currentTarget == null)
        {
            FindClosestTarget();
            return;
        }

        // Move toward target
        agent.SetDestination(currentTarget.transform.position);

        // Check if zombie caught human
        if (Vector3.Distance(transform.position, currentTarget.transform.position) <= catchDistance)
        {
            // Human is caught
            HumanController human = currentTarget.GetComponent<HumanController>();
            if (human != null)
                human.OnCaught();
        }
        if (!isPlayerControlled)
            return;

      
    }

    void FindClosestTarget()
    {
        if (targets.Count == 0)
        {
            hasTarget = false;
            return;
        }

        float closestDistance = float.MaxValue;
        GameObject closestHuman = null;

        foreach (GameObject human in targets)
        {
            if (human == null) continue;

            float distance = Vector3.Distance(transform.position, human.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestHuman = human;
            }
        }

        if (closestHuman != null && closestDistance <= detectionRange)
        {
            currentTarget = closestHuman;
            hasTarget = true;
        }
    }
}