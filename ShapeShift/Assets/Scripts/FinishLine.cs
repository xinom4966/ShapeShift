using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLine : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;

    private void finish()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<SpiderBehaviour>(out SpiderBehaviour spider))
        {
            if (spider.myState == SpiderBehaviour.SpiderState.Locked)
            {
                finish();
            }
        }
    }
}
