using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class RandomFlyer : MonoBehaviour
{
    [SerializeField] float idleSpeed, turnSpeed, switchSeconds;
    [SerializeField] Vector2 moveSpeedMinMax, changeTargetEveryFromTo;
    [SerializeField] Vector2 radiusMinMax, yMinMax;
    [SerializeField] public float randomBaseOffset = 5, delayStart = 0f;

    private Rigidbody body;
    private Vector3 direction;
    private Quaternion lookRotation;

    // Variables for ABM flock model
    public float cohesionWeight = 1f;
    public float alignmentWeight = 1f;
    public float separationWeight = 1f;
    public float neighborRadius = 5f;

    private List<RandomFlyer> neighbors = new List<RandomFlyer>();

    void Start()
    {
        body = GetComponent<Rigidbody>();
        direction = Quaternion.Euler(transform.eulerAngles) * Vector3.forward;
        if (delayStart < 0f) body.velocity = idleSpeed * direction;
    }

    void FixedUpdate()
    {
        if (delayStart > 0f)
        {
            delayStart -= Time.fixedDeltaTime;
            return;
        }

        ApplyFlockBehavior();

        WrapAround();

        body.velocity = idleSpeed * direction;
        body.MovePosition(body.position + body.velocity * Time.fixedDeltaTime);
    }

    void ApplyFlockBehavior()
    {
        Vector3 cohesion = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 separation = Vector3.zero;

        neighbors.Clear();

        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, neighborRadius);
        foreach (Collider col in nearbyObjects)
        {
            RandomFlyer neighbor = col.GetComponent<RandomFlyer>();
            if (neighbor != null && neighbor != this)
            {
                neighbors.Add(neighbor);
            }
        }

        foreach (RandomFlyer neighbor in neighbors)
        {
            cohesion += neighbor.transform.position;
            alignment += neighbor.body.velocity;
            separation += transform.position - neighbor.transform.position;
        }

        if (neighbors.Count > 0)
        {
            cohesion = (cohesion / neighbors.Count - transform.position).normalized * cohesionWeight;
            alignment = (alignment / neighbors.Count).normalized * alignmentWeight;
            separation = (separation / neighbors.Count).normalized * separationWeight;
        }

        Vector3 flockingDirection = cohesion + alignment + separation;
        if (flockingDirection != Vector3.zero)
        {
            lookRotation = Quaternion.LookRotation(flockingDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turnSpeed * Time.fixedDeltaTime);
        }


        direction = transform.forward;
    }

    void WrapAround()
    {
        Vector3 pos = transform.position;

        if (pos.x > radiusMinMax.y) pos.x = radiusMinMax.x;
        else if (pos.x < radiusMinMax.x) pos.x = radiusMinMax.y;

        if (pos.y > yMinMax.y) pos.y = yMinMax.x;
        else if (pos.y < yMinMax.x) pos.y = yMinMax.y;

        if (pos.z > radiusMinMax.y) pos.z = radiusMinMax.x;
        else if (pos.z < radiusMinMax.x) pos.z = radiusMinMax.y;

        transform.position = pos;
    }
}
