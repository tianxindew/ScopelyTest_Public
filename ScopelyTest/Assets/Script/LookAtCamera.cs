//This script allow user to control the camera to browse a target freely
using UnityEngine;

public class LookAtCamera : MonoBehaviour, ILookAtCamera
{
    //camera
    private Camera m_camera;
    private Transform m_cameraTransform;
    private Vector3 m_cameraOrbitOrigin;
    private Vector3 m_cameraVelocity;
    private Vector3 m_cameraResistance = Vector3.zero;
    private Rigidbody m_cameraRigidbody;
    private float m_cameraFOV;

    //target
    private GameObject m_lookingAtTarget;
    private Vector3 m_targetCenter;
    private BoxCollider m_targetCollider;
    private Bounds m_targetColliderBounds;

    //max and min m_radius for the camera from it's local system's origin
    private float m_minR = 0;
    private float m_maxR = 0;

    //allocate the variables need to be used at the beginning of instantiation to prevent frequent dynamic heap allocation
    //variables for pan, rotate and zoom
    private float m_minY = 0;
    private float m_maxY = 0;
    float m_radius = 0;
    float m_angleOriginToDestination = 0;
    float m_newDeltaY = 0;
    float m_newAngle = 0;
    private Vector3 m_futureOriginPosition = new Vector3();
    private Vector3 m_newDeltaPosition = new Vector3();
    private Vector3 m_newCameraPosition = new Vector3();
    private Vector2 m_originToDestination = new Vector2();
    private Vector2 m_centerToCamera = new Vector2();
    //variable for auto flying
    private Vector3 m_autoMoveDestination = new Vector3();
    private Vector3 m_autoMoveSpeed = new Vector3();
    private float m_autoMoveTime = 1.0f;
    private float m_autoMoveAngle = 0f;
    private float m_autoMoveAngleVerticle = 0f;
    private float m_autoMoveCounter = 0f;
    private float m_autoMoveFOV = 0f;
    private float m_moveResistanceFactor = 4f;
    private bool m_isAutoMove = false;

    private void OnDestroy()
    {
        System.GC.Collect();
    }

    public void InitCamera(GameObject target, float angle, float angleVerticle)
    {
        m_lookingAtTarget = target;
        m_targetCollider = m_lookingAtTarget.GetComponent<BoxCollider>();
        m_targetColliderBounds = m_targetCollider.bounds;
        m_targetCenter = m_targetColliderBounds.center;
        m_minR = ((m_targetColliderBounds.max - m_targetColliderBounds.min) / 2).magnitude * 1.5f;
        m_maxR = ((m_targetColliderBounds.max - m_targetColliderBounds.min) / 2).magnitude * 2f;

        m_camera = Camera.main;
        m_cameraTransform = m_camera.transform;
        m_cameraVelocity = m_camera.GetComponent<Rigidbody>().velocity;
        m_cameraRigidbody = m_camera.GetComponent<Rigidbody>();
        m_cameraFOV = m_camera.fieldOfView;

        m_minY = m_targetCenter.y + m_minR * Mathf.Sin(angleVerticle * Mathf.Deg2Rad);
        m_maxY = m_targetCenter.y + m_maxR * Mathf.Sin(angleVerticle * Mathf.Deg2Rad);

        m_cameraOrbitOrigin = m_targetCenter;
        m_cameraOrbitOrigin.y = 0;

        m_radius = (m_maxR + m_minR) / 2;
        m_newCameraPosition.x = m_radius * Mathf.Cos(angleVerticle * Mathf.Deg2Rad) * Mathf.Cos(angle * Mathf.Deg2Rad) + m_cameraOrbitOrigin.x;
        m_newCameraPosition.y = m_radius * Mathf.Sin(angleVerticle * Mathf.Deg2Rad) + m_targetCenter.y;
        m_newCameraPosition.z = m_radius * Mathf.Cos(angleVerticle * Mathf.Deg2Rad) * Mathf.Sin(angle * Mathf.Deg2Rad) + m_cameraOrbitOrigin.z;

        m_cameraTransform.position = m_newCameraPosition;
        m_cameraTransform.rotation = Quaternion.Euler(new Vector3(angleVerticle,0,0));
       
    }

    //instead of move camera directly, we move the camera with the origin of it's local system
    public void Pan(Vector3 deltaPosition)
    {
        //check origin's next frame
        m_futureOriginPosition.x = m_cameraOrbitOrigin.x + deltaPosition.x;
        m_futureOriginPosition.y = m_cameraOrbitOrigin.y;
        m_futureOriginPosition.z = m_cameraOrbitOrigin.z + deltaPosition.z;

        m_radius = m_targetColliderBounds.size.x > m_targetColliderBounds.size.z ? m_targetColliderBounds.size.x / 2 : m_targetColliderBounds.size.z / 2;

        if (Mathf.Pow(m_futureOriginPosition.x - m_targetCenter.x, 2) + Mathf.Pow(m_futureOriginPosition.z - m_targetCenter.z, 2) <= m_radius * m_radius)
        {
            m_cameraTransform.Translate(deltaPosition, Space.World);
            m_cameraOrbitOrigin += deltaPosition;
        } 
        else
        {
            m_originToDestination.x = m_futureOriginPosition.x - m_targetCenter.x;
            m_originToDestination.y = m_futureOriginPosition.z - m_targetCenter.z;

            m_angleOriginToDestination = Vector2.Angle(Vector2.right, m_originToDestination);
            if (m_originToDestination.y < 0)
            {
                m_angleOriginToDestination = 360 - m_angleOriginToDestination;
            }

            m_newDeltaPosition.x = m_radius * Mathf.Cos(m_angleOriginToDestination * Mathf.Deg2Rad) + m_targetCenter.x - m_cameraOrbitOrigin.x;
            m_newDeltaPosition.y = m_cameraOrbitOrigin.y - m_cameraOrbitOrigin.y;
            m_newDeltaPosition.z = m_radius * Mathf.Sin(m_angleOriginToDestination * Mathf.Deg2Rad) + m_targetCenter.z - m_cameraOrbitOrigin.z;

            m_cameraTransform.Translate(m_newDeltaPosition, Space.World);
            m_cameraOrbitOrigin += m_newDeltaPosition;
        }
    }

    public void Rotate(float degree)
    {
        //get current angle
        m_originToDestination.x = m_cameraTransform.position.x - m_targetCenter.x;
        m_originToDestination.y = m_cameraTransform.position.z - m_targetCenter.z;

        m_angleOriginToDestination = Vector2.Angle(Vector2.right, m_originToDestination);

        if (m_originToDestination.y < 0)
        {
            m_angleOriginToDestination = 360f - m_angleOriginToDestination;
        }

        m_angleOriginToDestination += degree;

        m_angleOriginToDestination %= 360;
        if (m_angleOriginToDestination < 0)
            m_angleOriginToDestination += 360;

        m_centerToCamera.x = m_cameraTransform.position.x - m_targetCenter.x;
        m_centerToCamera.y = m_cameraTransform.position.z - m_targetCenter.z;

        m_newCameraPosition.x = m_centerToCamera.magnitude * Mathf.Cos(m_angleOriginToDestination * Mathf.Deg2Rad) + m_targetCenter.x;
        m_newCameraPosition.y = m_cameraTransform.position.y;
        m_newCameraPosition.z = m_centerToCamera.magnitude * Mathf.Sin(m_angleOriginToDestination * Mathf.Deg2Rad) + m_targetCenter.z;

        m_cameraTransform.position = m_newCameraPosition;
        m_cameraTransform.Rotate(0, -degree, 0, Space.World);

        //rotate origin as well
        m_originToDestination.x = m_cameraOrbitOrigin.x - m_targetCenter.x;
        m_originToDestination.y = m_cameraOrbitOrigin.z - m_targetCenter.z;

        m_angleOriginToDestination = Vector2.Angle(Vector2.right, m_originToDestination);

        if (m_originToDestination.y < 0)
        {
            m_angleOriginToDestination = 360f - m_angleOriginToDestination;
        }

        m_angleOriginToDestination += degree;

        m_angleOriginToDestination %= 360;
        if (m_angleOriginToDestination < 0)
            m_angleOriginToDestination += 360;

        m_centerToCamera.x = m_cameraOrbitOrigin.x - m_targetCenter.x;
        m_centerToCamera.y = m_cameraOrbitOrigin.z - m_targetCenter.z;

        m_cameraOrbitOrigin.x = m_centerToCamera.magnitude * Mathf.Cos(m_angleOriginToDestination * Mathf.Deg2Rad) + m_targetCenter.x;
        m_cameraOrbitOrigin.z = m_centerToCamera.magnitude * Mathf.Sin(m_angleOriginToDestination * Mathf.Deg2Rad) + m_targetCenter.z;
    }

    public void Zoom(float distance)
    {
        m_newDeltaY = distance * Mathf.Sin(m_cameraTransform.rotation.eulerAngles.x * Mathf.Deg2Rad);

        if (m_cameraTransform.position.y + m_newDeltaY < m_minY || m_cameraTransform.position.y < m_minY)
        {
            m_newDeltaY = m_minY - m_cameraTransform.position.y;

            m_newDeltaPosition.x = 0;
            m_newDeltaPosition.y = 0;
            m_newDeltaPosition.z = -m_newDeltaY / Mathf.Sin(m_cameraTransform.rotation.eulerAngles.x * Mathf.Deg2Rad);
            m_cameraTransform.Translate(m_newDeltaPosition, Space.Self);
        }
        else if (m_cameraTransform.position.y + m_newDeltaY > m_maxY || m_cameraTransform.position.y > m_maxY)
        {
            m_newDeltaY = m_maxY - m_cameraTransform.position.y;

            m_newDeltaPosition.x = 0;
            m_newDeltaPosition.y = 0;
            m_newDeltaPosition.z = -m_newDeltaY / Mathf.Sin(m_cameraTransform.rotation.eulerAngles.x * Mathf.Deg2Rad);
            m_cameraTransform.Translate(m_newDeltaPosition, Space.Self);
        }
        else
        {
            m_newDeltaPosition.x = 0;
            m_newDeltaPosition.y = 0;
            m_newDeltaPosition.z = -m_newDeltaY / Mathf.Sin(m_cameraTransform.rotation.eulerAngles.x * Mathf.Deg2Rad);
            m_cameraTransform.Translate(m_newDeltaPosition, Space.Self);
        }
    }

    public void RotateVerticle(float degree)
    {
        m_newAngle = m_cameraTransform.rotation.eulerAngles.x + degree;
        if (m_newAngle < 90 && m_newAngle > 1)// we don't want to handle extreme condition like new angle = 90 or new angle = 0 since it's not necessary
        {
            m_minY = m_targetCenter.y + m_minR * Mathf.Sin(m_newAngle * Mathf.Deg2Rad);
            m_maxY = m_targetCenter.y + m_maxR * Mathf.Sin(m_newAngle * Mathf.Deg2Rad);

            m_originToDestination.x = m_cameraTransform.position.x - m_cameraOrbitOrigin.x;
            m_originToDestination.y = m_cameraTransform.position.z - m_cameraOrbitOrigin.z;
            m_angleOriginToDestination = Vector2.Angle(Vector2.right, m_originToDestination);

            if (m_originToDestination.y < 0)
            {
                m_angleOriginToDestination = 360 - m_angleOriginToDestination;
            }

            m_radius = (m_cameraTransform.position.y - m_targetCenter.y) / Mathf.Sin(m_cameraTransform.rotation.eulerAngles.x * Mathf.Deg2Rad);

            m_newCameraPosition.x = m_radius * Mathf.Cos(m_newAngle * Mathf.Deg2Rad) * Mathf.Cos(m_angleOriginToDestination * Mathf.Deg2Rad) + m_cameraOrbitOrigin.x;
            m_newCameraPosition.y = m_radius * Mathf.Sin(m_newAngle * Mathf.Deg2Rad) + m_targetCenter.y;
            m_newCameraPosition.z = m_radius * Mathf.Cos(m_newAngle * Mathf.Deg2Rad) * Mathf.Sin(m_angleOriginToDestination * Mathf.Deg2Rad) + m_cameraOrbitOrigin.z;

            m_cameraTransform.position = m_newCameraPosition;
            m_cameraTransform.Rotate(degree, 0, 0, Space.Self);
        }
    }

    public void UpdateCamera()
    {
        //Add resistance to the camera according to the speed, so the camera will stop naturally after user stop input
        m_cameraResistance = m_cameraVelocity * m_moveResistanceFactor;
        m_cameraRigidbody.AddForce(-m_cameraResistance);

        if (m_isAutoMove)
        {
            if (m_autoMoveCounter < m_autoMoveTime)
            {
                if (m_autoMoveCounter + Time.deltaTime <= m_autoMoveTime)
                {
                    m_cameraTransform.position += m_autoMoveSpeed * Time.deltaTime;
                    m_cameraTransform.RotateAround(m_cameraTransform.position, Vector3.up, m_autoMoveAngle * Time.deltaTime);
                    m_cameraTransform.RotateAround(m_cameraTransform.position, m_cameraTransform.right, m_autoMoveAngleVerticle * Time.deltaTime);
                    m_cameraFOV += m_autoMoveFOV * Time.deltaTime;
                    m_autoMoveCounter += Time.deltaTime;
                }
                //In the last frame of camera auto moving, the actual move time will be less than Time.deltaTime. 
                //In this case, we use (m_autoMoveTime - m_autoMoveCounter) instead of Time.deltaTime
                else
                {
                    m_cameraTransform.position += m_autoMoveSpeed * (m_autoMoveTime - m_autoMoveCounter);
                    m_cameraTransform.RotateAround(m_cameraTransform.position, Vector3.up, m_autoMoveAngle * (m_autoMoveTime - m_autoMoveCounter));
                    m_cameraTransform.RotateAround(m_cameraTransform.position, m_cameraTransform.right, m_autoMoveAngleVerticle * (m_autoMoveTime - m_autoMoveCounter));
                    m_cameraFOV += m_autoMoveFOV * (m_autoMoveTime - m_autoMoveCounter);
                    m_autoMoveCounter += (m_autoMoveTime - m_autoMoveCounter);
                }
            }
            else
            {
                m_isAutoMove = false;
                m_autoMoveCounter = float.MaxValue;
            }
        }
    }

    public void CameraAutoFlyToPoint(Vector3 targetPosition, float RotatingAngle, float RotatingAngleVerticle = 0, float FOV = 0)
    {
        if (m_isAutoMove == true)
            return;

        m_isAutoMove = true;

        //get destination and move speed
        m_autoMoveDestination = targetPosition;
        m_autoMoveSpeed = (m_autoMoveDestination - m_cameraTransform.position) / m_autoMoveTime;

        //get destination angle and rotate speed
        m_autoMoveAngle = RotatingAngle / m_autoMoveTime;
        m_autoMoveAngleVerticle = RotatingAngleVerticle / m_autoMoveTime;
        m_autoMoveFOV = FOV / m_autoMoveTime;

        m_autoMoveCounter = 0f;
    }
}
