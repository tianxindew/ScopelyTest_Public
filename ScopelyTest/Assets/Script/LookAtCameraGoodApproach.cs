using UnityEngine;

public class LookAtCameraGoodApproach : MonoBehaviour, ILookAtCamera
{
    private Camera m_camera;
    private Transform m_cameraTransform;
    private Vector3 m_cameraOrbitOrigin;
    
    public GameObject m_lookingAtTarget;
    private Vector3 m_targetCenter;
    private BoxCollider m_targetCollider;
    private Bounds m_targetColliderBounds;

    //bounds of the camera height
    private float m_minR = 0;
    private float m_maxR = 0;

    private float m_minY = 0;
    private float m_maxY = 0;

    public void InitCamera(float angle, float angleVerticle)
    {
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
        m_camera.transform.rotation = Quaternion.Euler(new Vector3(angleVerticle,0,0));
       
    }

    //instead of move camera directly, we move the camera with the origin of it's local system
    public void Pan(Vector3 deltaPosition)
    {
        //check origin's next frame
        Vector3 futureOriginPosition = new Vector3(m_cameraOrbitOrigin.x + deltaPosition.x, m_cameraOrbitOrigin.y, m_cameraOrbitOrigin.z + deltaPosition.z);

        float maxRadius = m_targetColliderBounds.size.x > m_targetColliderBounds.size.z ? m_targetColliderBounds.size.x / 2 : m_targetColliderBounds.size.z / 2;

        if (Mathf.Pow(futureOriginPosition.x - m_targetCenter.x, 2) + Mathf.Pow(futureOriginPosition.z - m_targetCenter.z, 2) <= maxRadius * maxRadius)
        {
            m_cameraTransform.Translate(deltaPosition, Space.World);
            m_cameraOrbitOrigin += deltaPosition;
        }
        else
        {
            Vector2 originToDestination = new Vector2(futureOriginPosition.x - m_targetCenter.x, futureOriginPosition.z - m_targetCenter.z);
            float angleOriginToDestination = 0f;

            angleOriginToDestination = Vector2.Angle(Vector2.right, originToDestination);
            if (originToDestination.y < 0)
            {
                angleOriginToDestination = 360 - angleOriginToDestination;
            }

            Vector3 newFutureOriginPosition = new Vector3(maxRadius * Mathf.Cos(angleOriginToDestination * Mathf.Deg2Rad), m_cameraOrbitOrigin.y,
                maxRadius * Mathf.Sin(angleOriginToDestination * Mathf.Deg2Rad)) + new Vector3(m_targetCenter.x, 0, m_targetCenter.z);

            Vector3 newDeltaPosition = newFutureOriginPosition - m_cameraOrbitOrigin;
            m_cameraTransform.Translate(newDeltaPosition, Space.World);
            m_cameraOrbitOrigin += newDeltaPosition;
        }
    }

    public void Rotate(float degree)
    {
        //get current angle
        Vector2 startPosition2 = new Vector2(m_cameraTransform.position.x, m_cameraTransform.position.z);
        Vector2 origin_Exterior2 = new Vector2(m_targetCenter.x, m_targetCenter.z);

        Vector2 norm = startPosition2 - origin_Exterior2;

        float angle = Vector2.Angle(Vector2.right, norm);

        if (norm.y < 0)
        {
            angle = 360f - angle;
        }

        angle += degree;

        angle %= 360;
        if (angle < 0)
            angle += 360;

        Vector2 R = new Vector2(m_cameraTransform.position.x - m_targetCenter.x, m_cameraTransform.position.z - m_targetCenter.z);

        m_cameraTransform.position = new Vector3(R.magnitude * Mathf.Cos(angle * Mathf.Deg2Rad), 0, R.magnitude * Mathf.Sin(angle * Mathf.Deg2Rad))
            + new Vector3(m_targetCenter.x, m_cameraTransform.position.y, m_targetCenter.z);
        m_cameraTransform.Rotate(0, -degree, 0, Space.World);

        //rotate origin as well
        startPosition2 = new Vector2(m_cameraOrbitOrigin.x, m_cameraOrbitOrigin.z);
        norm = startPosition2 - origin_Exterior2;

        angle = Vector2.Angle(Vector2.right, norm);

        if (norm.y < 0) 
        {
            angle = 360f - angle;
        }

        angle += degree;

        angle %= 360;
        if (angle < 0)
            angle += 360;

        R = new Vector2(m_cameraOrbitOrigin.x - m_targetCenter.x, m_cameraOrbitOrigin.z - m_targetCenter.z);

        m_cameraOrbitOrigin = new Vector3(R.magnitude * Mathf.Cos(angle * Mathf.Deg2Rad), 0, R.magnitude * Mathf.Sin(angle * Mathf.Deg2Rad))
            + new Vector3(m_targetCenter.x, m_cameraOrbitOrigin.y, m_targetCenter.z);
    }

    public void Zoom(float distance)
    {
        if (m_cameraTransform.position.y + distance < m_minY || m_cameraTransform.position.y < m_minY)
        {
            float newDeltaY = m_minY - m_cameraTransform.position.y;

            Vector3 move = new Vector3();
            move.z = -newDeltaY / Mathf.Sin(m_cameraTransform.rotation.eulerAngles.x * Mathf.Deg2Rad);
            m_cameraTransform.Translate(move, Space.Self);
        }
        else if (m_cameraTransform.position.y + distance > m_maxY || m_cameraTransform.position.y > m_maxY)
        {
            float newDeltaY = m_maxY - m_cameraTransform.position.y;

            Vector3 move = new Vector3();
            move.z = -newDeltaY / Mathf.Sin(m_cameraTransform.rotation.eulerAngles.x * Mathf.Deg2Rad);
            m_cameraTransform.Translate(move, Space.Self);
        }
        else
        {
            Vector3 move = new Vector3();
            move.z = -distance / Mathf.Sin(m_cameraTransform.rotation.eulerAngles.x * Mathf.Deg2Rad);
            m_cameraTransform.Translate(move, Space.Self);
        }
    }

    public void RotateVerticle(float degree)
    {
        float newAngle = m_cameraTransform.rotation.eulerAngles.x + degree;
        if (newAngle < 90 && newAngle > 0)
        {
            m_minY = m_targetCenter.y + m_minR * Mathf.Sin(newAngle * Mathf.Deg2Rad);
            m_maxY = m_targetCenter.y + m_maxR * Mathf.Sin(newAngle * Mathf.Deg2Rad);

            Vector2 originToDestination = new Vector2(m_cameraTransform.position.x - m_cameraOrbitOrigin.x, m_cameraTransform.position.z - m_cameraOrbitOrigin.z);
            float angleOriginToDestination = 0f;

            angleOriginToDestination = Vector2.Angle(Vector2.right, originToDestination);
            if (originToDestination.y < 0)
            {
                angleOriginToDestination = 360 - angleOriginToDestination;
            }

            float R = (m_cameraTransform.position.y - m_targetCenter.y) / Mathf.Sin(m_cameraTransform.rotation.eulerAngles.x * Mathf.Deg2Rad);
            float positionX = R * Mathf.Cos(newAngle * Mathf.Deg2Rad) * Mathf.Cos(angleOriginToDestination * Mathf.Deg2Rad);
            float positionY = R * Mathf.Sin(newAngle * Mathf.Deg2Rad);
            float positionZ = R * Mathf.Cos(newAngle * Mathf.Deg2Rad) * Mathf.Sin(angleOriginToDestination * Mathf.Deg2Rad);

            m_cameraTransform.position = new Vector3(positionX + m_cameraOrbitOrigin.x, positionY + m_targetCenter.y, positionZ + m_cameraOrbitOrigin.z);
            m_cameraTransform.Rotate(degree, 0, 0, Space.Self);
        }
    }
}
