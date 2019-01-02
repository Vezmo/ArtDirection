using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollisionHandler : MonoBehaviour
{

  public LayerMask CollisionLayer;
  public float SpreadClipPointsFactor;
  public bool Colliding;
  public Vector3[] AdjustedCameraClipPoints;
  public Vector3[] DesiredCameraClipPoints;

  private Camera m_camera;

  // Start is called before the first frame update

  public void Initialize(Camera _camera)
  {
    m_camera = _camera;
    AdjustedCameraClipPoints = new Vector3[5];
    DesiredCameraClipPoints = new Vector3[5];
  }

  public void UpdateCameraClipPoints(Vector3 _cameraPosition, Quaternion _atRotation, ref Vector3[] _intoArray)
  {
    _intoArray = new Vector3[5];

    float z = m_camera.nearClipPlane;
    float x = Mathf.Tan(m_camera.fieldOfView / SpreadClipPointsFactor) * z;
    float y = x / m_camera.aspect;

    //Top left
    _intoArray[0] = (_atRotation * new Vector3(-x, y, z)) + _cameraPosition;

    //Top Right
    _intoArray[1] = (_atRotation * new Vector3(x, y, z)) + _cameraPosition;

    //Bottom left
    _intoArray[2] = (_atRotation * new Vector3(-x, -y, z)) + _cameraPosition;

    //Bottom Right
    _intoArray[3] = (_atRotation * new Vector3(x, -y, z)) + _cameraPosition;

    //Camera position
    _intoArray[4] = _cameraPosition - m_camera.transform.forward;

  }

  private bool CollisionDetectedAtClipPoints(Vector3[] _clipPoints, Vector3 _fromPosition)
  {
    for (int i = 0; i < _clipPoints.Length; i++)
    {
      Ray ray = new Ray(_fromPosition, _clipPoints[i] - _fromPosition);
      float distance = Vector3.Distance(_clipPoints[i], _fromPosition);

      if (Physics.Raycast(ray, distance, CollisionLayer))
      {
        return true;
      }
    }

    return false;
  }

  public float GetAdjustedDistanceWithRayFrom(Vector3 _from)
  {
    float distance = -1;

    for (int i = 0; i < DesiredCameraClipPoints.Length; i++)
    {
      Ray ray = new Ray(_from, DesiredCameraClipPoints[i] - _from);
      RaycastHit hit;

      if (Physics.Raycast(ray, out hit))
      {
        if (distance == -1)
        {
          distance = hit.distance;
        }
        else
        {
          if (hit.distance < distance)
          {
            distance = hit.distance;
          }
        }
      }
    }

    if (distance == -1)
    {
      return 0;
    }
    else
    {
      return distance;
    }
  }

  public void CheckColliding(Vector3 targetPosition)
  {
    if (CollisionDetectedAtClipPoints(DesiredCameraClipPoints, targetPosition))
    {
      Colliding = true;
    }
    else
    {
      Colliding = false;
    }
  }
}
