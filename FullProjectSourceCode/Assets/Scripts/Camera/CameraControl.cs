// This class controls the ability to move the camera during game mode

using UnityEngine;

namespace Complete
{
    public class CameraControl : MonoBehaviour
    {
        Vector3 vec;
        float tiltAroundY = 0f;
        float tiltAroundX = 0f;

        // Update is called once per frame  
        void Update()
        {
            // WASD control horizontal positioning
            // QE control vertical positioning
            // Arrow keys control camera rotation
            if (Input.GetKey(KeyCode.A))
            {
                transform.Translate(-0.1f, 0f, 0f);
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.Translate(0.1f, 0f, 0f);
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.Translate(0.0f, 0f, -0.1f);
            }
            if (Input.GetKey(KeyCode.W))
            {
                transform.Translate(0.0f, 0f, 0.1f);
            }
            if (Input.GetKey(KeyCode.E))
            {
                transform.Translate(0.0f, 0.1f, 0.0f);
            }
            if (Input.GetKey(KeyCode.Q))
            {
                transform.Translate(0.0f, -0.1f, 0.0f);
            }
            if (Input.GetAxis("Vertical") != 0f)
            {
                tiltAroundX -= Input.GetAxis("Vertical");
            }
            if (Input.GetAxis("Horizontal") != 0f)
            {
                tiltAroundY += Input.GetAxis("Horizontal");
            }

            // Rotate the camera
            transform.rotation = Quaternion.Euler(tiltAroundX, tiltAroundY, 0f);
        }
    }
}