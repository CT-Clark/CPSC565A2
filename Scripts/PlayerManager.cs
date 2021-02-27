using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    #region Initialization

    /// <summary>
    /// Executes once on awake.
    /// </summary>
    public void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        aggressiveness = UnityEngine.Random.Range(19, 37);
        maxExhaustion = UnityEngine.Random.Range(35, 78);
        maxVelocity = UnityEngine.Random.Range(14, 20);
        weight = UnityEngine.Random.Range(63, 107);
        underDogValue = UnityEngine.Random.Range(5f, 10f);
    }

    void Start()
    {
        // Colour the players
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material.color = playerColor;

        // Set up teams references for easy access
        team = GameObject.Find(teamText + "Spawn").GetComponent<TeamManager>();
        if (teamText == "Gryffindor")
        {
            otherTeam = GameObject.Find("SlytherinSpawn").GetComponent<TeamManager>();
        }
        else if (teamText == "Slytherin")
        {
            otherTeam = GameObject.Find("GryffindorSpawn").GetComponent<TeamManager>();
        }

        Rigidbody.mass = weight;
    }

    #endregion

    #region Field/Properties

    private Rigidbody Rigidbody; // Reference to this player's Rigidbody component
    public TeamManager team; // Reference to the team this player belongs to
    public TeamManager otherTeam;
    private GameObject snitch; // Reference to hold the snitch to figure out where to move
    private Transform target;
    private float collisionAvoidanceRadiusThreshold = 5; // Radius to calculate collision avoidance
    private bool recovering = false;
    public bool spawning = false;
    public Color playerColor;
    public Vector3 spawnPoint; // Point this individual player spawned at to respawn at
    public string teamText;
    public GameObject teamObject;
    public bool unconscious = false;
    public double aggressiveness;
    public float maxExhaustion;
    public float maxVelocity;
    public float weight;
    public float currentExhaustion = 0;
    public float underDogValue;
    [HideInInspector] public double collisionValue;
    System.Random rnd = new System.Random();
    private PlayerManager collidedPlayer;

    //private PlayerMovement Movement;       
    private GameObject canvasGameObject;

    #endregion

    #region Methods

    /// <summary>
    /// A fixed update for calculating physics engine results, not tied to framerate.
    /// </summary>
    private void FixedUpdate()
    {
        // Adjust the rigidbodies position and orientation in FixedUpdate.
        if (!unconscious)
        {
            // Recover from exhaustion
            if (currentExhaustion > maxExhaustion - 1f && !recovering)
            {
                recovering = true;
                Rigidbody.velocity = Vector3.zero;
                Rigidbody.angularVelocity = Vector3.zero;
            } 
            else if (recovering)
            {
                currentExhaustion -= 0.1f;
                if (currentExhaustion < maxExhaustion / 2) // Get back down to half maxExhaustion
                    recovering = false;
            }
            else if (spawning) // Spawn delay
            {
                if (UnityEngine.Random.Range(0, 1000) < 10)
                    spawning = false;
            }
            else // If not exhausted, recovering from exhaustion, or respawning... move
            {
                Move();
            }
        }
        else // If unconscious fall towards the ground and then respawn
        {
            Rigidbody.useGravity = true;
            if (transform.position.y < 1.5f)
            {
                transform.position = spawnPoint;
                Rigidbody.velocity = Vector3.zero;
                Rigidbody.angularVelocity = Vector3.zero;
                unconscious = false;
                Rigidbody.useGravity = false;
                spawning = true;
            }
        }
    }

    /// <summary>
    /// Is called after update. Used to implement collision value checking to ensure both parties have calculated their values when asked.
    /// </summary>
    void LateUpdate()
    {
        if (collidedPlayer != null && !collidedPlayer.unconscious)
        {
            unconscious = false;
            if (collisionValue < collidedPlayer.collisionValue)
            {
                unconscious = true;
            }
            if (teamText == collidedPlayer.teamText && UnityEngine.Random.Range(1, 100) > 5)
            {
                unconscious = false;
            }
        }
        collidedPlayer = null;
    }

    /// <summary>
    /// Decides where and how to move.
    /// </summary>
    private void Move()
    {
        snitch = GameObject.Find("Snitch");
        target = snitch.transform;

        Vector3 velocity = Rigidbody.velocity;
        Vector3 acceleration = Vector3.zero;

        // Change the acceleration
        acceleration += NormalizeSteeringForce(ComputeSnitchAttraction(target)) * 5; // Attraction to the snitch
        acceleration += NormalizeSteeringForce(ComputeCollisionAvoidanceForce()) * 2;   // Repel from other players force
        acceleration += NormalizeSteeringForce(GroundAvoidanceForce()) * 5;


        acceleration /= Rigidbody.mass / 30f; // Heavier objects accelerate more slowly
        velocity += acceleration * Time.deltaTime;
        velocity = velocity.normalized * Mathf.Clamp(velocity.magnitude, 0, otherTeam.points > team.points ? maxVelocity + underDogValue : maxVelocity);
        Rigidbody.velocity = velocity;
        transform.forward = Rigidbody.velocity.normalized;

        transform.up = Rigidbody.velocity;

        // Increase exhaustion levels
        currentExhaustion += 0.01f * (otherTeam.points > team.points ? underDogValue/3f : 1f);
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
        //Debug.Log(otherTeam.points > team.points ? "Underdog maxVelocity: " + maxVelocity + underDogValue : "Not underdog maxVelocity: " + maxVelocity);
        return force.normalized * Mathf.Clamp(force.magnitude, 0, otherTeam.points > team.points ? maxVelocity + underDogValue : maxVelocity);
        
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

        return force - tempForce;
    }

    /// <summary>
    /// Computes the force that helps avoid collision with the ground
    /// </summary>
    private Vector3 GroundAvoidanceForce()
    {
        Vector3 force = transform.position;
        int layerMask = 1 << 9;

        // Check if heading to collision straight ahead
        if (!Physics.SphereCast(transform.position,
            collisionAvoidanceRadiusThreshold,
            transform.forward,
            out RaycastHit hitInfo,
            collisionAvoidanceRadiusThreshold, layerMask))
        {
            return Vector3.zero;
        }

        return transform.position - hitInfo.point;
    }

    /// <summary>
    /// What to do when the object detects a collision.
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Player") // What to do when you collide w/ a player
        {
            collidedPlayer = collision.collider.GetComponent<PlayerManager>();
            collisionValue = aggressiveness * (rnd.NextDouble() * (1.2 - 0.8) + 0.8) * (1 - currentExhaustion / maxExhaustion);
        }
        else if (collision.gameObject.name == "Snitch") // What to do when you catch a snitch
        {
            if (team.lastPointScored == true)
            {
                team.points += 2;
            }
            else
            {
                team.points += 1;
            }
            team.lastPointScored = true;

            otherTeam.lastPointScored = false;

            // New snitch location
            collision.gameObject.GetComponent<Transform>().position = new Vector3(
                UnityEngine.Random.Range(-100, 100), 
                UnityEngine.Random.Range(0, 100), 
                UnityEngine.Random.Range(-100, 100)
                );
        }
        else // What to do if you collide with the environment
        {
            unconscious = true;
        }
    }
    #endregion
}
