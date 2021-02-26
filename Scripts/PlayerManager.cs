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
    }

    void Start()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material.color = playerColor;
    }

    
    public void Initialize(TeamManager teamScript)
    {
        TeamManager team = teamScript;
    }

    #endregion

    #region Field/Properties

    private Rigidbody Rigidbody;
    private TeamManager team;
    private GameObject snitch;
    private float collisionAvoidanceRadiusThreshold = 10;
    private Transform target;
    public Color playerColor;
    public Transform spawnPoint;
    public string teamText;
    public bool unconscious = false;
    public double aggressiveness;
    public float maxExhaustion;
    public float maxVelocity;
    public float weight;
    public float currentExhaustion = 0;
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
            Move();
        }
        else
        {
            Rigidbody.useGravity = true;
        }
    }

    /// <summary>
    /// Decides where and how to move.
    /// </summary>
    private void Move()
    {
        snitch = GameObject.Find("SnitchTemplate(Clone)");
        target = snitch.transform;

        Vector3 velocity = Rigidbody.velocity;
        Vector3 acceleration = Vector3.zero;

        // Change the acceleration
        acceleration += NormalizeSteeringForce(ComputeSnitchAttraction(target) * 2); // Attraction to the snitch
        acceleration += NormalizeSteeringForce(ComputeCollisionAvoidanceForce());   // Repel from other players force


        acceleration /= weight / 100f; // Heavier objects accelerate more slowly
        velocity += acceleration * Time.deltaTime;
        velocity = velocity.normalized * Mathf.Clamp(velocity.magnitude, 0, maxVelocity);
        Rigidbody.velocity = velocity;
        transform.forward = Rigidbody.velocity.normalized;

        // Increase exhaustion levels
        currentExhaustion += 0.01f;
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
            tempForce += hitInfo.point;
        }

        if (tempForce != Vector3.zero)
        {
            return force - tempForce;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// What to do when the object detects a collision.
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "PlayerTemplate(Clone)")
        {
            collidedPlayer = collision.collider.GetComponent<PlayerManager>();
            Debug.Log("Collision w/ another player");
            collisionValue = aggressiveness * (rnd.NextDouble() * (1.2 - 0.8) + 0.8) * (1 - currentExhaustion / maxExhaustion);
        }
    }

    public double GetCollisionValue()
    {
        return collisionValue;
    }

    public string GetTeamText()
    {
        return teamText;
    }

    /// <summary>
    /// Is called after update. Used to implement collision value checking to ensure both parties have calculated their values when asked.
    /// </summary>
    void LateUpdate()
    {
        if (collidedPlayer != null)
        {
            unconscious = false;
            if (collisionValue < collidedPlayer.GetCollisionValue())
            {
                unconscious = true;
            }
            if (teamText == collidedPlayer.GetTeamText() && UnityEngine.Random.Range(1, 100) > 5)
            {
                unconscious = false;
                Debug.Log("Same team collision");
            }

            Debug.Log("My collision value: " + collisionValue + " | Their collision value: " + collidedPlayer.GetCollisionValue() + " | My state: " + unconscious);
        }
        collidedPlayer = null;
    }



    #endregion
}
