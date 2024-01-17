using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class Elevator : NetworkBehaviour
{
    public float targetY;
    public float speed = 4.0f;
    //public float waitTime = 3.0f;

    private float originalY;
    private bool movingToTarget = true;

    void Start()
    {
        originalY = transform.position.y;
        //StartCoroutine(Move());
    }

    private void Update()
    {
        if (IsServer)
        {
            if (movingToTarget == true)
            {
                if (transform.position.y <= targetY)
                {
                    transform.Translate(Vector3.up * Time.deltaTime * speed);
                }
                else
                {
                    transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
                    movingToTarget = false;
                }
            }
            else if (movingToTarget == false)
            {
                if (transform.position.y >= originalY)
                {
                    transform.Translate(-Vector3.up * Time.deltaTime * speed);
                }
                else
                {
                    transform.position = new Vector3(transform.position.x, originalY, transform.position.z);
                    movingToTarget = true;
                }
            }
        }
    }
}
