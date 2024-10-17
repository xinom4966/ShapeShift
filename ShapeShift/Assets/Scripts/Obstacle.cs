using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private ObstacleType type;
    [SerializeField] private float speed;
    private bool isLethal;

    [Header("MovingPlatforms Attributes")]
    [SerializeField] private List<Transform> wayPoints;
    private int ind = 0;

    private void Start()
    {
        switch (type)
        {
            case ObstacleType.Saw:
                isLethal = true;
                break;
            case ObstacleType.DestructiveWall:
                isLethal = true;
                break;
            case ObstacleType.MovingPlatform:
                transform.position = wayPoints[ind].position;
                isLethal = false;
                break;
            case ObstacleType.RotatingPlatform:
                isLethal = false;
                break;
        }
    }

    private void Update()
    {
        switch (type)
        {
            case ObstacleType.Saw:
                Rotate();
                break;
            case ObstacleType.DestructiveWall:
                break;
            case ObstacleType.MovingPlatform:
                Move();
                break;
            case ObstacleType.RotatingPlatform:
                Rotate();
                break;
            default:
                break;
        }
    }

    private void Rotate()
    {
        transform.Rotate(new Vector3(0, 0, speed));
    }

    private void Move()
    {
        if (Vector2.Distance(transform.position, wayPoints[ind].position) < 0.02f)
        {
            ind++;
            if (ind == wayPoints.Count)
            {
                ind = 0;
            }
        }
        transform.position = Vector3.MoveTowards(transform.position, wayPoints[ind].position, speed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isLethal)
        {
            return;
        }
        if (collision.gameObject.TryGetComponent<WebBehaviour>(out WebBehaviour web))
        {
            Destroy(web.spring);
        }
        if (collision.gameObject.TryGetComponent<SpiderBehaviour>(out SpiderBehaviour spider))
        {
            spider.DestroyConnections();
        }
    }

    public enum ObstacleType
    {
        Saw,
        DestructiveWall,
        MovingPlatform,
        RotatingPlatform
    }
}
