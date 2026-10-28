using UnityEngine;


public class StopStartAnimationOnCollision : MonoBehaviour
{

    private Animator[] animators; // Array to store all animators on the children

    // Start is called before the first frame update
    void Start()
    {

        animators = GetComponentsInChildren<Animator>();// Retrieve all Animator components from the children
        foreach (Animator animator in animators) // Walk through all the Animators
        {
            animator.SetTrigger("Start"); // Activate the trigger on all animators
        }
    }


    void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("Hit"))
        {
            foreach (Animator animator in animators) // Loop through all the Animators
            {
                animator.SetTrigger("Stop"); // Activate the trigger on all animators
            }
        }

    }


}
