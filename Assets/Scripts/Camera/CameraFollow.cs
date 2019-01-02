using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

  private enum State
  {
    Null = 0,
    Free = 1,
    Locking = 2,
    Locked = 3,
    Unlocking = 4
  }

  public float Sensitivity;
  public Transform FollowObject;
  public float LockOffset;
  public float dstFromTarget;
  public Vector2 PitchBounds;

  private State m_currentState;

  public AnimationCurve LockMoveCurve;

  public float LockDuration;
  private Timer m_lockTimer;
  private Timer m_unlockTimer;
  public CameraCollisionHandler CollisionHandler;

  private Vector3 m_finalPosition;
  private Vector3 m_finalRotation;
  private Vector3 m_lastAdjustment;
  private Vector3 m_cameraLerpVelocity;
  private float m_yaw;
  private float m_pitch;

  public float AdjustSmoothing;

  private void Awake()
  {
    m_lockTimer = new Timer(LockDuration);
    m_lockTimer.OnComplete += LockTimer_OnComplete;

    m_unlockTimer = new Timer(LockDuration);
    m_unlockTimer.OnComplete += UnlockTimer_OnComplete;
  }

  private void Start()
  {
    m_currentState = State.Free;

    CollisionHandler.Initialize(Camera.main);
    CollisionHandler.UpdateCameraClipPoints(transform.position, transform.rotation, ref CollisionHandler.AdjustedCameraClipPoints);
    CollisionHandler.UpdateCameraClipPoints(m_finalPosition, transform.rotation, ref CollisionHandler.DesiredCameraClipPoints);

  }

  private void Update()
  {

    m_unlockTimer.Loop(Time.deltaTime);
    m_lockTimer.Loop(Time.deltaTime);


    m_finalRotation = CalculateFinalRotation();
    transform.eulerAngles = m_finalRotation;

    m_finalPosition = CalculateFinalPosition();
    Vector3 adjustment = CalculateCollisionAdjustments();
    m_lastAdjustment = adjustment;
    transform.position = m_finalPosition + adjustment;

    DrawDebugRays();

  }

  ///Rotation can be altered by:
  ///Player Input
  ///Locking an opponent
  ///More to come...
  private Vector3 CalculateFinalRotation()
  {
    return CalculateOrbitCameraRotation() /* + ...*/;
  }

  ///Position can be altered by:
  ///Following the player
  ///Locking an opponent
  ///More to come...
  private Vector3 CalculateFinalPosition()
  {
    return CalculateFollowPosition() + CalculateLockPosition();
  }

  private Vector3 CalculateCollisionAdjustments()
  {
    Vector3 adjustment = Vector3.zero;

    CollisionHandler.UpdateCameraClipPoints(transform.position, transform.rotation, ref CollisionHandler.AdjustedCameraClipPoints);
    CollisionHandler.UpdateCameraClipPoints(m_finalPosition, transform.rotation, ref CollisionHandler.DesiredCameraClipPoints);

    CollisionHandler.CheckColliding(FollowObject.transform.position);

    if (CollisionHandler.Colliding)
    {
      float adjustmentDistance = CollisionHandler.GetAdjustedDistanceWithRayFrom(FollowObject.transform.position);
      adjustment = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + FollowObject.eulerAngles.y, 0) *
                            -Vector3.forward * (adjustmentDistance - Vector3.Distance(FollowObject.transform.position, m_finalPosition));

    }

    adjustment = Vector3.SmoothDamp(m_lastAdjustment, adjustment, ref m_cameraLerpVelocity, AdjustSmoothing);

    return adjustment;
  }


  //--------------ROTATION--------------//
  private Vector3 CalculateOrbitCameraRotation()
  {
    if (m_currentState == State.Free || m_currentState == State.Unlocking)
    {
      m_yaw += Input.GetAxis("RHorizontal") * Sensitivity;
      m_pitch -= Input.GetAxis("RVertical") * Sensitivity;
      m_pitch = Mathf.Clamp(m_pitch, PitchBounds.x, PitchBounds.y);

      return new Vector3(m_pitch, m_yaw);
    }
    else
    {
      return Vector3.zero;
    }
  }

 

  //--------------POSITION--------------//
  private Vector3 CalculateFollowPosition()
  {
    return (FollowObject.position - transform.forward * dstFromTarget);
  }

  private Vector3 CalculateLockPosition()
  {
    Vector3 finalLockPosition = Vector3.zero;

    switch (m_currentState)
    {
      case State.Locking:
        Vector3 targetLockingPosition = Vector3.up * LockOffset;
        finalLockPosition = Vector3.Lerp(Vector3.zero, targetLockingPosition, LockMoveCurve.Evaluate(m_lockTimer.GetNormalizedTime()));
        break;

      case State.Locked:
        finalLockPosition = Vector3.up * LockOffset;
        break;

      case State.Unlocking:
        Vector3 targetUnlockingPosition = Vector3.zero;
        finalLockPosition = Vector3.Lerp(Vector3.up * LockOffset, Vector2.zero, LockMoveCurve.Evaluate(m_unlockTimer.GetNormalizedTime()));
        break;
    }

    return finalLockPosition;
  }

  private void SwitchState(State _newState)
  {
    m_currentState = _newState;
  }

  private void InitiateLock()
  {
    m_lockTimer.Start();
    m_currentState = State.Locking;

  }

  private void DisengageLock()
  {
    m_unlockTimer.Start();
    m_currentState = State.Unlocking;

    m_yaw = transform.eulerAngles.y;
    m_pitch = transform.eulerAngles.x;

    if (m_pitch > 180)
    {
      m_pitch = m_pitch - 360;
    }
  }

  private void LockTimer_OnComplete()
  {
    m_currentState = State.Locked;
  }

  private void UnlockTimer_OnComplete()
  {
    m_currentState = State.Free;
  }

  private void DrawDebugRays()
  {
    for (int i = 0; i < 5; i++)
    {
      Debug.DrawLine(FollowObject.transform.position, CollisionHandler.DesiredCameraClipPoints[i], Color.red);
      Debug.DrawLine(FollowObject.transform.position, CollisionHandler.AdjustedCameraClipPoints[i], Color.green);
    }
  }

  public bool IsLockedOn()
  {
    return m_currentState == State.Locked || m_currentState == State.Locking || m_currentState == State.Unlocking;
  }

}