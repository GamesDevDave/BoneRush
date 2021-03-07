using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    private int nextLevel;
    private Scene scene;

    private void Start()
    {
        scene = SceneManager.GetActiveScene();
    }

    private void OnTriggerEnter(Collider other)
    {
        scene = SceneManager.GetActiveScene();
        if (SceneManager.GetActiveScene().buildIndex == 8)
        {
            SceneManager.LoadScene(1);
        }
        else
        {
            nextLevel = Random.Range(2, 7);
            while (nextLevel == SceneManager.GetActiveScene().buildIndex)
            {
                nextLevel = Random.Range(2, 7);
            }
            SceneManager.LoadScene(nextLevel);
        }
    }
}
