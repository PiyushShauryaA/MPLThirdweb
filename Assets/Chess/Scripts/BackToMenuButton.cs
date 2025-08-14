using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

namespace CanvasChess
{
    public class BackToMenuButton : MonoBehaviour
    {
        // Call this from the button's OnClick event
        public void OnBackToMenuClicked()
        {
            Debug.Log("[BackToMenuButton] Returning to menu, disconnecting from Photon");
            
            // Disconnect from Photon if connected
            if (PhotonNetwork.IsConnected)
            {
                // Disable automatic scene sync before disconnecting to prevent SetProperties errors
                PhotonNetwork.AutomaticallySyncScene = false;
                PhotonNetwork.Disconnect();
                // Wait a short time before loading the menu to allow disconnect to complete
                StartCoroutine(LoadMenuAfterDelay(0.2f));
            }
            else
            {
                // Load the main menu scene immediately if not connected
                SceneManager.LoadScene("MainMenuMulti");
            }
        }

        private System.Collections.IEnumerator LoadMenuAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene("MainMenuMulti");
        }
    }
} 