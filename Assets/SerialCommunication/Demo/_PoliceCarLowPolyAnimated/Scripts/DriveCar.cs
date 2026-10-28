using System.Collections;
using UnityEngine;



public class DriveCar : MonoBehaviour
{


    public float speed = 0f;
    public float turnSpeed = 10.0f;
    float verticalInput;

    float horizontalInput;

    // Start is called before the first frame update
    void Start()
    {


    }
    void Update()
    {
        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up * Time.deltaTime * turnSpeed * horizontalInput);

        transform.Translate(Vector3.forward * Time.deltaTime * speed * verticalInput);

    }



}
