using UnityEngine;
using System.Collections;

public class DragRigidbody2D : MonoBehaviour
{
    public float Damper = 5f;
    public float Frequency = 3;
    public float Drag = 10f;
    public float AngularDrag = 5f;
    
    private SpringJoint2D _springJoint;

    private Camera _camera;
    private RaycastHit2D _rayHit;

	void Start ()
	{
        _camera = gameObject.GetComponent<Camera>();
	}
	
	void Update () 
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        //Looking for any collider under mouse position
        _rayHit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (_rayHit.collider == null)
            return;

        if (!_rayHit.collider.GetComponent<Rigidbody2D>() || _rayHit.collider.GetComponent<Rigidbody2D>().isKinematic)
            return;


        if (!_springJoint)
        {
            //Create spring joint
            GameObject go = new GameObject("[Rigidbody2D_dragger]");
            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            _springJoint = go.AddComponent<SpringJoint2D>();
            body.isKinematic = true;
        }

	    _springJoint.transform.position = _rayHit.point;

	    _springJoint.anchor = Vector2.zero;

        //Apply parameters to spring joint
	    _springJoint.frequency = Frequency;
	    _springJoint.dampingRatio = Damper;
	    _springJoint.distance = 0;
        _springJoint.connectedBody = _rayHit.collider.GetComponent<Rigidbody2D>();

	    StartCoroutine("DragObject");
	}

    IEnumerator DragObject()
    {
        var oldDrag = _springJoint.connectedBody.drag;
        var oldAngDrag = _springJoint.connectedBody.angularDrag;

        _springJoint.connectedBody.drag = Drag;
        _springJoint.connectedBody.angularDrag = AngularDrag;

        while (Input.GetMouseButton(0))
        {
            Vector2 newPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            _springJoint.transform.position = new Vector2(newPos.x, newPos.y);
            yield return new WaitForSeconds(0.1f);
        }

        if (_springJoint.connectedBody)
        {
            _springJoint.connectedBody.drag = oldDrag;
            _springJoint.connectedBody.angularDrag = oldAngDrag;
            _springJoint.connectedBody = null;
        }
    }
}
