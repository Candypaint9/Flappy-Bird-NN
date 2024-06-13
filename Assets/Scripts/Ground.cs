using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : MonoBehaviour
{
    public GameObject otherGround;

    public bool hasMovedOtherGround = false;

    public float moveSpeed;

    private void Update()
    {
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        if (!hasMovedOtherGround && transform.position.x <= -1)
        {
            otherGround.transform.position = transform.position + Vector3.right * 21;   //21 is size of sprite
            otherGround.GetComponent<Ground>().hasMovedOtherGround = false;

            hasMovedOtherGround = true;
        }
    }
}
