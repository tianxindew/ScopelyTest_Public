using UnityEngine;
using System.Collections;

public interface ILookAtCamera
{
    void InitCamera(float angle, float verticleAngle);
    void Pan( Vector3 deltaPosition);
    void Rotate(float degree);
    void RotateVerticle(float degree);
    void Zoom(float distance);
}
