using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private GameObject target;
    private Vector3 position;
    private float width;
    private float height;

    public ILookAtCamera myCamera;
    public Camera currentCamera;

    void Awake()
    {
        width = (float)Screen.width / 2.0f;
        height = (float)Screen.height / 2.0f;

        // Position used for the cube.
        position = new Vector3(0.0f, 0.0f, 0.0f);

        Input.simulateMouseWithTouches = true;
        myCamera = currentCamera.GetComponent<ILookAtCamera>();
        myCamera.InitCamera(target, - 90, 30);
    }

    void OnGUI()
    {
        // Compute a fontSize based on the size of the screen width.
        GUI.skin.label.fontSize = (int)(Screen.width / 25.0f);

        GUI.Label(new Rect(20, 20, width, height * 0.25f),
            "x = " + position.x.ToString("f2") +
            ", y = " + position.y.ToString("f2"));
    }

    void Update()
    {
       
        if(Input.GetMouseButton(0))
        {
            myCamera.Zoom(-0.1f);
        }

        if (Input.GetMouseButton(1))
        {
            myCamera.Zoom(0.1f);

        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            myCamera.Rotate(1);

        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            myCamera.Rotate(-1);

        }

        if (Input.GetKey(KeyCode.UpArrow))
        {

            myCamera.RotateVerticle(1);

        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            myCamera.RotateVerticle(-1);

        }

        if (Input.GetKey(KeyCode.Q))
        {
            myCamera.Pan(new Vector3(1, 0, 0));

        }

        if (Input.GetKey(KeyCode.W))
        {
            myCamera.Pan(new Vector3(-1, 0, 0));

        }

        if (Input.GetKey(KeyCode.A))
        {
            myCamera.Pan(new Vector3(0, 0, 1));

        }

        if (Input.GetKey(KeyCode.S))
        {
            myCamera.Pan(new Vector3(0, 0, -1));
        }
    }
}