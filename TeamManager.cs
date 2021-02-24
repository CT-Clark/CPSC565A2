using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    [HideInInspector] public int m_Wins = 0;
    public int m_Points= 0; // Current number of points
    public string m_TeamText; // Name of the team
    public PlayerManager[] m_Players; // Player objects on the team
    [HideInInspector] public GameObject m_Player; // A reference to a player object
    [HideInInspector] public int m_TeamNumber;

    public void Setup()
    {
        SpawnPlayers();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnPlayers()
    {
        for (int i = 0; i < m_Players.Length; i++)
        {
            m_Players[i].m_Instance = Instantiate(m_Player, m_Players[i].m_SpawnPoint.position, m_Players[i].m_SpawnPoint.rotation) as GameObject;
            m_Players[i].m_PlayerNumber = i + 1;
            m_Players[i].Setup();
        }
    }






}
