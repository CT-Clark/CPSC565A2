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
    }

    /// <summary>
    /// Run before the first frame. Used to assign fields
    /// </summary>
    void Start()
    {
        // Colour the players
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material.color = playerColor;

        // Set up teams references for easy access
        team = GameObject.Find(teamText + "Spawn").GetComponent<TeamManager>();
        if (teamText == "Gryffindor") { otherTeam = GameObject.Find("SlytherinSpawn").GetComponent<TeamManager>(); }
        else if (teamText == "Slytherin") { otherTeam = GameObject.Find("GryffindorSpawn").GetComponent<TeamManager>(); }
 
        // Set the player's attributes
        if (teamText == "Gryffindor") {
            weight = BoxMuller(75f, 12f);
            maxVelocity = BoxMuller(18f, 2f);
            aggressiveness = BoxMuller(22f, 3f);
            maxExhaustion = BoxMuller(65f, 13f);
            underDogValue = BoxMuller(10f, 2f);
            nearCaptainBonus = BoxMuller(1.2f, 0.05f);
            captainDetectionRadius = BoxMuller(8f, 3f);
        }
        else // Team is Slytherin
        {
            weight = BoxMuller(85f, 17f);
            maxVelocity = BoxMuller(16f, 2f);
            aggressiveness = BoxMuller(30f, 7f);
            maxExhaustion = BoxMuller(50f, 15f);
            underDogValue = BoxMuller(7f, 1f);
            nearCaptainBonus = BoxMuller(1.1f, 0.05f);
            captainDetectionRadius = BoxMuller(10f, 5f);
        }

        Rigidbody.mass = weight;
    }

    #endregion

    #region Field/Properties

    private Rigidbody Rigidbody; // Reference to this player's Rigidbody component
    private GameObject snitch; // Reference to hold the snitch to figure out where to move
    private Transform target;
    private float collisionAvoidanceRadiusThreshold = 5; // Radius to calculate collision avoidance
    private bool recovering = false;
    private bool spawning = false;
    public TeamManager team; // Reference to the team this player belongs to
    public TeamManager otherTeam;
    public bool captain = false; // Whether or not this player is the team captain
    public bool nearCaptain = false;
    public float nearCaptainBonus;
    public float captainDetectionRadius;
    public Color playerColor; // This player's teamcolour
    public Vector3 spawnPoint; // Point this individual player spawned at to respawn at
    public string teamText; // Name of the team this player belongs to
    public bool unconscious = false;
    public double aggressiveness; // Affects collision outcomes
    public float maxExhaustion; // How long the player can move before becoming exhausted
    public float maxVelocity; // Max speed
    public float weight; // Affects collision outcomes and acceleration
    public float currentExhaustion = 0;
    public float underDogValue; // The amount to increase this player's maxVelocity if the other team is 3+ points ahead
    [HideInInspector] public double collisionValue; // Used in collision calculations
    System.Random rnd = new System.Random();
    private PlayerManager collidedPlayer; // Used in collision calculations     

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
            // Recover from exhaustion if you're just about to fall unconscious
            if (currentExhaustion > maxExhaustion - 1f && !recovering) 
            {
                recovering = true;
                Rigidbody.velocity = Vector3.zero;
                Rigidbody.angularVelocity = Vector3.zero;
            } 
            else if (recovering)
            {
                currentExhaustion -= 0.1f; // Faster to recover than to play
                if (currentExhaustion < maxExhaustion / 2) // Get back down to half maxExhaustion
                    recovering = false;
            }
            else if (spawning) // Spawn delay
            {
                if (UnityEngine.Random.Range(0, 1000) < 10)
                {
                    spawning = false;
                    currentExhaustion = 0f; // Reset the exhaustion (They've "rested")
                }
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
        float nearCaptainValue = CheckIfNearCaptain() ? nearCaptainBonus : 1.0f;

        // Change the acceleration
        acceleration += NormalizeSteeringForce(ComputeSnitchAttraction(target), nearCaptainValue); // Attraction to the snitch
        acceleration += NormalizeSteeringForce(ComputeCollisionAvoidanceForce(), nearCaptainValue);   // Repel from other players force
        acceleration += NormalizeSteeringForce(GroundAvoidanceForce(), nearCaptainValue) * 5;


        acceleration /= Rigidbody.mass / 75f; // Heavier objects accelerate more slowly
        velocity += acceleration * Time.deltaTime;
        velocity = velocity.normalized * Mathf.Clamp(velocity.magnitude, 0, (otherTeam.points > team.points + 2 ? maxVelocity + underDogValue : maxVelocity) * nearCaptainValue);
        Rigidbody.velocity = velocity;
        transform.up = Rigidbody.velocity.normalized; // Orient the model the proper way

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
    private Vector3 NormalizeSteeringForce(Vector3 force, float nearCaptainValue)
    {
        return force.normalized * Mathf.Clamp(force.magnitude, 0, (otherTeam.points > team.points + 2 ? (maxVelocity + underDogValue) : maxVelocity) * nearCaptainValue);
    }

    /// <summary>
    /// Computer the attraction force towards the snitch
    /// </summary>
    /// <param name="target"></param>
    /// <returns>Vector3</returns>
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
    /// Checks to see if this player is near enough to their captain to benefit
    /// </summary>
    /// <returns>Whether this player is near the captain</returns>
    private bool CheckIfNearCaptain()
    {
        bool result = false;
        // Look around within a certain radius for other players to avoid collisions
        Collider[] colliders = Physics.OverlapSphere(transform.position, captainDetectionRadius);
        if (colliders.Length > 0 && !captain) // If you're the captain don't gain the bonus
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                PlayerManager pm = colliders[i].GetComponent<PlayerManager>();
                if (pm)
                {
                    if (pm.captain && pm.teamText == teamText)
                        result = true;
                }
            }
        }

        nearCaptain = result;
        return result;
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
            if (team.lastPointScored == true) { team.points += 2; }
            else { team.points += 1; }

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

    /// <summary>
    /// Generates a random number pulled from a normal distribution using the Box Muller method
    /// In this case x is the mean and s the variance
    /// </summary>
    /// <param name="x"></param>
    /// <param name="s"></param>
    /// <returns></returns>
    private float BoxMuller(float x, float s)
    {
        float result = 0;
        while (result == 0f)
        {
            float u = UnityEngine.Random.Range(0.0f, 1.0f);
            float v = UnityEngine.Random.Range(0.0f, 1.0f);

            result = Mathf.Sqrt(-2 * Mathf.Log(u)) * Mathf.Cos(2 * Mathf.PI * v);
        }

        result *= s;
        result += x;
        return result;
    }
    #endregion
}
