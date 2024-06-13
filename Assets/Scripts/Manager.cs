using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Manager : MonoBehaviour
{
    [Header("Bird Variables")]
    public GameObject birdObject;
    Vector3 birdStartPos = new Vector3(0, 1.5f, 0);

    [Header("Pipe Variables")]
    public GameObject pipeObject;
    public float pipeSpeed;
    List<GameObject> pipeList = new List<GameObject>();
    public float pipeTimer;

    //UI
    [HideInInspector]   public int generation = 0;
    [HideInInspector]   public int birdsAlive = 0;
    [HideInInspector]   public int score = 0;
    public TextMeshProUGUI genText;
    public TextMeshProUGUI aliveText;
    public TextMeshProUGUI scoreText;

    [Header("Neural Network")]
    public int populationSize;
    public int[] layers = new int[3] {5, 3, 1};
    [Range(1f, 100f)] public float MutationChancePercent;       //chance of mutation
    [Range(0f, 1f)] public float MutationStrength;       //strength of mutation
    public string modelSaveName;

    [Header("Load NN")]
    public bool loadSavedNN;
    public string modelFileName;

    List<NeuralNetwork> networks;
    List<Bird> birds;


    private void Start()
    {
        InitNetworks();
        CreateBirds();

        StartCoroutine(MakePipes());
    }


    private void Update()
    { 
        MovePipes();
        CheckCrash();

        genText.SetText(generation.ToString());
        aliveText.SetText(birdsAlive.ToString());
        scoreText.SetText(score.ToString());
    }

    #region PIPES

    void MovePipes()
    {
        for (int i = 0; i < pipeList.Count; i++)
        {
            GameObject pipe = pipeList[i];
            pipe.transform.position += Vector3.left * pipeSpeed * Time.deltaTime;

            if (pipe.transform.position.x <= -10)
            {
                Destroy(pipe.gameObject);
                pipeList.Remove(pipe);
            }
        }
    }

    public IEnumerator MakePipes()
    {
        while (true)
        {
            Vector3 pos = new Vector3(10, Random.Range(-1.1f, 2.1f), 0);

            pipeList.Add(Instantiate(pipeObject, pos, Quaternion.identity));

            yield return new WaitForSeconds(pipeTimer);
        }
    }

    #endregion


    void CreateBirds()
    {
        birds = new List<Bird>();

        bool mutated = false;

        for (int i = 0; i < populationSize; i++)
        {
            Bird bird = Instantiate(birdObject, birdStartPos, Quaternion.identity).GetComponent<Bird>();
            birds.Add(bird);

            //mutating genes    
            if (!mutated && generation != 0)  //running it only once as it mutates whole array of networks
            {
                MutateNetwork();
                mutated = true;
            }

            bird.net = networks[i];
        }
    }


    public void CheckCrash()
    {
        birdsAlive = GameObject.FindGameObjectsWithTag("Player").Length;

        if (birdsAlive == 0)
        {
            StartCoroutine(EndGeneration());
        }
    }


    IEnumerator EndGeneration()
    {
        yield return new WaitForSeconds(0.5f);

        score = 0;

        StopAllCoroutines();
        CancelInvoke();
        DestroyPipes();
        NewGeneration();
    }


    void NewGeneration()
    {
        birdsAlive = populationSize;
        generation++;

        CreateBirds();
        StartCoroutine(MakePipes());
    }


    void DestroyPipes()
    {
        for (int i = 0; i < pipeList.Count; i++)
        {
            Destroy(pipeList[i].gameObject);
        }

        pipeList.Clear();
    }


    void InitNetworks()
    {
        networks = new List<NeuralNetwork>();
        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            net.Mutate((int)(100 / MutationChancePercent), MutationStrength);

            if (loadSavedNN)
            {
                net.Load("Assets/Models/" + modelFileName + ".txt");
            }

            networks.Add(net);
        }
    }

    void MutateNetwork()
    {
        //fitness is already updated before destroying bird

        networks.Sort();
        networks.Reverse();

        //saving the first network everytime
        networks[0].Save("Assets/Models/" + modelSaveName + ".txt");


        for (int i = 0; i < populationSize; i++)
        {
            //leave the first one as it is
            if (i > 0 && i < populationSize * 0.25)      //if in top 25% mutate itself
            {
                networks[i].Mutate((int)(100 / MutationChancePercent), MutationStrength / 1.25f);
            }

            //if above 25% mutate the top nn to get new ones

            if (i >= populationSize * 0.25 && i < populationSize * 0.50)   
            {
                //copy the first one and mutate
                networks[i] = networks[0].copy(new NeuralNetwork(layers));

                networks[i].Mutate((int)(100 / (MutationChancePercent * 1.5)), MutationStrength);
            }

            if (i >= populationSize * 0.50 && i < populationSize * 0.75) 
            {
                //copy the first one and mutate
                networks[i] = networks[0].copy(new NeuralNetwork(layers));

                networks[i].Mutate((int)(100 / (MutationChancePercent * 2f)), MutationStrength);
            }   
            
            if (i >= populationSize * 0.75 && i < populationSize) 
            {
                //copy the first one and mutate
                networks[i] = networks[0].copy(new NeuralNetwork(layers));

                networks[i].Mutate((int)(100 / (MutationChancePercent * 5f)), MutationStrength);
            }
        }
    }
}
