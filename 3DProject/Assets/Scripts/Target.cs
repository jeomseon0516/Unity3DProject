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
        transform.position += new Vector3(Input.GetAxis("Horizontal"), 0, 0) * speed * Time.deltaTime;
        float ratio = 0;

        if (customCollision.Vertics[index].x > transform.position.x && index > 0)
            --index;
        else if (index + 1 < customCollision.Vertics.Length - 1 && customCollision.Vertics[index + 1].x < transform.position.x)
            ++index;

        if (customCollision.Vertics[index].x     < transform.position.x &&
            customCollision.Vertics[index + 1].x > transform.position.x)
        {
            float distance = CustomMath.GetDistance(customCollision.Vertics[index], customCollision.Vertics[index + 1]);

            float height = customCollision.Vertics[index + 1].y - customCollision.Vertics[index].y;
            ratio = customCollision.Vertics[index].y;

            if (height != 0)
            {
                float width = ((customCollision.Vertics[index + 1].x - customCollision.Vertics[index].x) / height) * distance;
                float intervalX = ((transform.position.x - customCollision.Vertics[index].x) / height) * distance;

                ratio += (intervalX / width) * height;
            }
        }
        
        if (ratio >= transform.position.y)
        {
            if (isFall)
            {
                jump = 0;
                isFall = false;
            }

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
    }
}