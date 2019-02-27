using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour {

    private static Queue<BallController> popBubbleQueue = new Queue<BallController>();
    private const int minChainLength = 3;

    public Camera mainCamera;
    public Cannon cannon;
    public LevelController levelController;

    private int ballsLanded = 0;
    public int ballsPerDrop = 3;
    public float heightDropped = 1.0f;
    public float maxHeightDropped = 7.0f;
    private float totalHeightDropped = 0f;

    private float GameOverHeight {
        get {
            return cannon.launchLocation.position.y;
        }
    }


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
        BallController.OnBallLanded += CheckBallHeight;

        DebugObjectPool = debugObjectPoolReference;
    }

    private void DropCeiling(BallController ball)
    {
        ballsLanded++;
        if(ballsLanded % ballsPerDrop == 0 && totalHeightDropped < maxHeightDropped)
        {
            mainCamera.transform.position += new Vector3(0, heightDropped, 0);
            totalHeightDropped += heightDropped;

            if (totalHeightDropped > maxHeightDropped) {
                float diff = totalHeightDropped - maxHeightDropped;
                mainCamera.transform.position += new Vector3(0, -diff, 0);
                totalHeightDropped = maxHeightDropped;
            }
        }
    }

    private void CheckBallHeight(BallController ball) {
        if (ball.transform.position.y < GameOverHeight) {
            EndGame();
        }
    }

    private void EndGame() {
        levelController.LoadNextLevel();
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
