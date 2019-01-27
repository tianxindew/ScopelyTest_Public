using UnityEngine;

public class LookAtCameraAutoFly : MonoBehaviour
{
    //allocate the variable which will be used in the class.
    //In this way, runtime dynamic memory allocation can be avoid.
    private Vector3 autoMoveDestination = new Vector3();
    private Vector3 autoMoveSpeed = new Vector3();
    private float autoMoveTime = 1.0f;
    private float autoMoveAngle = 0f;
    private float autoMoveAngleVerticle = 0f;
    private float autoMoveCounter = 0f;
    private float autoMoveFOV = 0f;
    [SerializeField]//expose the value in editor for designer to adjust the value of camera
    private float moveResistanceFactor = 4f;
    private bool isAutoMove = false;

    private Camera cam = null;
    private Vector3 cameraVelocity;
    private Vector3 cameraResistance = Vector3.zero;
    private Rigidbody cameraRigidbody;
    private Transform cameraTransform;
    private float cameraFOV;

    public float AutoMoveTime
    {
        set
        {
            autoMoveTime = value;
        }
        get
        {
            return autoMoveTime;
        }
    }

    void Start()
    {
        InitCamera();
    }

    public void InitCamera()
    {
        //init the camera variables
        cam = Camera.main;
        cameraVelocity = cam.GetComponent<Rigidbody>().velocity;
        cameraRigidbody = cam.GetComponent<Rigidbody>();
        cameraTransform = cam.transform;
        cameraFOV = cam.fieldOfView;
    }

    public void UpdateCamera()
    {
        if (cam == null)
        {
            InitCamera();
        }

        //Add resistance to the camera according to the speed, so the camera will stop naturally after user stop input
        cameraResistance = cameraVelocity * moveResistanceFactor;
        cameraRigidbody.AddForce(-cameraResistance);

        if (isAutoMove)
        {
            if (autoMoveCounter < autoMoveTime)
            {
                if (autoMoveCounter + Time.deltaTime <= autoMoveTime)
                {
                    cameraTransform.position += autoMoveSpeed * Time.deltaTime;
                    cameraTransform.RotateAround(cameraTransform.position, Vector3.up, autoMoveAngle * Time.deltaTime);
                    cameraTransform.RotateAround(cameraTransform.position, cameraTransform.right, autoMoveAngleVerticle * Time.deltaTime);
                    cameraFOV += autoMoveFOV * Time.deltaTime;
                    autoMoveCounter += Time.deltaTime;
                }
                //In the last frame of camera auto moving, the actual move time will be less than Time.deltaTime. 
                //In this case, we use (autoMoveTime - autoMoveCounter) instead of Time.deltaTime
                else
                {
                    cameraTransform.position += autoMoveSpeed * (autoMoveTime - autoMoveCounter);
                    cameraTransform.RotateAround(cameraTransform.position, Vector3.up, autoMoveAngle * (autoMoveTime - autoMoveCounter));
                    cameraTransform.RotateAround(cameraTransform.position, cameraTransform.right, autoMoveAngleVerticle * (autoMoveTime - autoMoveCounter));
                    cameraFOV += autoMoveFOV * (autoMoveTime - autoMoveCounter);
                    autoMoveCounter += (autoMoveTime - autoMoveCounter);
                }
            }
            else
            {
                isAutoMove = false;
                autoMoveCounter = float.MaxValue;
            }
        }
    }

    public void CameraAutoFlyToPoint(Vector3 targetPosition, float RotatingAngle, float RotatingAngleVerticle = 0, float FOV = 0)
    {
        if (isAutoMove == true)
            return;

        isAutoMove = true;

        //get destination and move speed
        autoMoveDestination = targetPosition;
        autoMoveSpeed = (autoMoveDestination - cameraTransform.position) / autoMoveTime;

        //get destination angle and rotate speed
        autoMoveAngle = RotatingAngle / autoMoveTime;
        autoMoveAngleVerticle = RotatingAngleVerticle / autoMoveTime;
        autoMoveFOV = FOV / autoMoveTime;

        autoMoveCounter = 0f;
    }

    public bool IsAutoMove
    {
        get { return isAutoMove; }
    }

    public void Pan(Vector3 startPosition, Vector3 deltaPosition)
    {

    }

    public void Rotate(float degree)
    {

    }

    public void Zoom(float distance)
    {

    }
}
