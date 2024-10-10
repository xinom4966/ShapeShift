using UnityEngine;

public class WebPrediction : MonoBehaviour
{
    private float distance;
    public Transform target;

    private void Update()
    {
        distance = Vector2.Distance(transform.position, target.position);

        //Rotation du fil en fonction de la spider en déplacement
        float angle = Mathf.Atan2(target.position.y - transform.position.y, target.position.x - transform.position.x)*Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(angle, 0, 0);

        //Rescale en fonction de la distance entre les 2 spiders
        transform.localScale = new(distance / transform.localScale.x, 0.3f, 1);
    }
}
