using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpiderBehaviour : MonoBehaviour
{
    [SerializeField] private Rigidbody2D my2DRB;
    private Vector3 mouseWorldPosition;
    [SerializeField] public SpiderState myState;
    private List<SpringJoint2D> connections = new List<SpringJoint2D>();
    private List<Collider2D> OverlappedSpiders = new List<Collider2D>();
    private List<GameObject> targets = new List<GameObject>();
    [SerializeField] private GameObject webLine;
    private GameObject webPrediction;
    [SerializeField] private LayerMask mask;
    [SerializeField] private float detectionRadius;

    private void Start()
    {
        if (gameObject.TryGetComponent<SpringJoint2D>(out SpringJoint2D _spring))
        {
            myState = SpiderState.Locked;
            gameObject.layer = 6;
            connections.Add(_spring);

        }
        else if (DetectIndirectlyAttachedSpringJoints(1))
        {
            myState = SpiderState.Locked;
            gameObject.layer = 6;
        }
        switch (myState)
        {
            case SpiderState.Free:
                my2DRB.gravityScale = 1f;
                break;
            case SpiderState.Locked:
                my2DRB.gravityScale = 0f;
                break;
            case SpiderState.Dragging:
                Debug.LogError("D'une quelconque manière les spiders sont en dragging au start");
                break;
            default:
                break;
        }
    }

    private void Update()
    {
        mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = Camera.main.nearClipPlane;
    }

    private void OnMouseDrag()
    {
        if (myState == SpiderState.Locked)
        {
            DestroyConnections();
        }
        myState = SpiderState.Dragging;
        gameObject.layer = 0;
        transform.position = mouseWorldPosition;
        DetectNearbySpiders();
    }

    private void OnMouseUp()
    {
        if (targets.Count > 0)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                SpringJoint2D newSpring = gameObject.AddComponent(typeof(SpringJoint2D)) as SpringJoint2D;
                newSpring.connectedBody = targets[i].GetComponent<Rigidbody2D>();
            }
            myState = SpiderState.Locked;
            my2DRB.gravityScale = 0f;
            return;
        }
        myState = SpiderState.Free;
        my2DRB.gravityScale = 1f;
        return;
    }

    private void DestroyConnections()
    {
        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i] != null)
            {
                Destroy(connections[i]);
            }
        }
        DetectIndirectlyAttachedSpringJoints(0);
    }

    private void DetectNearbySpiders()
    {
        OverlappedSpiders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, mask).ToList();
        if (OverlappedSpiders.Count >= 1)
        {
            for (int i=0; i<OverlappedSpiders.Count; i++)
            {
                if (!targets.Contains(OverlappedSpiders[i].gameObject))
                {
                    targets.Add(OverlappedSpiders[i].gameObject);
                    webPrediction = Instantiate(webLine, transform.position, Quaternion.identity);
                    webPrediction.GetComponent<WebPrediction>().target = OverlappedSpiders[i].gameObject.transform;
                    webPrediction.GetComponent<WebPrediction>().start = transform;
                }
            }
        }
    }

    private bool DetectIndirectlyAttachedSpringJoints(int _instruction)
    {
        OverlappedSpiders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, mask).ToList();
        if (OverlappedSpiders.Count >= 1)
        {
            for (int i = 0; i < OverlappedSpiders.Count; i++)
            {
                if (OverlappedSpiders[i].gameObject.TryGetComponent<SpringJoint2D>(out SpringJoint2D _targetSpring))
                {
                    if (_targetSpring.connectedBody == my2DRB)
                    {
                        switch (_instruction)
                        {
                            case 0:
                                Destroy(_targetSpring);
                                break;
                            case 1:
                                connections.Add(_targetSpring);
                                break;
                            case 2:
                                break;
                            default:
                                break;
                        }
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public enum SpiderState
    {
        Free,
        Dragging,
        Locked
    }
}
