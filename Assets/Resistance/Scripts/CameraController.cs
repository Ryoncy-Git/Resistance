using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject rig;
    private bool isCameraDragged;
    private Vector3 posStartDragCursol;
    private Vector3 posStartDragTransform;

    private float minZoom = 1f, maxZoom = 50f;
    private float zoomSpeed = 50f;
    private Camera cam;
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        // ズーム
        float scroll = Input.GetAxis("Mouse ScrollWheel");


        if (Mathf.Abs(scroll) > 0.01f)
        {
            float size = cam.orthographicSize * (1f - scroll * zoomSpeed * Time.deltaTime * 10f);
            cam.orthographicSize = Mathf.Clamp(size, minZoom, maxZoom);
        }

        // ドラッグ開始合図
        if (Input.GetMouseButtonDown(2))
        {
            posStartDragCursol = Input.mousePosition;
            posStartDragTransform = rig.transform.position;
            isCameraDragged = true;
        }

        // ドラッグ終わり合図
        if (Input.GetMouseButtonUp(2))
        {
            isCameraDragged = false;
        }


        // ドラッグの処理
        if (isCameraDragged)
        {
            Vector3 deltaScreen = Input.mousePosition - posStartDragCursol;
            Vector3 deltaWorld = cam.ScreenToWorldPoint(new Vector3(deltaScreen.x, deltaScreen.y, cam.nearClipPlane))
                               - cam.ScreenToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
            Vector3 buf = posStartDragTransform - deltaWorld;

            float x = Mathf.Clamp(buf.x, -5f, 5f);
            float y = Mathf.Clamp(buf.y, -5f, 5f);

            rig.transform.position = new Vector3(x, y, 0f);
        }
    }
}
