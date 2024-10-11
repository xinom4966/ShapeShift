using UnityEngine;

public class WebPrediction : MonoBehaviour
{
    private float distance;
    public Transform target;
    public Transform start;
    private Vector3 direction;
    [SerializeField] private SpriteRenderer myRenderer;
    private Color transparent = Color.clear;
    private Color baseColor;
    [SerializeField] private float maxDist;

    private void Start()
    {
        baseColor = myRenderer.color;
    }

    private void Update()
    {
        distance = Vector2.Distance(target.position, start.position);
        direction = target.position - start.position;
        transform.position = start.position + direction/2;

        //Rotation du fil en fonction de la spider en déplacement
        float angle = Mathf.Atan2(target.position.y - transform.position.y, target.position.x - transform.position.x)*Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        //Rescale en fonction de la distance entre les 2 spiders
        transform.localScale = new(distance / start.transform.localScale.x, 0.3f, 1);

        if (distance > maxDist)
        {
            myRenderer.color = transparent;
        }
        else
        {
            myRenderer.color = baseColor;
        }
    }
}
