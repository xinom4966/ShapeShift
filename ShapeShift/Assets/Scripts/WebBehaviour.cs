using UnityEngine;

public class WebBehaviour : MonoBehaviour
{
    [SerializeField] private LineRenderer myRenderer;
    public GameObject end1;
    public GameObject end2;

    private void Update()
    {
        myRenderer.SetPosition(0, end1.transform.position);
        myRenderer.SetPosition(1, end2.transform.position);
    }
}