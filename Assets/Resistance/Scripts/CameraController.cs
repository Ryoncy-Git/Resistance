using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject board;
    private bool isCameraDragged;
    private Vector3 posStartDragCursol;
    private Vector3 posStartDragTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float input = Input.GetAxis("Mouse ScrollWheel");

        if (input != 0f)
        {
            float minScale = 0.2f;
            float maxScale = 5f;

            board.transform.localScale += new Vector3(input, input, 0f);
            if(board.transform.localScale.x < minScale)
            {
                board.transform.localScale = new Vector3(minScale, minScale, 0f);
            }

            if(board.transform.localScale.x > maxScale)
            {
                board.transform.localScale = new Vector3(maxScale, maxScale, 0f);
            }
        }

        if (Input.GetMouseButtonDown(2))
        {
            posStartDragCursol = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            posStartDragTransform = board.transform.position;
            isCameraDragged = true;
        }

        if (Input.GetMouseButtonUp(2))
        {
            isCameraDragged = false;
        }
        

        if(isCameraDragged)
        {
            Vector3 delta = Camera.main.ScreenToWorldPoint(Input.mousePosition) - posStartDragCursol;
            Vector3 buf = posStartDragTransform + delta;
            float x = Mathf.Clamp(buf.x, -5f, 5f);
            float y = Mathf.Clamp(buf.y, -5f, 5f);
            board.transform.position = new Vector3(x, y, 0f);
        }
    }
}
