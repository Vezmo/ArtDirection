using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : MonoBehaviour
{
  public bool UseCharacterController;
  public Transform CameraTransform;
  public Vector3 ProjectileSpawnOffset;
  // public Transform ProjectileSpawnTransform;
  public GameObject ProjectilePrefab;

  public float Gravity;
  public float RunSpeed;
  public float SprintSpeed;
  public float LeftStickDeadZone;

  public float DaggerSpeed = 14;
  public float DaggerDamage = 50;

  private float m_currentSpeed;
  private Vector2 m_leftStickInput;
  private float m_velocityY;
  Vector3 m_velocity;

  public float TurnSmoothTime;
  private float m_turnSmoothVelocity;
  private CharacterController m_controller;

  // Start is called before the first frame update
  void Start()
  {
    m_controller = GetComponent<CharacterController>();
  }

  // Update is called once per frame
  void Update()
  {

    if (!IsGrounded())
      ApplyGravity();
    else
      m_velocityY = 0;


    Move();
  }

  void Move()
  {
    m_leftStickInput.x = Input.GetAxisRaw("LHorizontal");
    m_leftStickInput.y = Input.GetAxisRaw("LVertical");

    if (DirectionalInput())
    {
      if (Input.GetButton("B"))
      {
        m_currentSpeed = m_leftStickInput.magnitude * SprintSpeed;
      }
      else
      {
        m_currentSpeed = m_leftStickInput.magnitude * RunSpeed;
      }

      m_leftStickInput.Normalize();


      float rotation = Mathf.Atan2(m_leftStickInput.x, m_leftStickInput.y) * Mathf.Rad2Deg + CameraTransform.eulerAngles.y;
      transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, rotation, ref m_turnSmoothVelocity, TurnSmoothTime); //We always move forward, but we rotate the character

      m_velocity = transform.forward * m_currentSpeed;
      m_controller.Move(m_velocity * Time.deltaTime);
    }

  }

  bool IsGrounded()
  {
    if (m_controller.isGrounded)
      return true;

    Vector3 bottom = m_controller.transform.position;
    RaycastHit hit;

    if (Physics.Raycast(bottom, Vector3.down, out hit, 0.2f))
    {
      m_controller.Move(Vector3.down * hit.distance);
      return true;
    }

    return false;
  }

  void ApplyGravity()
  {
    m_velocityY += Time.deltaTime * Gravity;
    m_controller.Move(Vector3.up * m_velocityY * Time.deltaTime);
  }


  bool DirectionalInput()
  {
    return (Mathf.Abs(m_leftStickInput.x) >= LeftStickDeadZone || Mathf.Abs(m_leftStickInput.y) >= LeftStickDeadZone);
  }
}
