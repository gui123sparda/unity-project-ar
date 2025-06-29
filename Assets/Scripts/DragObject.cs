
using UnityEngine;
using GLTFast.Schema;
using Camera = UnityEngine.Camera;

public class DragObject : MonoBehaviour
{
    public Camera cam;
    private Plane dragPlane;
    private Vector3 offset;
    private bool dragging = false;



    void Awake()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    void Update()
    {
        if (cam == null)
        {
            cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        }

        if (Input.touchSupported && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchPos = touch.position;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    TryStartDrag(touchPos);
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (dragging) DragTo(touchPos);
                    break;
                case TouchPhase.Ended:
                    dragging = false;
                    break;
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            TryStartDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && dragging)
        {
            DragTo(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }
    }

    void TryStartDrag(Vector3 screenPosition)
    {
        Ray ray = cam.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == this.transform || hit.transform.IsChildOf(this.transform))
            {
                dragPlane = new Plane(Vector3.up, hit.point);
                offset = this.transform.position - hit.point;
                dragging = true;
            }
        }
    }

    void DragTo(Vector3 screenPosition)
    {
        Ray ray = cam.ScreenPointToRay(screenPosition);
        if (dragPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            transform.position = hitPoint + offset;
        }
    }

}
