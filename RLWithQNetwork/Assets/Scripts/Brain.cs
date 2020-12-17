using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Packages.Rider.Editor.UnitTesting;
using Random = UnityEngine.Random;

public class Replay
{
    public List<double> states;
    public double reward;

    public Replay(double xr, double ballz, double ballvx, double r)
    {
        states = new List<double>();
        states.Add(xr);
        states.Add(ballz);
        states.Add(ballvx);
        reward = r;
    }
}
public class Brain : MonoBehaviour
{
    private ANN ann;

    public GameObject ball;

    private float reward = 0.0f;
    private List<Replay> replayMemory = new List<Replay>();

    private int mCapacity = 1000;

    private float discount = 0.99f;

    private float exploreRate = 100.0f;

    private float maxExploreRate = 100.0f;

    private float minExploreRate = 0.01f;

    private float exploreDecay = 0.001f;

    private Vector3 ballStartPos;

    private int failCount = 0;

    private float tillSpeed = 0.5f;

    private float timer = 0;

    private float maxBalanceTime = 0;
    // Start is called before the first frame update
    void Start()
    {
        ann = new ANN(3, 2, 1, 6, 0.2f);
        ballStartPos = ball.transform.position;
        Time.timeScale = 5.0f;
    }
    
    GUIStyle guiStyle = new GUIStyle();

    private void OnGUI()
    {
        guiStyle.fontSize = 25;
        guiStyle.normal.textColor = Color.white;
        GUI.BeginGroup(new Rect(10,10,600,150));
        GUI.Box(new Rect(0, 0, 140, 140), "Stats", guiStyle);
        GUI.Label(new Rect(10, 25, 500, 30), "Fails: " + failCount, guiStyle);
        GUI.Label(new Rect(10, 50, 500, 30), "Decay Rate: " + exploreRate, guiStyle);
        GUI.Label(new Rect(10, 75, 500, 30), "Last Best Balance: " + maxBalanceTime, guiStyle);
        GUI.Label(new Rect(10, 100, 500, 30), "This Balance: " + timer, guiStyle);
        GUI.EndGroup();
    }

    private void FixedUpdate()
    {
        ActiveLearning();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            ResetBall();
        }
    }
    

    private void ResetBall()
    {
        ball.transform.position = ballStartPos;
        ball.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
        ball.GetComponent<Rigidbody>().angularVelocity = new Vector3(0,0,0);
    }

    private void ActiveLearning()
    {
        timer += Time.deltaTime;
        List<double> states = new List<double>();
        List<double> qs = new List<double>();
        
        states.Add(transform.rotation.x);
        states.Add(ball.transform.position.z);
        states.Add(ball.GetComponent<Rigidbody>().angularVelocity.x);

        qs = SoftMax(ann.CalcOutput(states));
        double maxQ = qs.Max();
        int maxQIndex = qs.ToList().IndexOf(maxQ);
        exploreRate = Mathf.Clamp(exploreRate - exploreDecay, minExploreRate, maxExploreRate);

        if (Random.Range(0, 100) < exploreRate)
        {
            maxQIndex = Random.Range(0, 2);
        }

        if (maxQIndex == 0)
        {
            transform.Rotate(Vector3.right, tillSpeed * (float)qs[maxQIndex]);
        }
        else if (maxQIndex == 1)
        {
            transform.Rotate(Vector3.right, -tillSpeed * (float)qs[maxQIndex]);
        }

        if (ball.GetComponent<BallState>().droppe)
        {
            reward = -1.0f;
        }
        else
        {
            reward = 0.1f;
        }
        
        Replay lastMemory = new Replay(transform.rotation.x, ball.transform.position.z, ball.GetComponent<Rigidbody>().angularVelocity.x,reward);

        if (replayMemory.Count > mCapacity)
        {
            replayMemory.RemoveAt(0);
        }
        
        replayMemory.Add(lastMemory);

        if (ball.GetComponent<BallState>().droppe)
        {
            for (int i = replayMemory.Count - 1; i >= 0; i--)
            {
                List<double> toutputsOld = new List<double>();
                List<double> toutputsNew = new List<double>();
                toutputsOld = SoftMax(ann.CalcOutput(replayMemory[i].states));

                double maxQOld = toutputsOld.Max();
                int action = toutputsOld.ToList().IndexOf(maxQOld);

                double feedback;
                if (i == replayMemory.Count - 1 || replayMemory[i].reward == -1)
                {
                    feedback = replayMemory[i].reward;
                }
                else
                {
                    toutputsNew = SoftMax(ann.CalcOutput(replayMemory[i + 1].states));
                    maxQ = toutputsNew.Max();
                    feedback = (replayMemory[i].reward + discount * maxQ);
                }

                toutputsOld[action] = feedback;
                ann.Train(replayMemory[i].states, toutputsOld);
            }

            if (timer > maxBalanceTime)
            {
                maxBalanceTime = timer;
            }

            timer = 0;

            ball.GetComponent<BallState>().droppe = false;
            transform.rotation = Quaternion.identity;
            ResetBall();
            replayMemory.Clear();
            failCount++;
        }
    }

    private List<double> SoftMax(List<double> values)
    {
        double max = values.Max();

        float scale = 0.0f;
        for (int i = 0; i < values.Count; ++i)
        {
            scale += Mathf.Exp((float) (values[i] - max));
        }
        
        List<double> result = new List<double>();
        for (int i = 0; i < values.Count; ++i)
        {
            result.Add(Mathf.Exp((float)(values[i] - max)) / scale);
        }

        return result;
    }
    
}
