// This script manages running the game

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Complete
{
    public class GameManager : MonoBehaviour
    {
        public int pointsToWin = 100;             // The number of points a team needs to win the game.
        public Text messageText;                  // Reference to the overlay Text to display
        public GameObject player;                 // Reference to the player object.
        public GameObject snitch;                 // Reference to the snitch object
        public PlayerManager[] players;           
        public TeamManager[] teams;
        public SnitchManager[] snitches;
        private TeamManager roundWinner;          
        private TeamManager gameWinner;    
        
        void Awake()
        {
            SpawnSnitch();
        }

        // Updates tied to framerate
        void Update()
        {
            // If the teams haven't won yet, display the score
            if (teams[0].points < 100 && teams[1].points < 100)
            {
                messageText.text = PlayingMessage();
            }
            else
            {
                gameWinner = GetGameWinner();
                messageText.text = "<color=#" + ColorUtility.ToHtmlStringRGB(gameWinner.teamColor) + ">" + gameWinner.teamText + " WINS THE GAME!</color>";
            }
        }

        // Spawns the golden snitch
        public void SpawnSnitch()
        {
            float xcor, ycor, zcor;
            Vector3 pos;
            xcor = Random.Range(-100, 100);
            ycor = Random.Range(1, 100);
            zcor = Random.Range(-100, 100);
            pos = new Vector3(xcor, ycor, zcor);
            snitches[0].instance = Instantiate(snitch, pos, snitch.transform.rotation) as GameObject;
            snitches[0].Setup();
        }

        /// <summary>
        /// Creates the message used during playing
        /// </summary>
        /// <returns>string msg</returns>
        public string PlayingMessage()
        {
            string msg = "";
            for (int i = 0; i < teams.Length; i++)
            {
                msg += "<color=#" + ColorUtility.ToHtmlStringRGB(teams[i].teamColor) + ">" + teams[i].teamText + ": </color>" + teams[i].points + "\t\t"; 
            }
            return msg;
        }


        // This function is to find out if there is a winner of the game.
        private TeamManager GetGameWinner()
        {
            // Go through all the Teams...
            for (int i = 0; i < teams.Length; i++)
            {
                // ... and if one of them has enough points to win the game, return it.
                if (teams[i].points == pointsToWin)
                    return teams[i];
            }

            return null;
        }
    }
}
