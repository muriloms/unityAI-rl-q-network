using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallState : MonoBehaviour
{

    public bool droppe = false;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "drop")
        {
            droppe = true;
        }
    }
}
