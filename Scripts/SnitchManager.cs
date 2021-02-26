using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnitchManager : MonoBehaviour
{
    public Color snitchColor;
    public GameObject instance;
    private float weight = 1;
    private float maxVelocity = 50;
    private Rigidbody Rigidbody;


    public void Setup()
    {
        instance.transform.localScale = Vector3.one * 0.1f;
        MeshRenderer renderer = instance.GetComponent<MeshRenderer>();
        renderer.material.color = snitchColor;
    }

    public void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Update but called independantly of framerate
    void FixedUpdate()
    {
        Move();
    }

    // We need the snitch to move about randomly, but smoothly
    private void Move()
    {
        Vector3 target = Vector3.zero;
        target.Set(Random.Range(-100, 100), Random.Range(0, 100), Random.Range(-100, 100));

        Vector3 velocity = Rigidbody.velocity;
        Vector3 acceleration = Vector3.zero;

        // Change the acceleration
        acceleration += NormalizeSteeringForce(ComputeMovement(target)) * 2; // Attraction to the snitch


        acceleration /= weight / 0.1f; // Heavier objects accelerate more slowly
        velocity += acceleration * Time.deltaTime;
        velocity = velocity.normalized * Mathf.Clamp(velocity.magnitude, 0, maxVelocity);
        Rigidbody.velocity = velocity;
        transform.forward = Rigidbody.velocity.normalized;
    }

    /// <summary>
    /// Normalizes the steering force and clamps it.
    /// </summary>
    private Vector3 NormalizeSteeringForce(Vector3 force)
    {
        return force.normalized * Mathf.Clamp(force.magnitude, 0.5f, maxVelocity);
    }

    private Vector3 ComputeMovement(Vector3 target)
    {
        // Compute force
        return target - transform.localPosition;
    }
}
