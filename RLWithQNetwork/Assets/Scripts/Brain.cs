using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    private float reward = 0.0f;
    private List<double> replayMemory = new List<double>();

    private int mCapacity = 1000;

    private float discount = 0.99f;

    private float exploreRate = 100.0f;

    private float maxExploreRate = 100.0f;

    private float minExploreRate = 0.01f;

    private float exploreDecay = 0.001f;

    private Vector3 ballStartPos;

    private int failCount = 0;

    private float tillSpeed = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
