using UnityEngine;
using UnityEngine.SceneManagement; // nodig om scenes te laden

/// <summary>
/// This script loads a new scene when the player enters a trigger.
/// Attach it to a GameObject with a Collider set as Trigger.
/// </summary>
public class LoadSceneOnTrigger : MonoBehaviour
{
    [Tooltip("Name of the scene to load")]
    public string sceneName;

    [Tooltip("Optional: only load scene if the player enters the trigger")]
    public string playerTag = "Player"; // restrict to certain object tag

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger has the right tag
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag))
            return;

        if (!string.IsNullOrEmpty(sceneName))
        {
            // Load the specified scene
            SceneManager.LoadScene(sceneName);
            Debug.Log("Loading scene: " + sceneName);
        }
        else
        {
            Debug.LogWarning("Scene name is empty!");
        }
    }
}