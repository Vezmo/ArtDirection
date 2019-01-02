using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
  public Transform Player;
  public float GizmoRadius;
  public float SmoothSpeed;
  public float DstFromFeetToChest;

  // Start is called before the first frame update
  void Start()
  {
    transform.position = Player.transform.position + Vector3.up * DstFromFeetToChest;
  }

  // Update is called once per frame
  void Update()
  {
    Vector3 desiredPosition = Player.transform.position + Vector3.up * DstFromFeetToChest;
    Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, SmoothSpeed);
    transform.position = smoothedPosition;
  }

  void OnDrawGizmosSelected()
  {
    // Display the explosion radius when selected
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, GizmoRadius);
  }
}
