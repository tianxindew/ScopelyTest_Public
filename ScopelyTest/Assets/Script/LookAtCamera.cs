//This script allow user to control the camera to browse a target freely
using UnityEngine;

public class LookAtCamera : MonoBehaviour, ILookAtCamera
{
    //camera
    private Camera m_camera;
    private Transform m_cameraTransform;
    private Vector3 m_cameraOrbitOrigin;

    //target
    private GameObject m_lookingAtTarget;
    private Vector3 m_targetCenter;
    private BoxCollider m_targetCollider;
    private Bounds m_targetColliderBounds;

    //max and min radius for the camera from it's local system's origin
    private float m_minR = 0;
    private float m_maxR = 0;

    //allocate the variables need to be used at the beginning of instantiation to prevent frequent dynamic heap allocation
    private float m_minY = 0;
    private float m_maxY = 0;
    float radius = 0;
    float angleOriginToDestination = 0;
    float newDeltaY = 0;
    float newAngle = 0;
    private Vector3 futureOriginPosition = new Vector3();
    private Vector3 newDeltaPosition = new Vector3();
    private Vector3 newCameraPosition = new Vector3();
    private Vector2 originToDestination = new Vector2();
    private Vector2 centerToCamera = new Vector2();

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

        m_minY = m_targetCenter.y + m_minR * Mathf.Sin(angleVerticle * Mathf.Deg2Rad);
        m_maxY = m_targetCenter.y + m_maxR * Mathf.Sin(angleVerticle * Mathf.Deg2Rad);

        float R = (m_maxR + m_minR) / 2;
        float positionX = R * Mathf.Cos(angleVerticle * Mathf.Deg2Rad) * Mathf.Cos(angle * Mathf.Deg2Rad);
        float positionY = R * Mathf.Sin(angleVerticle * Mathf.Deg2Rad);
        float positionZ = R * Mathf.Cos(angleVerticle * Mathf.Deg2Rad) * Mathf.Sin(angle * Mathf.Deg2Rad);
        m_cameraOrbitOrigin = m_targetCenter;
        m_cameraOrbitOrigin.y = 0;

        m_cameraTransform.position = new Vector3(positionX + m_cameraOrbitOrigin.x, positionY + m_targetCenter.y, positionZ + m_cameraOrbitOrigin.z);
        m_cameraTransform.rotation = Quaternion.Euler(new Vector3(angleVerticle,0,0));
       
    }

    //instead of move camera directly, we move the camera with the origin of it's local system
    public void Pan(Vector3 deltaPosition)
    {
        //check origin's next frame
        futureOriginPosition.x = m_cameraOrbitOrigin.x + deltaPosition.x;
        futureOriginPosition.y = m_cameraOrbitOrigin.y;
        futureOriginPosition.z = m_cameraOrbitOrigin.z + deltaPosition.z;

        radius = m_targetColliderBounds.size.x > m_targetColliderBounds.size.z ? m_targetColliderBounds.size.x / 2 : m_targetColliderBounds.size.z / 2;

        if (Mathf.Pow(futureOriginPosition.x - m_targetCenter.x, 2) + Mathf.Pow(futureOriginPosition.z - m_targetCenter.z, 2) <= radius * radius)
        {
            m_cameraTransform.Translate(deltaPosition, Space.World);
            m_cameraOrbitOrigin += deltaPosition;
        } 
        else
        {
            originToDestination.x = futureOriginPosition.x - m_targetCenter.x;
            originToDestination.y = futureOriginPosition.z - m_targetCenter.z;

            angleOriginToDestination = Vector2.Angle(Vector2.right, originToDestination);
            if (originToDestination.y < 0)
            {
                angleOriginToDestination = 360 - angleOriginToDestination;
            }

            newDeltaPosition.x = radius * Mathf.Cos(angleOriginToDestination * Mathf.Deg2Rad) + m_targetCenter.x - m_cameraOrbitOrigin.x;
            newDeltaPosition.y = m_cameraOrbitOrigin.y - m_cameraOrbitOrigin.y;
            newDeltaPosition.z = radius * Mathf.Sin(angleOriginToDestination * Mathf.Deg2Rad) + m_targetCenter.z - m_cameraOrbitOrigin.z;

            m_cameraTransform.Translate(newDeltaPosition, Space.World);
            m_cameraOrbitOrigin += newDeltaPosition;
        }
    }

    public void Rotate(float degree)
    {
        //get current angle
        originToDestination.x = m_cameraTransform.position.x - m_targetCenter.x;
        originToDestination.y = m_cameraTransform.position.z - m_targetCenter.z;

        angleOriginToDestination = Vector2.Angle(Vector2.right, originToDestination);

        if (originToDestination.y < 0)
        {
            angleOriginToDestination = 360f - angleOriginToDestination;
        }

        angleOriginToDestination += degree;

        angleOriginToDestination %= 360;
        if (angleOriginToDestination < 0)
            angleOriginToDestination += 360;

        centerToCamera.x = m_cameraTransform.position.x - m_targetCenter.x;
        centerToCamera.y = m_cameraTransform.position.z - m_targetCenter.z;

        newCameraPosition.x = centerToCamera.magnitude * Mathf.Cos(angleOriginToDestination * Mathf.Deg2Rad) + m_targetCenter.x;
        newCameraPosition.y = m_cameraTransform.position.y;
        newCameraPosition.z = centerToCamera.magnitude * Mathf.Sin(angleOriginToDestination * Mathf.Deg2Rad) + m_targetCenter.z;

        m_cameraTransform.position = newCameraPosition;
        m_cameraTransform.Rotate(0, -degree, 0, Space.World);

        //rotate origin as well
        originToDestination.x = m_cameraOrbitOrigin.x - m_targetCenter.x;
        originToDestination.y = m_cameraOrbitOrigin.z - m_targetCenter.z;

        angleOriginToDestination = Vector2.Angle(Vector2.right, originToDestination);

        if (originToDestination.y < 0)
        {
            angleOriginToDestination = 360f - angleOriginToDestination;
        }

        angleOriginToDestination += degree;

        angleOriginToDestination %= 360;
        if (angleOriginToDestination < 0)
            angleOriginToDestination += 360;

        centerToCamera.x = m_cameraOrbitOrigin.x - m_targetCenter.x;
        centerToCamera.y = m_cameraOrbitOrigin.z - m_targetCenter.z;

        m_cameraOrbitOrigin.x = centerToCamera.magnitude * Mathf.Cos(angleOriginToDestination * Mathf.Deg2Rad) + m_targetCenter.x;
        m_cameraOrbitOrigin.z = centerToCamera.magnitude * Mathf.Sin(angleOriginToDestination * Mathf.Deg2Rad) + m_targetCenter.z;
    }

    public void Zoom(float distance)
    {
        newDeltaY = distance * Mathf.Sin(m_cameraTransform.rotation.eulerAngles.x * Mathf.Deg2Rad);

        if (m_cameraTransform.position.y + newDeltaY < m_minY || m_cameraTransform.position.y < m_minY)
        {
            newDeltaY = m_minY - m_cameraTransform.position.y;

            newDeltaPosition.x = 0;
            newDeltaPosition.y = 0;
            newDeltaPosition.z = -newDeltaY / Mathf.Sin(m_cameraTransform.rotation.eulerAngles.x * Mathf.Deg2Rad);
            m_cameraTransform.Translate(newDeltaPosition, Space.Self);
        }
        else if (m_cameraTransform.position.y + newDeltaY > m_maxY || m_cameraTransform.position.y > m_maxY)
        {
            newDeltaY = m_maxY - m_cameraTransform.position.y;

            newDeltaPosition.x = 0;
            newDeltaPosition.y = 0;
            newDeltaPosition.z = -newDeltaY / Mathf.Sin(m_cameraTransform.rotation.eulerAngles.x * Mathf.Deg2Rad);
            m_cameraTransform.Translate(newDeltaPosition, Space.Self);
        }
        else
        {
            newDeltaPosition.x = 0;
            newDeltaPosition.y = 0;
            newDeltaPosition.z = -newDeltaY / Mathf.Sin(m_cameraTransform.rotation.eulerAngles.x * Mathf.Deg2Rad);
            m_cameraTransform.Translate(newDeltaPosition, Space.Self);
        }
    }

    public void RotateVerticle(float degree)
    {
        newAngle = m_cameraTransform.rotation.eulerAngles.x + degree;
        if (newAngle < 90 && newAngle > 1)// we don't want to handle extreme condition like new angle = 90 or new angle = 0 since it's not necessary
        {
            m_minY = m_targetCenter.y + m_minR * Mathf.Sin(newAngle * Mathf.Deg2Rad);
            m_maxY = m_targetCenter.y + m_maxR * Mathf.Sin(newAngle * Mathf.Deg2Rad);

            originToDestination.x = m_cameraTransform.position.x - m_cameraOrbitOrigin.x;
            originToDestination.y = m_cameraTransform.position.z - m_cameraOrbitOrigin.z;
            angleOriginToDestination = Vector2.Angle(Vector2.right, originToDestination);

            if (originToDestination.y < 0)
            {
                angleOriginToDestination = 360 - angleOriginToDestination;
            }

            radius = (m_cameraTransform.position.y - m_targetCenter.y) / Mathf.Sin(m_cameraTransform.rotation.eulerAngles.x * Mathf.Deg2Rad);

            newCameraPosition.x = radius * Mathf.Cos(newAngle * Mathf.Deg2Rad) * Mathf.Cos(angleOriginToDestination * Mathf.Deg2Rad) + m_cameraOrbitOrigin.x;
            newCameraPosition.y = radius * Mathf.Sin(newAngle * Mathf.Deg2Rad) + m_targetCenter.y;
            newCameraPosition.z = radius * Mathf.Cos(newAngle * Mathf.Deg2Rad) * Mathf.Sin(angleOriginToDestination * Mathf.Deg2Rad) + m_cameraOrbitOrigin.z;

            m_cameraTransform.position = newCameraPosition;
            m_cameraTransform.Rotate(degree, 0, 0, Space.Self);
        }
    }
}
