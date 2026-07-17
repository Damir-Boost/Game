using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("камера/camera")]
    [SerializeField] private Camera _cameraComponent;

    [Header("скорости/Speeds")]
    [SerializeField] private float _walkSpeed = 10.0f;
    [SerializeField] private float _runSpeed = 15.0f;
    [SerializeField] private float _cameraSensitivity = 5.0f;

    [SerializeField] private float _jumpHeight = 2.0f;

    private float _speedCapacity;

    private CharacterController _playerMovementComponent;

    private float _moveX;
    private float _moveZ;

    private float _mouseX;
    private float _mouseY;
    private float _mouseVertical;

    private bool _isGrounded;
    private float _verticalVelocity;
    private float _gravity = -9.0f;



    private void Update()
    {
        if (!CheckScriptController()) return;

        GetMovementController();
        ApplyMovement();
        GetRotateCameraController();

        ApplyCameraRotation();
        SpeedController();

        GravityController();
        JumpController();
    }

    private void GetMovementController()
    {
        _moveX = Input.GetAxis("Horizontal");
        _moveZ = Input.GetAxis("Vertical");
    }

    private void ApplyMovement()
    {
        Vector3 MoveDirection = (transform.right * _moveX + transform.forward * _moveZ) * _speedCapacity;
        MoveDirection.y = _verticalVelocity;
        _playerMovementComponent.Move(MoveDirection * Time.deltaTime);
    }

    private void GetRotateCameraController()
    {
        _mouseX = Input.GetAxis("Mouse X") * _cameraSensitivity;
        _mouseY = Input.GetAxis("Mouse Y") * _cameraSensitivity;
    }

    private void ApplyCameraRotation()
    {
        transform.Rotate(Vector3.up * _mouseX);
        _mouseVertical -= _mouseY;
        _mouseVertical = Mathf.Clamp(_mouseVertical, -90.0f, 90.0f);

        _cameraComponent.transform.localRotation = Quaternion.Euler(_mouseVertical, 0.0f, 0.0f);
    }

    private void SpeedController()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            _speedCapacity = _runSpeed;
        else
            _speedCapacity = _walkSpeed;
    }

    private bool CheckScriptController()
    {
        if (_cameraComponent == null)
        {
            Debug.Log("назначь камеру в инспекторе со скриптом!/assign a camera in the inspector with a script!");
            return false;
        }

        if (_playerMovementComponent == null)
        {
            Debug.Log("назначь контроллер в инспекторе со скриптом!/assign a controller in the inspector with a script!");
            return false;
        }

        return true;
    }

    private void GravityController()
    {
        _isGrounded = _playerMovementComponent.isGrounded;

        if (_isGrounded)
        {
            if (_verticalVelocity < 0.0f)
                _verticalVelocity = -2.0f;
        }

        _verticalVelocity += _gravity * Time.deltaTime;
    }

    private void JumpController()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
            _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2.0f * _gravity);
    }

    private void Awake()
    {
        ReceivingComponents();
        CursorLocked();
    }

    private void ReceivingComponents()
    {
        _playerMovementComponent = GetComponent<CharacterController>();
        _speedCapacity = _walkSpeed;
    }

    private void CursorLocked()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
