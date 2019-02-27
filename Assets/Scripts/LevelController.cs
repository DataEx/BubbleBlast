using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    public string nextLevel;

    public void LoadNextLevel() {
        SceneManager.LoadScene(nextLevel, LoadSceneMode.Single);
    }

}
