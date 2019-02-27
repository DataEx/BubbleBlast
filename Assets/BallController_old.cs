using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BallController_Old : MonoBehaviour
{
    static int counter = 0;

    public float movementSpeed = 2.5f;
    private Rigidbody rb;
    private Color color;

    public List<BallController_Old> connectedBalls;

    bool isHittingRoof = false;
    public bool isConnected;

    private void Awake()
    {
        connectedBalls = new List<BallController_Old>();

        rb = GetComponent<Rigidbody>();
        rb.AddForce(this.transform.forward * Time.deltaTime * movementSpeed);

        this.name = "Ball: " + counter;
        counter++;

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
        // Bounce off wall
        if (colliderTag == "Wall")
        {
            Vector3 normal = collision.contacts[0].normal;
            Vector3 originalVelocity = rb.velocity;
            Vector3 reflectionForce = Vector3.Reflect(this.transform.forward, normal);
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(reflectionForce * Time.deltaTime * movementSpeed);
            this.transform.forward = reflectionForce;
        }
        // Stick to object
        else
        {
            // 
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if(colliderTag == "Roof")
        {
            isHittingRoof = true;
        }

        BallController_Old hitBall = collision.gameObject.GetComponent<BallController_Old>();
        if (hitBall)
        {
            this.AddBall(hitBall);
            //GameController.AddToQueue(this);
        }


    }

    public void AddBall(BallController_Old ball)
    {
        connectedBalls.Add(ball);
    }

    public HashSet<BallController_Old> GetNeighbors(HashSet<BallController_Old> neighbors)
    {
        neighbors.Add(this);
        foreach (BallController_Old ball in connectedBalls)
        {
            if (!neighbors.Contains(ball))
            {
                neighbors = ball.GetNeighbors(neighbors);
            }
        }

        return neighbors;
    }

    public bool IsTherePathToCeiling(HashSet<BallController_Old> checkedBalls)
    {
        if (isHittingRoof)
            return true;

        checkedBalls.Add(this);
        foreach (BallController_Old ball in connectedBalls)
        {
            if (ball.isHittingRoof)
            {
                return true;
            }

            if (!checkedBalls.Contains(ball))
            {
                if(ball.IsTherePathToCeiling(checkedBalls))
                {
                    return true;
                }
            }

        }

        return false;
    }


    public HashSet<BallController_Old> AddNeighbors(HashSet<BallController_Old> neighbors, HashSet<BallController_Old> chain)
    {
        foreach (BallController_Old ball in connectedBalls)
        {
            if(!chain.Contains(ball))
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
        foreach(BallController_Old ball in connectedBalls)
        {
            ball.connectedBalls.Remove(this);
        }
    }

    public HashSet<BallController_Old> ConnectChain(HashSet<BallController_Old> chain)
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

    public List<BallController_Old> GetConnectedBalls()
    {
        return connectedBalls;
    }

    public Color GetColor()
    {
        return color;
    }
}

// TODO: create arc-predcition to show where you're going to hit