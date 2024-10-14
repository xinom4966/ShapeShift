using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpiderBehaviour : MonoBehaviour
{
    [SerializeField] private Rigidbody2D my2DRB;
    private Vector3 mouseWorldPosition;
    [SerializeField] public SpiderState myState;
    [SerializeField] private List<SpringJoint2D> connections = new List<SpringJoint2D>();
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
            foreach (SpringJoint2D spring in gameObject.GetComponents<SpringJoint2D>())
            {
                connections.Add(spring);
            }
        }
        if (DetectIndirectlyAttachedSpringJoints(1))
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
        OnJointDestruction();
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
                targets[i].GetComponent<SpiderBehaviour>().AddConnection(newSpring);
            }
            myState = SpiderState.Locked;
            my2DRB.gravityScale = 0f;
            Destroy(webPrediction);
            return;
        }
        myState = SpiderState.Free;
        my2DRB.gravityScale = 1f;
        Destroy(webPrediction);
        return;
    }

    private void DestroyConnections()
    {
        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i] != null)
            {
                Destroy(connections[i]);
                RemoveInConnectionList(i);
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
        for (int j = 0; j < targets.Count; j++)
        {
            if (!OverlappedSpiders.Contains(targets[j].GetComponent<Collider2D>()))
            {
                targets.RemoveAt(j);
            }
        }
    }

    private bool DetectIndirectlyAttachedSpringJoints(int _instruction)
    {
        bool result = false;
        OverlappedSpiders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, mask).ToList();
        Debug.Log(OverlappedSpiders.Count);
        if (OverlappedSpiders.Count >= 1)
        {
            for (int i = 0; i < OverlappedSpiders.Count; i++)
            {
                if (OverlappedSpiders[i].gameObject.TryGetComponent<SpringJoint2D>(out SpringJoint2D _targetSpring))
                {
                    foreach (SpringJoint2D spring in OverlappedSpiders[i].gameObject.GetComponents<SpringJoint2D>())
                    {
                        if (spring.connectedBody == my2DRB)
                        {
                            switch (_instruction)
                            {
                                case 0:
                                    Destroy(spring);
                                    OverlappedSpiders[i].gameObject.GetComponent<SpiderBehaviour>().RemoveInConnectionList(-1, spring);
                                    break;
                                case 1:
                                    connections.Add(spring);
                                    break;
                                case 2:
                                    break;
                                default:
                                    break;
                            }
                            result = true;
                        }
                    }
                }
            }
        }
        return result;
    }

    private void OnJointDestruction()
    {
        if (connections.Count == 0)
        {
            myState = SpiderState.Free;
            my2DRB.gravityScale = 1.0f;
            //gameObject.layer = 0;
        }
    }

    public void AddConnection(SpringJoint2D _spring)
    {
        connections.Add(_spring);
    }

    public void RemoveInConnectionList(int index = -1,SpringJoint2D _spring = null)
    {
        if (index == -1)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i] == _spring)
                {
                    connections.RemoveAt(i);
                }
            }
        }
        else
        {
            connections.RemoveAt(index);
        }
    }

    public enum SpiderState
    {
        Free,
        Dragging,
        Locked
    }
}
