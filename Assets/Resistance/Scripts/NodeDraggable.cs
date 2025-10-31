using UnityEngine;
using UnityEngine.EventSystems;

public class NodeDraggable : MonoBehaviour
{
    private bool isDragged = false;
    private Vector3 posStartDragTransform;
    private Vector3 posStartDragCursol;
    public void OnMouseDown()
    {
        Vector3 mousePos = Input.mousePosition;
        posStartDragCursol = Camera.main.ScreenToWorldPoint(mousePos);
        posStartDragTransform = this.transform.position;

        isDragged = true;
    }
    
    public void OnMouseUp()
    {
        isDragged = false;
    }

    void Update()
    {
        if(isDragged)
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 delta = Camera.main.ScreenToWorldPoint(mousePos) - posStartDragCursol;
            Vector3 buf = posStartDragTransform + delta;
            this.transform.position = new Vector3(buf.x, buf.y, 0f);
        }
    }
}