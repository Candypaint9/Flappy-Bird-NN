using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    [HideInInspector]   public NeuralNetwork net;
    [HideInInspector]   public float fitness;

    [HideInInspector]   public bool crashed;

    [Header("Jump Variables")]
    public float jumpForce;
    public float maxVelocity;
    public Rigidbody2D rb;

    public float timer;
    float currentTime;

    //Inputs
    float[] inputs;
    float yPos;
    float lowerPipePos_1;
    float upperPipePos_1;
    float lowerPipePos_2;
    float upperPipePos_2;
    float xDistToPipe;


    private void Start()
    {
        crashed = false;
    }


    private void Update()
    {
        currentTime += Time.deltaTime;


        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxVelocity);

        if (!crashed)
        {
            fitness += 0.01f;

            GetInputs();
            GetOutpus();
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }


    void GetOutpus()
    {
        float[] output = net.FeedForward(inputs);

        if (output[0] > 0f)
        {
            Jump();
        }
    }


    void GetInputs()
    {
        inputs = new float[6];

        yPos = transform.position.y;

        GameObject closestPipe = FindNextPipe();

        lowerPipePos_1 = closestPipe.transform.GetChild(2).transform.position.y - 0.75f;
        upperPipePos_1 = closestPipe.transform.GetChild(3).transform.position.y - 0.75f;
        
        lowerPipePos_2 = closestPipe.transform.GetChild(2).transform.position.y + 0.75f;
        upperPipePos_2 = closestPipe.transform.GetChild(3).transform.position.y + 0.75f;

        xDistToPipe = closestPipe.transform.position.x - transform.position.x;

        inputs[0] = yPos;
        inputs[1] = lowerPipePos_1;
        inputs[2] = upperPipePos_1;
        inputs[3] = lowerPipePos_2;
        inputs[4] = upperPipePos_2;
        inputs[5] = xDistToPipe;
    }


    public GameObject FindNextPipe()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Pipes");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            if (go.transform.childCount == 4 && go.transform.position.x >= -1.35f)
            {
                Vector3 diff = go.transform.position - position;
                float curDistance = diff.sqrMagnitude;
                if (curDistance < distance)
                {
                    closest = go;
                    distance = curDistance;
                }
            }
        }
        return closest;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Pipes") ||
            collision.gameObject.CompareTag("Boundaries"))
        {
            crashed = true;

            if (collision.gameObject.CompareTag("Boundaries"))
            {
                fitness--;
            }

            net.fitness = fitness;

            Destroy(gameObject);
        } 
    }


    void Jump()
    {
        if (currentTime >= timer)
        {
            currentTime = 0;
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }
}
