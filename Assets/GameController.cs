using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour {

    private static Queue<BallController> popBubbleQueue = new Queue<BallController>();
    private const int minChainLength = 3;

    public Camera mainCamera;
    public Cannon cannon;

    private int ballsLanded = 0;
    public int ballsPerDrop = 3;
    public float heightDropped = 1.0f;

    public static BallController LiveBall;

    public static ObjectPool DebugObjectPool;
    public ObjectPool debugObjectPoolReference;

    public static void AddToQueue(BallController ball)
    {
        popBubbleQueue.Enqueue(ball);
    }

    private void Awake()
    {
        BallController.OnBallLanded += DropCeiling;

        DebugObjectPool = debugObjectPoolReference;
    }

    private void DropCeiling()
    {
        ballsLanded++;
        if(ballsLanded % ballsPerDrop == 0)
        {
            mainCamera.transform.position += new Vector3(0, heightDropped, 0);
        }
    }

    private void FixedUpdate()
    {
        if(popBubbleQueue.Count > 0)
        {
            BallController ballToCheck = popBubbleQueue.Dequeue();
            if(ballToCheck && ballToCheck.isActiveAndEnabled)
            {
                CheckForChain(ballToCheck);
            }
        }
    }

    private void CheckForChain(BallController queueBall)
    {
        HashSet<BallController> chain = new HashSet<BallController>();
        chain.Add(queueBall);
        for (int i = 0; i < queueBall.connectedBalls.Count; i++)
        {
            if (queueBall.connectedBalls[i].GetColor() != queueBall.GetColor())
                continue;

            chain = queueBall.ConnectChain(chain);
        }

        if (chain.Count >= minChainLength)
        {
            HashSet<BallController> neighborBalls = new HashSet<BallController>();
            foreach (BallController ball in chain)
            {
                neighborBalls = ball.AddNeighbors(neighborBalls, chain);
                ball.Disconnect();
                ball.GetComponent<Collider>().enabled = false;
                Destroy(ball.gameObject);
            }

            HashSet<BallController> ballsToRemove = new HashSet<BallController>();
            foreach (BallController ball in neighborBalls)
            {
                HashSet<BallController> checkedBalls = new HashSet<BallController>();
                checkedBalls.Add(ball);
                if (!ball.IsTherePathToCeiling(checkedBalls))
                {
                    ballsToRemove.Add(ball);
                }
            }

            BallController[] ballsToRemoveList = ballsToRemove.ToArray();
            foreach (BallController ball in ballsToRemoveList)
            {
                ballsToRemove = ball.GetNeighbors(ballsToRemove);
            }


            foreach (BallController ball in ballsToRemove)
            {
                ball.Drop();
            }

            // Need to recall simulate for Cannon
            cannon.ProjectBallPath();
        }
    }
}
