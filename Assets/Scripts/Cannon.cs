using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour {

    private readonly float spawnRate = 1.0f;
    private float lastSpawnTime;

    public GameObject ballPrefab;
    public Transform launchLocation;

    public float turnRate = 30f;
    private readonly float minRotation = -60f;
    private readonly float maxRotation = 60f;
    public Transform rotationAxis;
    bool rotatedThisFrame = false;

    public Color[] ballColors;

    public BallController nextBall;

    public GameObject debugPrefab;
    public static bool RealBallInTransit = false;

    public GameObject testerPrefab;
    public Tester testBall = null;

    private List<GameObject> predictivePathBalls = new List<GameObject>();

    private float CurrentRotation
    {
        get
        {
            float rotation = rotationAxis.eulerAngles.z;
            if (rotation > 180)
                rotation -= 360f;
            return rotation;
        }
        set
        {
            Vector3 rotation = rotationAxis.eulerAngles;
            rotation.z = value;
            rotationAxis.eulerAngles = rotation;
            rotatedThisFrame = true;
        }
    }

	// Use this for initialization
	void Awake() {
        lastSpawnTime = -spawnRate;

        nextBall.SetColor(GetRandomBallColor());
    }

    // Update is called once per frame
    void Update () {

        float deltaTime = Time.deltaTime;

        rotatedThisFrame = false;

        // Determine if rotating
        if (Input.GetKey(KeyCode.LeftArrow) && CurrentRotation > minRotation)
        {
            float newRotation = Mathf.Max(CurrentRotation - deltaTime * turnRate, minRotation);
            CurrentRotation = newRotation;
        }
        if (Input.GetKey(KeyCode.RightArrow) && CurrentRotation < maxRotation)
        {
            float newRotation = Mathf.Min(CurrentRotation + deltaTime * turnRate, maxRotation);
            CurrentRotation = newRotation;
        }

        // Determine if firing
        float timeSinceLastSpawn = Time.time - lastSpawnTime;
        if(Input.GetKey(KeyCode.Space) && timeSinceLastSpawn >= spawnRate)
        {

            BallController ball = Instantiate(ballPrefab, launchLocation.position, launchLocation.rotation).GetComponent<BallController>();
            ball.SetColor(nextBall.GetColor());
            nextBall.SetColor(GetRandomBallColor());
            lastSpawnTime = Time.time;
            RealBallInTransit = true;
            testBall.gameObject.SetActive(false);
            ResetPredictivePath();
        }

        // Don't fire when real ball is live
        if (GameController.LiveBall == null)
        {
            if(!testBall.gameObject.activeSelf)
            {
                testBall.gameObject.SetActive(true);
                rotatedThisFrame = true;
            }
            // Determine if need to fire. 
            // Detect either change in angle OR newly
            if(rotatedThisFrame)
            {
                ProjectBallPath();
                ShowPredictivePath();
            }

        }


    }

    public void ProjectBallPath()
    {
        testBall.transform.position = launchLocation.position;
        testBall.transform.rotation = launchLocation.rotation;
        testBall.RunSimulation();
    }

    private void ShowPredictivePath()
    {
        ResetPredictivePath();

        float distanceBetweenMarkers = 1.5f;
        float maxDistanceBetweenMarkers = 2f;
        float minDistanceBetweenMarkers = 1.25f;

        BallController bc = testBall.GetBallController();
        List<Vector3> collisionPoints = bc.collisionPoints;
        collisionPoints.Insert(0, launchLocation.position);
        
        for (int i = 1; i < bc.collisionPoints.Count; i++)
        {
            Vector3 startPosition = bc.collisionPoints[i - 1];
            Vector3 endPosition = bc.collisionPoints[i];
            Vector3 vector = endPosition - startPosition;
            Vector3 unitVector = vector.normalized;
            float distance = vector.magnitude - 1f; // need to give room for last ball

            int counter = 1;
            while (counter * distanceBetweenMarkers <= distance)
            {
                Vector3 spawnLocation = startPosition + unitVector * counter * distanceBetweenMarkers;
                predictivePathBalls.Add(SpawnDebugBall(spawnLocation, Quaternion.identity));
                counter++;
            }

            predictivePathBalls.Add(SpawnDebugBall(endPosition, Quaternion.identity));
        }
    }

    private void ResetPredictivePath()
    {
        if (predictivePathBalls.Count == 0)
            return;

        foreach(GameObject ball in predictivePathBalls)
        {
            DestroyDebugBall(ball);
        }

        predictivePathBalls.Clear();
    }

    private GameObject SpawnDebugBall(Vector3 spawnLocation, Quaternion spawnRotation)
    {
        GameObject debugBall = GameController.DebugObjectPool.GetObject();

        if(debugBall == null)
        {
            debugBall = Instantiate(debugPrefab);
        }

        debugBall.transform.position = spawnLocation;
        debugBall.transform.rotation = spawnRotation;

        return debugBall;
    }

    private void DestroyDebugBall(GameObject debugBall)
    {
        GameController.DebugObjectPool.AddToPool(debugBall);
    }

    private Color GetRandomBallColor()
    {
        int index = UnityEngine.Random.Range(0, ballColors.Length);
        return ballColors[index];
    }
}

