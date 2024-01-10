using UnityEngine;
using System.Collections;

public class Elevator : MonoBehaviour
{
    public float targetY;
    public float speed = 4.0f;
    public float waitTime = 3.0f;

    private float originalY;
    private bool movingToTarget = true;

    void Start()
    {
        originalY = transform.position.y;
        StartCoroutine(Move());
    }

    IEnumerator Move()
    {
        while (true)
        {
            float targetPositionY = movingToTarget ? targetY : originalY;
            Vector3 targetPosition = new Vector3(transform.position.x, targetPositionY, transform.position.z);

            while (Mathf.Abs(transform.position.y - targetPositionY) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                yield return null;
            }
            yield return new WaitForSeconds(waitTime);
            movingToTarget = !movingToTarget;
        }
    }
}
