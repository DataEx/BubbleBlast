using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BallController : MonoBehaviour
{
    static int counter = 0;

    public float movementSpeed = 2.5f;
    private Rigidbody rb;
    private Color color;

    public List<BallController> connectedBalls;

    bool isHittingRoof = false;
    public bool isConnected;

    private Tester tester;
    public bool isFake = false;
    public List<Vector3> collisionPoints;

    bool hasAppliedForce = false;

    public delegate void BallLanded(BallController ball);
    public static event BallLanded OnBallLanded;

    bool isDropping = false;

    private void Awake()
    {
        connectedBalls = new List<BallController>();
        rb = GetComponent<Rigidbody>();
        tester = GetComponent<Tester>();

        if(IsRealBall())
        {
            GameController.LiveBall = this;
            this.name = "Ball: " + counter;
            counter++;
        }
        else
        {
            this.name = "Debug";
            collisionPoints = new List<Vector3>();
        }
    }

    private void FixedUpdate()
    {
        if(!hasAppliedForce && !tester)
        {
            Vector3 forceApplied = this.transform.forward * Time.deltaTime * movementSpeed;
            rb.AddForce(forceApplied);
            hasAppliedForce = true;
        }

        if(isDropping)
        {
            this.transform.position += Vector3.down * 3f * Time.deltaTime;
        }
    }

    public void SetColor(Color ballColor)
    {
        color = ballColor;
        GetComponent<Renderer>().material.color = ballColor;
    }


    //TODO: Fix bug where CheckForChain is being called twice, once on hitter and once on hittee
    private void OnCollisionEnter(Collision collision)
    {
        string colliderTag = collision.gameObject.tag;

        if(rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        Vector3 originalVelocity = rb.velocity;

        // Bounce off wall
        if (colliderTag == "Wall")
        {
            if(tester)
            {
                collisionPoints.Add(this.transform.position);
            }

            Vector3 normal = collision.contacts[0].normal;
            Vector3 reflectionForce = Vector3.Reflect(this.transform.forward, normal);
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(reflectionForce * Time.fixedDeltaTime * movementSpeed);
            this.transform.forward = reflectionForce;
        }
        // Stick to object
        else
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (tester)
            {
                collisionPoints.Add(this.transform.position);
                tester.StopSimulation();
            }

            if (GameController.LiveBall == this)
            {

                Cannon.RealBallInTransit = false;
                OnBallLanded(this);
                GameController.LiveBall = null;
            }
        }


        if (colliderTag == "Roof")
        {
            isHittingRoof = true;
        }
        //TODO: Clean this up with DebugBall and 
        // 1) Destroy all DebugBalls while a real one is in transit
        // 2) Don't spawn any new balls while a real one is in transit

        BallController hitBall = collision.gameObject.GetComponent<BallController>();
        if (hitBall && (hitBall.IsRealBall() && IsRealBall()))
        {
            this.AddBall(hitBall);
            GameController.AddToQueue(this);
        }

    }

    public void AddBall(BallController ball)
    {
        connectedBalls.Add(ball);
    }

    public HashSet<BallController> GetNeighbors(HashSet<BallController> neighbors)
    {
        neighbors.Add(this);
        foreach (BallController ball in connectedBalls)
        {
            if (!neighbors.Contains(ball))
            {
                neighbors = ball.GetNeighbors(neighbors);
            }
        }

        return neighbors;
    }

    public bool IsTherePathToCeiling(HashSet<BallController> checkedBalls)
    {
        if (isHittingRoof)
            return true;

        checkedBalls.Add(this);
        foreach (BallController ball in connectedBalls)
        {
            if (ball.isHittingRoof)
            {
                return true;
            }

            if (!checkedBalls.Contains(ball))
            {
                if (ball.IsTherePathToCeiling(checkedBalls))
                {
                    return true;
                }
            }

        }

        return false;
    }


    public HashSet<BallController> AddNeighbors(HashSet<BallController> neighbors, HashSet<BallController> chain)
    {
        foreach (BallController ball in connectedBalls)
        {
            if (!chain.Contains(ball))
            {
                neighbors.Add(ball);
            }
        }

        return neighbors;
    }

    // Remove destroyed ball from ConnectedBalls
    // If another ball has no connection that links to the ceiling, let it drop
    // If the other ball has no other connected balls and is not hitting the ceiling, let it drop 
    public void Disconnect()
    {
        foreach (BallController ball in connectedBalls)
        {
            ball.connectedBalls.Remove(this);
        }
    }

    public HashSet<BallController> ConnectChain(HashSet<BallController> chain)
    {
        chain.Add(this);
        for (int i = 0; i < connectedBalls.Count; i++)
        {
            if (connectedBalls[i].GetColor() != color)
                continue;
            if (chain.Contains(connectedBalls[i]))
                continue;

            chain = connectedBalls[i].ConnectChain(chain);
        }

        return chain;
    }

    // Fall and then delete self
    public void Drop()
    {
        this.GetComponent<Collider>().enabled = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        isDropping = true;
        StartCoroutine(DropCoroutine());
    }
    IEnumerator DropCoroutine()
    {
        float dropTime = 4f;
        yield return new WaitForSeconds(dropTime);

        Destroy(this.gameObject);
    }


    public List<BallController> GetConnectedBalls()
    {
        return connectedBalls;
    }

    public Color GetColor()
    {
        return color;
    }

    public bool IsRealBall()
    {
        return !isFake;
    }
}

// TODO: Bug where fall balls not being marked as destroyed. One of the balls had a null reference in the Connected array

// TODO: I think I can remove all traces of SelfDestruct (ie isDebug).