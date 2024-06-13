using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreInc : MonoBehaviour
{
    bool scoreIncDone = false;


    private void Update()
    {
        if (!scoreIncDone && transform.position.x <= 0)
        {
            scoreIncDone = true;

            GameObject.Find("GameManager").GetComponent<Manager>().score++;
        }
    }
}
