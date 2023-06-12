using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    private CustomCollision customCollision;
    private float speed;
    private float jump;
    private int index;
    private bool isFall;
    private void Awake()
    {
        GameObject.Find("triangle").TryGetComponent(out customCollision);
    }
    void Start()
    {
        speed = 5.0f;
        jump = 0;
        isFall = false;
        index = 0;
    }

    void Update()
    {
        float ratio = 0;
        float moveSpeed = speed;
        Vector3 front = transform.position, back = customCollision.Vertics[0];

        if (customCollision.Vertics[index].x > transform.position.x && index > 0)
            --index;
        else if (index + 1 < customCollision.Vertics.Length - 1 && customCollision.Vertics[index + 1].x < transform.position.x)
            ++index;

        if (customCollision.Vertics[index].x      < transform.position.x &&
            customCollision.Vertics[index + 1].x  > transform.position.x)
        {
            front = customCollision.Vertics[index];
            back = customCollision.Vertics[index + 1];

            float height = back.y - front.y;
            ratio = front.y;

            if (height != 0)
            {
                float distance = CustomMath.GetDistance(front, back);
                float intervalX = ((transform.position.x - front.x) / height) * distance;
                float width = ((back.x - front.x) / height) * distance;

                ratio += (intervalX / width) * height;
            }
        }
        
        if (ratio > transform.position.y)
        {
            if (isFall)
            {
                jump = 0;
                isFall = false;
            }

            float angle = CustomMath.ConvertFromRadianToAngle(
                CustomMath.GetFromPositionToRadian(
                    back,
                    front
                    )
                );

            moveSpeed = ((90.0f - Mathf.Abs(angle)) / 90.0f) * speed;
            transform.position = new Vector3(transform.position.x, ratio, transform.position.z);
        }
        else
        {
            if (!isFall)
                isFall = true;

            jump -= 9.8f * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space))
            jump += 2000.0f * Time.deltaTime;

        transform.position += new Vector3(0, jump, 0) * Time.deltaTime;
        transform.position += new Vector3(Input.GetAxis("Horizontal"), 0, 0) * moveSpeed * Time.deltaTime;
    }
}