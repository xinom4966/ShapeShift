using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    private List<GameObject> allPreviews = new List<GameObject>();
    [SerializeField] private LayerMask mask;
    [SerializeField] private float detectionRadius;
    [SerializeField] private GameObject connectionPrefab;
    private GameObject connectionLine;
    private Vector2 bounceVector = Vector2.zero;
    private Color lockedColor = new Color(255,255,255,1);
    private Color freeColor = new Color(255,255,255,0.25f);
    [SerializeField] private SpriteRenderer myRenderer;

    private void Start()
    {
        if (gameObject.TryGetComponent<SpringJoint2D>(out SpringJoint2D _spring))
        {
            myState = SpiderState.Locked;
            gameObject.layer = 6;
            foreach (SpringJoint2D spring in gameObject.GetComponents<SpringJoint2D>())
            {
                connections.Add(spring);
                SetUpVisuals(spring.connectedBody.gameObject, spring);
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
                myRenderer.color = freeColor;
                break;
            case SpiderState.Locked:
                my2DRB.gravityScale = 0f;
                myRenderer.color = lockedColor;
                break;
            case SpiderState.Dragging:
                Debug.LogError("D'une quelconque mani�re les spiders sont en dragging au start");
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

        //Fonction de restart fait tr�s grossi�rement le vendredi du rendu � 16h40
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void OnMouseDrag()
    {
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
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
        gameObject.GetComponent<BoxCollider2D>().enabled = true;
        my2DRB.velocity = Vector3.zero;
        if (targets.Count > 0)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                SpringJoint2D newSpring = gameObject.AddComponent(typeof(SpringJoint2D)) as SpringJoint2D;
                newSpring.connectedBody = targets[i].GetComponent<Rigidbody2D>();
                AddConnection(newSpring);
                targets[i].GetComponent<SpiderBehaviour>().AddConnection(newSpring);
                SetUpVisuals(targets[i], newSpring);
                CalculateAndAddToBounceForce(connectionLine.GetComponent<WebBehaviour>());
            }
            myState = SpiderState.Locked;
            my2DRB.gravityScale = 0f;
            myRenderer.color = lockedColor;
            gameObject.layer = 6;
            DestroyAllPreviews();
            my2DRB.velocity += bounceVector*2;
            bounceVector = Vector2.zero;
            return;
        }
        myState = SpiderState.Free;
        my2DRB.gravityScale = 1f;
        myRenderer.color = freeColor;
        DestroyAllPreviews();
        return;
    }

    public void DestroyConnections()
    {
        foreach (SpringJoint2D connection in connections)
        {
            Destroy(connection);
        }
        connections.Clear();
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
                    if (!SearchForPreview(OverlappedSpiders[i].gameObject))
                    {
                        webPrediction = Instantiate(webLine, transform.position, Quaternion.identity);
                        webPrediction.GetComponent<WebPrediction>().target = OverlappedSpiders[i].gameObject;
                        webPrediction.GetComponent<WebPrediction>().start = gameObject;
                        allPreviews.Add(webPrediction);
                    }
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
                                    OverlappedSpiders[i].gameObject.GetComponent<SpiderBehaviour>().RemoveInConnectionList(-1, spring);
                                    Destroy(spring);
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
            myRenderer.color = freeColor;
            gameObject.layer = 0;
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

    private void DestroyAllPreviews()
    {
        foreach (GameObject element in allPreviews)
        {
            Destroy(element);
        }
        allPreviews.Clear();
    }

    private bool SearchForPreview(GameObject _targetObject)
    {
        for (int i = 0; i <allPreviews.Count; i++)
        {
           if (allPreviews[i].GetComponent<WebPrediction>().target == _targetObject)
            {
                return true;
            }
        }
        return false;
    }

    private void SetUpVisuals(GameObject _targetObject, SpringJoint2D _spring)
    {
        connectionLine = Instantiate(connectionPrefab, Vector3.zero, Quaternion.identity);
        connectionLine.GetComponent<WebBehaviour>().end1 = gameObject;
        connectionLine.GetComponent<WebBehaviour>().end2 = _targetObject;
        connectionLine.GetComponent<WebBehaviour>().spring = _spring;
    }

    private void CalculateAndAddToBounceForce(WebBehaviour _web)
    {
        Vector2 direction = gameObject.transform.position - _web.end2.transform.position;
        bounceVector += direction;
    }

    public enum SpiderState
    {
        Free,
        Dragging,
        Locked
    }
}
