using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Color playerColor;
    public string playerTeamText;
    public Transform spawnPoint;
    public string teamText;
    [HideInInspector] public int playerNumber;
    [HideInInspector] public string coloredPlayerText;
    [HideInInspector] public GameObject instance;
    public bool unconscious = false;
    public GameObject snitch;
    private float collisionAvoidanceRadiusThreshold = 5;

    private float aggressiveness;
    private float maxExhaustion = 5f;
    private float maxVelocity = 16f;
    private float weight = 80;
    private float currentExhaustion = 0;
    private Transform target;
    private Rigidbody Rigidbody;
    private int fnum = 0;

    //private PlayerMovement Movement;       
    private GameObject canvasGameObject;

    public void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }

    public void Setup() {
        
        MeshRenderer renderer = instance.GetComponent<MeshRenderer>();
        renderer.material.color = playerColor;

        aggressiveness = UnityEngine.Random.Range(0, 10);
        maxExhaustion = UnityEngine.Random.Range(5, 10);
        maxVelocity = UnityEngine.Random.Range(10, 15);
        weight = UnityEngine.Random.Range(50, 100);
    }

    private void FixedUpdate()
    {
        // Adjust the rigidbodies position and orientation in FixedUpdate.
        if (!unconscious)
        {
            Move();
        }
        else
        {
            Rigidbody.useGravity = true;
            Debug.Log("Unconscious");
        }    
    }


    public void Reset()
    {
        instance.transform.position = spawnPoint.position;
        instance.transform.rotation = spawnPoint.rotation;

    }

    private void Move()
    {
        snitch = GameObject.Find("Snitch(Clone)");
        target = snitch.transform;

        Vector3 velocity = Rigidbody.velocity;
        Vector3 acceleration = Vector3.zero;

        // Change the acceleration
        acceleration += NormalizeSteeringForce(ComputeSnitchAttraction(target)) * 2; // Attraction to the snitch
        acceleration += NormalizeSteeringForce(ComputeCollisionAvoidanceForce());   // Repel from other players force


        acceleration /= weight / 100f; // Heavier objects accelerate more slowly
        velocity += acceleration * Time.deltaTime;
        velocity = velocity.normalized * Mathf.Clamp(velocity.magnitude, 0, maxVelocity);
        Rigidbody.velocity = velocity;
        transform.forward = Rigidbody.velocity.normalized;

        // Increase exhaustion levels
        currentExhaustion += 0.01f;
        Debug.Log("Exhaustion: " + currentExhaustion);
        if (currentExhaustion > maxExhaustion)
        {
            unconscious = true;
        }
    }


    /// <summary>
    /// Normalizes the steering force and clamps it.
    /// </summary>
    private Vector3 NormalizeSteeringForce(Vector3 force)
    {
        return force.normalized * Mathf.Clamp(force.magnitude, 0, maxVelocity);
    }

    private Vector3 ComputeSnitchAttraction(Transform target)
    {
        // Compute force
        return target.position - transform.localPosition;
    }

    /// <summary>
    /// Computes the force that helps avoid collision.
    /// </summary>
    private Vector3 ComputeCollisionAvoidanceForce()
    {
        Vector3 force = transform.position;
        Vector3 tempForce = Vector3.zero;

        // Look around within a certain radius for other players to avoid collisions
        Collider[] colliders = Physics.OverlapSphere(transform.position, collisionAvoidanceRadiusThreshold);
        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();
                if (!targetRigidbody) // Don't worry about non-physics objects
                    continue;
                if (colliders[i].GetComponent<Light>()) // Don't be repulsed from the snitch
                    continue;

                tempForce += targetRigidbody.position;
            }

            tempForce /= colliders.Length;
        }

        // Check if heading to collision straight ahead
        if (Physics.SphereCast(transform.position,
            collisionAvoidanceRadiusThreshold,
            transform.forward,
            out RaycastHit hitInfo,
            collisionAvoidanceRadiusThreshold))
        {
            Debug.Log("~Forward collision detected~");
            tempForce += hitInfo.point;
        }

        if (tempForce != Vector3.zero)
        {
            return force - tempForce;
        }

        return Vector3.zero;
    }
}
