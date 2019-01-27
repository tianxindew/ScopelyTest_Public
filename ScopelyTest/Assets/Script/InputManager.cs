using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class InputManager : MonoBehaviour
{
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
        myCamera.InitCamera(-90, 30);
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
       
        if(Input.GetMouseButtonUp(0))
        {
            myCamera.Zoom(-0.1f);
        }

        if (Input.GetMouseButtonUp(1))
        {
            myCamera.Zoom(0.1f);

        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            myCamera.Rotate(10);

        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            myCamera.Rotate(-10);

        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {

            myCamera.RotateVerticle(1);

        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            myCamera.RotateVerticle(-1);

        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            myCamera.Pan(new Vector3(1, 0, 0));

        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            myCamera.Pan(new Vector3(-1, 0, 0));

        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            myCamera.Pan(new Vector3(0, 0, 1));

        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            myCamera.Pan(new Vector3(0, 0, -1));
        }
    }
}