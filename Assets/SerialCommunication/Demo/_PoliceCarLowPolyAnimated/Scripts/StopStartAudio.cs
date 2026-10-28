
using UnityEngine;

public class StopStartAudio : MonoBehaviour
{
        AudioSource audioSource;
        public AudioClip otherClip;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        //audioSource.Play();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Hit")) 
		{
            audioSource.clip = otherClip;
            audioSource.Play();
            audioSource.loop = false;
		}
     
            
    }

}
