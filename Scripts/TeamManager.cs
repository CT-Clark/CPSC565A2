using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    
    #region Initialization

    /// <summary>
    /// Executes once on awake.
    /// </summary>
    private void Awake()
    {
        spawnTransform = GetComponent<Transform>();

        // Create new players
        for (int i = 0; i < numberOfPlayers; i++)
        {
            CreatePlayer();
        }

    }

    /// <summary>
    /// Generates the players on the team.
    /// </summary>
    /// <param name="numberOfPlayers">The number of players to be generated on this team.</param>
    public void Initialize()
    {
        
    }

    #endregion

    #region Fields/Properties

    [HideInInspector] public int wins = 0;
    public Color teamColor;
    public int points = 0; // Current number of points
    public string teamText; // Name of the team

    [SerializeField]
    public GameObject PlayerTemplate;

    public GameObject player; // A reference to a player object
    [HideInInspector] public int teamNumber;
    private Rigidbody Rigidbody;

    public Transform spawnTransform;

    [SerializeField]
    private GameObject playersParent;

    [SerializeField]
    private List<PlayerManager> _players;
    public List<PlayerManager> players { get { return _players; } }

    public int numberOfPlayers;

    #endregion

    #region Methods

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    /// <summary>
    /// Creates a single player around the spawn location's location
    /// </summary>
    private void CreatePlayer()
    {
        if (_players == null)
            _players = new List<PlayerManager>();

        GameObject player = GameObject.Instantiate(PlayerTemplate, playersParent.transform);

        PlayerManager playerScript = player.GetComponent<PlayerManager>();
        player.transform.position = new Vector3
        (
            spawnTransform.position.x + Random.Range(-10f, 10f),
            0,
            spawnTransform.position.z + Random.Range(-10f, 10f)
        );

        playerScript.spawnPoint = playerScript.transform;
        playerScript.playerColor = teamColor;
        playerScript.teamText = teamText;

        playerScript.Initialize(this);
    }

    #endregion
}
