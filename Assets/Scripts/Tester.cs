using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour {

    bool finishedTraveling = false;
    float timeInSec = 5f;
    Rigidbody rb;
    Collider ballCollider;
    BallController bc;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ballCollider = GetComponent<SphereCollider>();
        bc = GetComponent<BallController>();
    }

    public void RunSimulation()
    {
        bc.collisionPoints.Clear();

        float timeToSimulate = timeInSec;
        ballCollider.enabled = true;

        finishedTraveling = false;
        Physics.autoSimulation = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;


        Vector3 forceApplied = this.transform.forward * Time.fixedDeltaTime * 20000;
        //TODO: Remove Time.fixedDeltaTime from force applied. Since we're only applying it once, we don't care about deltaTime

        float startTime = Time.realtimeSinceStartup;

        rb.AddForce(forceApplied);
        while (timeToSimulate >= Time.fixedDeltaTime)
        {
            timeToSimulate -= Time.fixedDeltaTime;
            Physics.Simulate(Time.fixedDeltaTime);

            if (finishedTraveling)
            {
                break;
            }

        }

        float endTime = Time.realtimeSinceStartup;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Physics.autoSimulation = true;


        //TODO: Now that we've figured out where the ball is going to end up, we want to generate the expected path


    }

    public void StopSimulation()
    {
        finishedTraveling = true;
        ballCollider.enabled = false;
    }

    public BallController GetBallController()
    {
        return bc;
    }

}
