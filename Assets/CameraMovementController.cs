using UnityEngine;

public class CameraMovementController : MonoBehaviour
{
    public float sensitivity;
    public float dragPanSensitivity = 1;
    public float panSpeedPercent;

    public float minCameraSize = 3;
    public float maxCameraSize = 30;

    private Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
        if (camera == null)
        {
            throw new System.Exception("No camera found");
        }
        lastMouse = Input.mousePosition;
    }

    Vector3 lastMouse;

    // Update is called once per frame
    void Update()
    {
        var scrollInput = Input.GetAxis("Mouse ScrollWheel") * sensitivity;
        camera.orthographicSize = Mathf.Min(Mathf.Max(camera.orthographicSize + scrollInput, minCameraSize), maxCameraSize);

        if (Input.GetMouseButton(2))
        {
            var translate = lastMouse - Input.mousePosition;
            camera.transform.position += dragPanSensitivity * translate * Time.deltaTime * camera.orthographicSize / Time.timeScale;
        }

        if (Input.GetKey(KeyCode.W))
        {
            camera.transform.position += Vector3.up * (panSpeedPercent * camera.orthographicSize);
        }
        if (Input.GetKey(KeyCode.A))
        {
            camera.transform.position += Vector3.left * (panSpeedPercent * camera.orthographicSize);
        }
        if (Input.GetKey(KeyCode.S))
        {
            camera.transform.position += Vector3.down * (panSpeedPercent * camera.orthographicSize);
        }
        if (Input.GetKey(KeyCode.D))
        {
            camera.transform.position += Vector3.right * (panSpeedPercent * camera.orthographicSize);
        }


        lastMouse = Input.mousePosition;
    }

    //public override void OnScroll(PointerEventData data)
    //{
    //    Debug.Log("On Scroll delegate called.");
    //    var scrollDist = data.scrollDelta.magnitude;
    //    this.camera.orthographicSize += 1;
    //}

    //public override void OnPointerClick(PointerEventData data)
    //{
    //    Debug.Log("OnPointerClick called.");
    //}
}
