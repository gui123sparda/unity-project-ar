using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadSceneManager : MonoBehaviour
{
    public String sceneName;
    public void LoadScene()
    {
        SceneManager.LoadSceneAsync(sceneName);
    }
}
