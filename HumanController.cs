using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

//Class responsible for human behavior. It deals with its various parameters.
public class HumanController : MonoBehaviour
{
    private NavMeshAgent agent;
    private List<GameObject> threats = new List<GameObject>();

    [Header("Human Settings")]
    public float detectionRange = 15f;
    public float moveSpeed = 3.0f;
    public float cornerDetectionRadius = 2.5f;
    public float cornerEscapeDistance = 5.0f;
    public LayerMask obstacleLayer; 

    private Vector3 lastPosition;
    private float stuckTime = 0f;
    private bool isStuck = false;
    private const float stuckThreshold = 0.5f; // Distance to determine if stuck
    private const float stuckTimeThreshold = 1.0f; // Time to declare as stuck

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }
        agent.speed = moveSpeed;
        lastPosition = transform.position;
    }

    void Start()
    {
        // Find all zombies in the scene on start
        GameObject[] zombieObjects = GameObject.FindGameObjectsWithTag("zombie");
        foreach (GameObject zombie in zombieObjects)
        {
            threats.Add(zombie);
        }
    }

    public void Initialize(List<GameObject> zombieThreats)
    {
        threats.Clear();
        threats.AddRange(zombieThreats);
    }

    void Update()
    {
        // Check if stuck in corner
        CheckIfStuck();

        // If in a corner and zombie is close, try to escape
        if (isStuck && IsZombieNearby())
        {
            EscapeFromCorner();
            return;
        }

        // Normal fleeing behavior
        GameObject closestZombie = FindClosestThreat();
        if (closestZombie != null)
        {
            // Calculates direction away from zombie
            Vector3 fromZombie = transform.position - closestZombie.transform.position;

            // Calculates a point to run to
            Vector3 runToPosition = transform.position + fromZombie.normalized * 10f;

            // Try to find a valid position on NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(runToPosition, out hit, 10f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }

    private void CheckIfStuck()
    {
        // Check if we've moved significantly
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);

        if (distanceMoved < stuckThreshold)
        {
            stuckTime += Time.deltaTime;
            if (stuckTime > stuckTimeThreshold)
            {
                // Additional check - are we near a corner or obstacle?
                if (IsInCorner())
                {
                    isStuck = true;
                }
            }
        }
        else
        {
            // Reset stuck status if we've moved
            stuckTime = 0f;
            isStuck = false;
        }

        lastPosition = transform.position;
    }

    private bool IsInCorner()
    {
        // Cast rays in multiple directions to detect walls/obstacles
        int wallsDetected = 0;
        float rayLength = cornerDetectionRadius;

        // Check in 8 directions
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            if (Physics.Raycast(transform.position, direction, rayLength, obstacleLayer))
            {
                wallsDetected++;
            }
        }

        // If we detect walls in multiple directions, we're probably in a corner
        return wallsDetected >= 3; // Tune this number based on your environment
    }

    private bool IsZombieNearby()
    {
        GameObject closestZombie = FindClosestThreat();
        if (closestZombie != null)
        {
            float distance = Vector3.Distance(transform.position, closestZombie.transform.position);
            return distance < detectionRange;
        }
        return false;
    }

    private void EscapeFromCorner()
    {
        // Find the most open direction
        Vector3 bestEscapeDirection = Vector3.zero;
        float longestDistance = 0f;

        // Check multiple directions
        for (int i = 0; i < 36; i++)
        {
            float angle = i * 10f; // Check every 10 degrees
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, cornerEscapeDistance, obstacleLayer))
            {
                // Found an obstacle in this direction
                if (hit.distance > longestDistance)
                {
                    longestDistance = hit.distance;
                    bestEscapeDirection = direction;
                }
            }
            else
            {
                // No obstacle - this is a good direction!
                longestDistance = cornerEscapeDistance;
                bestEscapeDirection = direction;
                break; // We found a clear path
            }
        }

        // If we found a direction, go that way
        if (bestEscapeDirection != Vector3.zero)
        {
            Vector3 escapePoint = transform.position + bestEscapeDirection * cornerEscapeDistance;

            // Ensure it's on the NavMesh
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(escapePoint, out navHit, cornerEscapeDistance, NavMesh.AllAreas))
            {
                agent.SetDestination(navHit.position);
                Debug.Log("Human escaping from corner to: " + navHit.position);
            }
        }
    }

    GameObject FindClosestThreat()
    {
        if (threats.Count == 0)
            return null;

        float closestDistance = float.MaxValue;
        GameObject closestZombie = null;

        foreach (GameObject zombie in threats)
        {
            if (zombie == null) continue;

            float distance = Vector3.Distance(transform.position, zombie.transform.position);
            if (distance < closestDistance && distance < detectionRange)
            {
                closestDistance = distance;
                closestZombie = zombie;
            }
        }

        return closestZombie;
    }

    public void OnCaught()
    {
        // Human has been caught by a zombie
        Debug.Log("Human caught by zombie: " + gameObject.name);
        // You could trigger an animation, particle effect, or other feedback here

        // Optionally, destroy the human or transform them into a zombie
        //Destroy(gameObject);
    }
}