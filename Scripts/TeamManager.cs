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

    #endregion

    #region Fields/Properties

    [SerializeField]
    private GameObject playersParent; // Reference to the object to hold all the players, reduces screen cluttering
    [SerializeField]
    private List<PlayerManager> _players; // List of players
    public List<PlayerManager> players { get { return _players; } } // A way to get the players
    public int numberOfPlayers; // Number of players to instantiate
    public Transform spawnTransform;
    [HideInInspector] public int wins = 0; // Whether or not this team has won
    public int points = 0; // Current number of points
    public Color teamColor;
    public string teamText; // Name of the team
    public bool lastPointScored = false;
    public GameObject PlayerTemplate; // Reference to the player prefab
    public GameObject player; // A reference to a player object
    
    #endregion

    #region Methods

    /*
    /// <summary
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {

    }
    */

    /// <summary>
    /// Creates a single player around the spawn's location
    /// </summary>
    private void CreatePlayer()
    {
        if (_players == null)
            _players = new List<PlayerManager>();

        GameObject player = GameObject.Instantiate(PlayerTemplate, playersParent.transform);
        PlayerManager playerScript = player.GetComponent<PlayerManager>();

        // Change the player's location
        float spawnX = spawnTransform.position.x + Random.Range(-10f, 10f);
        float spawnY = 2;
        float spawnZ = spawnTransform.position.z + Random.Range(-10f, 10f);
        player.transform.position = new Vector3(spawnX, spawnY, spawnZ);

        // Assign member variables for the player
        playerScript.spawnPoint = new Vector3(spawnX, spawnY, spawnZ);
        playerScript.playerColor = teamColor;
        playerScript.teamText = teamText;
        player.name = "Player";
    }

    #endregion
}
