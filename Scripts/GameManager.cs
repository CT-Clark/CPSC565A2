using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Complete
{
    public class GameManager : MonoBehaviour
    {
        public int m_PointsToWin = 100;             // The number of points a team needs to win the game.
        public float m_StartDelay = 3f;             // The delay between the start of RoundStarting and RoundPlaying phases.
        public float m_EndDelay = 3f;               // The delay between the end of RoundPlaying and RoundEnding phases.
        //public CameraControl m_CameraControl;       // Reference to the CameraControl script for control during different phases.
        public Text m_MessageText;                  // Reference to the overlay Text to display
        public GameObject m_Player;                 // Reference to the players.
        public PlayerManager[] m_Players;           // A collection of managers for enabling and disabling different aspects of the game.
        public TeamManager[] m_Teams;

        private int m_RoundNumber;                  // Which round the game is currently on.
        private WaitForSeconds m_StartWait;         // Used to have a delay whilst the round starts.
        private WaitForSeconds m_EndWait;           // Used to have a delay whilst the round or game ends.
        private TeamManager m_RoundWinner;          // Reference to the winner of the current round.  Used to make an announcement of who won.
        private TeamManager m_GameWinner;           // Reference to the winner of the game.  Used to make an announcement of who won.


        private void Start()
        {
            // Create the delays so they only have to be made once.
            m_StartWait = new WaitForSeconds(m_StartDelay);
            m_EndWait = new WaitForSeconds(m_EndDelay);

            SpawnAllTeams();

            // Once the Players have been created and the camera is using them as targets, start the game.
            StartCoroutine(GameLoop());
        }


        private void SpawnAllTeams()
        {
            // For all the Teams...
            for (int i = 0; i < m_Teams.Length; i++)
            {
                m_Teams[i].Setup();
            }
        }


        // This is called from start and will run each phase of the game one after another.
        private IEnumerator GameLoop()
        {
            // Start off by running the 'RoundStarting' coroutine but don't return until it's finished.
            yield return StartCoroutine(RoundStarting());

            // Once the 'RoundStarting' coroutine is finished, run the 'RoundPlaying' coroutine but don't return until it's finished.
            yield return StartCoroutine(RoundPlaying());

            // Once execution has returned here, run the 'RoundEnding' coroutine, again don't return until it's finished.
            yield return StartCoroutine(RoundEnding());

            // This code is not run until 'RoundEnding' has finished.  At which point, check if a game winner has been found.
            if (m_GameWinner != null)
            {
                // If there is a game winner, restart the level.
                //SceneManager.LoadScene(0);
            }
            else
            {
                // If there isn't a winner yet, restart this coroutine so the loop continues.
                // Note that this coroutine doesn't yield.  This means that the current version of the GameLoop will end.
                StartCoroutine(GameLoop());
            }
        }


        private IEnumerator RoundStarting()
        {
            // As soon as the round starts reset the Players and make sure they can't move.
            ResetAllPlayers();

            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_StartWait;
        }


        // Handles the period of time after the round starts and before the round ends
        private IEnumerator RoundPlaying()
        {
            // Clear the text from the screen.
            m_MessageText.text = string.Empty;
            return null;
        }

        // What happens when the round ends
        private IEnumerator RoundEnding()
        {
            // Clear the winner from the previous round.
            m_RoundWinner = null;

            // See if there is a winner now the round is over.
            m_RoundWinner = GetRoundWinner();

            // Now the winner's score has been incremented, see if someone has one the game.
            m_GameWinner = GetGameWinner();

            // Get a message based on the scores and whether or not there is a game winner and display it.
            string message = EndMessage();
            m_MessageText.text = message;

            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_EndWait;
        }


        private TeamManager GetRoundWinner()
        {
            //continue;
            return null;
        }


        // This function is to find out if there is a winner of the game.
        private TeamManager GetGameWinner()
        {
            // Go through all the Teams...
            for (int i = 0; i < m_Teams.Length; i++)
            {
                // ... and if one of them has enough points to win the game, return it.
                if (m_Teams[i].m_Points == m_PointsToWin)
                    return m_Teams[i];
            }

            // If no Teams have enough rounds to win, return null.
            return null;
        }


        // Returns a string message to display at the end of each round.
        private string EndMessage()
        {
            // By default when a round ends there are no winners so the default end message is a draw.
            string message = "DRAW!";

            // If there is a winner then change the message to reflect that.
            if (m_RoundWinner != null)
                message = m_RoundWinner.m_TeamText + " WINS THE ROUND!";

            // Add some line breaks after the initial message.
            message += "\n\n\n\n";

            /*
            // Go through all the Players and add each of their scores to the message.
            for (int i = 0; i < m_Players.Length; i++)
            {
                message += m_Players[i].m_ColoredPlayerText + ": " + m_Players[i].m_Wins + " WINS\n";
            }
            */

            // If there is a game winner, change the entire message to reflect that.
            if (m_GameWinner != null)
                message = m_GameWinner.m_TeamText + " WINS THE GAME!";

            return message;
        }


        // This function is used to turn all the Players back on and reset their positions and properties.
        private void ResetAllPlayers()
        {
            for (int i = 0; i < m_Players.Length; i++)
            {
                m_Players[i].Reset();
            }
        }
    }
}
