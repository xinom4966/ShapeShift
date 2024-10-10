using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpiderBehaviour : MonoBehaviour
{
    [SerializeField] private Rigidbody2D my2DRB;
    private Vector3 mouseWorldPosition;
    [SerializeField] public SpiderState myState;
    private List<SpringJoint2D> connections = new List<SpringJoint2D>();
    private Collider2D[] OverlappedSpiders;
    private List<GameObject> targets = new List<GameObject>();
    [SerializeField] private GameObject webLine;
    private GameObject webPrediction;

    private void Start()
    {
        if (gameObject.TryGetComponent<SpringJoint2D>(out SpringJoint2D _spring))
        {
            myState = SpiderState.Locked;
            gameObject.layer = 6;
            connections.Add(_spring);
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
        transform.position = mouseWorldPosition;
        DetectNearbySpiders();
    }

    private void OnMouseUp()
    {
        /*if (myState == SpiderState.Locked)
        {
            my2DRB.gravityScale = 0f;
        }*/
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
        OverlappedSpiders = Physics2D.OverlapCircleAll(transform.position, 7f, LayerMask.GetMask("LockedSpider"));
        if (OverlappedSpiders.Length >= 1)
        {
            for (int i=0; i<OverlappedSpiders.Length; i++)
            {
                if (!targets.Contains(OverlappedSpiders[i].gameObject))
                {
                    targets.Add(OverlappedSpiders[i].gameObject);
                    webPrediction = Instantiate(webLine, transform.position, Quaternion.identity, transform);
                    webPrediction.GetComponent<WebPrediction>().target = OverlappedSpiders[i].gameObject.transform;
                }
            }
        }
    }

    private void DetectIndirectlyAttachedSpringJoints(int _instruction)
    {
        OverlappedSpiders = Physics2D.OverlapCircleAll(transform.position, 7f, LayerMask.GetMask("LockedSpider"));
        if (OverlappedSpiders.Length >= 1)
        {
            for (int i = 0; i < OverlappedSpiders.Length; i++)
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
                                break;
                            case 2:
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }
    }

    public enum SpiderState
    {
        Free,
        Dragging,
        Locked
    }
}
