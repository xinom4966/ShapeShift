using UnityEngine;

public class SpiderBehaviour : MonoBehaviour
{
    [SerializeField] private Rigidbody2D my2DRB;
    private Vector3 mouseWorldPosition;
    [SerializeField] private SpiderState myState;
    private SpringJoint2D[] connections = new SpringJoint2D[8];
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
    }

    private void OnMouseUp()
    {
        //my2DRB.gravityScale = 0f;
    }

    private void DestroyConnections()
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (connections[i] != null)
            {
                Destroy(connections[i]);
            }
        }
    }

    enum SpiderState
    {
        Free,
        Dragging,
        Locked
    }
}
