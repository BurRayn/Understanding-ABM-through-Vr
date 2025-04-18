using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidBehavior : MonoBehaviour
{
    public Vector3 minBounds;
    public Vector3 maxBounds;
    public Vector3 velocity;
    public float speed = 5.0f;
    public float rotationSpeed = 5.0f; 

    private float cohesionWeight = 1f;
    private float alignmentWeight = 1f;
    private float separationWeight = 1f;

    void OnEnable()
    {
        // Subscribe to FlockManager events
        FlockManager.OnCohesionChanged += UpdateCohesionWeight;
        FlockManager.OnAlignmentChanged += UpdateAlignmentWeight;
        FlockManager.OnSeparationChanged += UpdateSeparationWeight;
    }

    void OnDisable()
    {
        // Unsubscribe from FlockManager events
        FlockManager.OnCohesionChanged -= UpdateCohesionWeight;
        FlockManager.OnAlignmentChanged -= UpdateAlignmentWeight;
        FlockManager.OnSeparationChanged -= UpdateSeparationWeight;
    }

    void Start()
    {
        // Initialize velocity with a random direction and speed
        velocity = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * speed;
    }

    void Update()
    {
        // Calculate movement vectors based on the current weights
        Vector3 cohesionVector = CalculateCohesion() * cohesionWeight;
        Vector3 alignmentVector = CalculateAlignment() * alignmentWeight;
        Vector3 separationVector = CalculateSeparation() * separationWeight;

        // Combine the movement vectors to adjust the boid's velocity
        Vector3 movement = cohesionVector + alignmentVector + separationVector;

        // Apply the movement to velocity
        velocity += movement * Time.deltaTime;
        velocity = velocity.normalized * speed;

        // Update the boid's position
        transform.position += velocity * Time.deltaTime;

        // Ensure the boid wraps around the set bounds
        WrapAround();
        // Rotate the boid to face the direction of movement
        if (velocity != Vector3.zero)
        {
            // Calculate target rotation based on the velocity vector
            Quaternion targetRotation = Quaternion.LookRotation(velocity);

            // Smoothly interpolate from the current rotation to the target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // Update the cohesion weight based on the event from FlockManager
    void UpdateCohesionWeight(float newWeight) => cohesionWeight = newWeight;

    // Update the alignment weight based on the event from FlockManager
    void UpdateAlignmentWeight(float newWeight) => alignmentWeight = newWeight;

    // Update the separation weight based on the event from FlockManager
    void UpdateSeparationWeight(float newWeight) => separationWeight = newWeight;

    // Calculate the cohesion vector towards the center of nearby boids
    Vector3 CalculateCohesion()
    {
        Vector3 centerMass = Vector3.zero;
        int count = 0;

        Collider[] nearbyBoids = Physics.OverlapSphere(transform.position, 5.0f);

        foreach (Collider boid in nearbyBoids)
        {
            if (boid.gameObject != this.gameObject && boid.CompareTag("Boid"))
            {
                centerMass += boid.transform.position;
                count++;
            }
        }

        if (count > 0)
        {
            centerMass /= count;
            return (centerMass - transform.position).normalized;
        }
        return Vector3.zero;
    }

    // Calculate the alignment vector to match the velocity of nearby boids
    Vector3 CalculateAlignment()
    {
        Vector3 averageVelocity = Vector3.zero;
        int count = 0;

        Collider[] nearbyBoids = Physics.OverlapSphere(transform.position, 5.0f);

        foreach (Collider boid in nearbyBoids)
        {
            if (boid.gameObject != this.gameObject && boid.CompareTag("Boid"))
            {
                BoidBehavior otherBoid = boid.GetComponent<BoidBehavior>();
                if (otherBoid != null)
                {
                    averageVelocity += otherBoid.velocity;
                    count++;
                }
            }
        }

        if (count > 0)
        {
            averageVelocity /= count;
            return (averageVelocity - velocity).normalized;
        }
        return Vector3.zero;
    }

    // Calculate the separation vector to avoid crowding nearby boids
    Vector3 CalculateSeparation()
    {
        Vector3 separationVector = Vector3.zero;
        int count = 0;

        Collider[] nearbyBoids = Physics.OverlapSphere(transform.position, 2.0f);

        foreach (Collider boid in nearbyBoids)
        {
            if (boid.gameObject != this.gameObject && boid.CompareTag("Boid"))
            {
                separationVector += (transform.position - boid.transform.position);
                count++;
            }
        }

        if (count > 0)
        {
            separationVector /= count;
            return separationVector.normalized;
        }
        return Vector3.zero;
    }

    // Wrap around the environment bounds like Pacman
    void WrapAround()
    {
        if (transform.position.x > maxBounds.x)
        {
            transform.position = new Vector3(minBounds.x, transform.position.y, transform.position.z);
        }
        else if (transform.position.x < minBounds.x)
        {
            transform.position = new Vector3(maxBounds.x, transform.position.y, transform.position.z);
        }

        if (transform.position.y > maxBounds.y)
        {
            transform.position = new Vector3(transform.position.x, minBounds.y, transform.position.z);
        }
        else if (transform.position.y < minBounds.y)
        {
            transform.position = new Vector3(transform.position.x, maxBounds.y, transform.position.z);
        }

        if (transform.position.z > maxBounds.z)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, minBounds.z);
        }
        else if (transform.position.z < minBounds.z)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, maxBounds.z);
        }
    }
}
