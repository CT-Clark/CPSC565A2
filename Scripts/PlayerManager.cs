using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Color m_PlayerColor;
    public string m_PlayerTeamText;
    public Transform m_SpawnPoint;
    [HideInInspector] public int m_PlayerNumber;
    [HideInInspector] public string m_ColoredPlayerText;
    [HideInInspector] public GameObject m_Instance;
    public float m_Speed = 12f;                 // How fast the player moves.
    public bool tackled;

    private int m_Aggressiveness;
    private int m_MaxExhaustion;
    private int m_MaxVelocity;
    private int m_Weight;
    private int m_CurrentExhaustion;


    //private PlayerMovement m_Movement;       
    private GameObject m_CanvasGameObject;

    public void Setup() {
        
        MeshRenderer renderer = m_Instance.GetComponent<MeshRenderer>();

        renderer.material.color = m_PlayerColor;
    }

    private void FixedUpdate()
    {
        // Adjust the rigidbodies position and orientation in FixedUpdate.
        Move();
    }


    public void Reset()
    {
        m_Instance.transform.position = m_SpawnPoint.position;
        m_Instance.transform.rotation = m_SpawnPoint.rotation;

    }

    private void Move()
    {
        //Vector3 movement = transform.forward * Time.deltaTime;

        // Apply this movement to the rigidbody's position.
        //m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
    }
}
