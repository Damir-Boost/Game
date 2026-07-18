using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("камера/camera")]
    [SerializeField] private Camera _cameraComponent;

    [Header("скорости/Speeds")]
    [SerializeField] private float _walkSpeed = 10.0f;
    [SerializeField] private float _runSpeed = 15.0f;
    [SerializeField] private float _cameraSensitivity = 5.0f;
    [SerializeField] private float _jumpHeight = 2.0f;

    [Header("здоровье/Health")]
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private int _currentHealth;
    [SerializeField] private Slider _healthBar;
    [SerializeField] private GameObject _deathScreen;
    [SerializeField] private float _damageFlashDuration = 0.2f;

    // Компоненты
    private CharacterController _playerMovementComponent;
    private Renderer _playerRenderer;
    private Color _originalColor;

    // Переменные движения
    private float _speedCapacity;
    private float _moveX;
    private float _moveZ;
    private float _mouseX;
    private float _mouseY;
    private float _mouseVertical;
    private bool _isGrounded;
    private float _verticalVelocity;
    private float _gravity = -9.8f;

    // Переменные здоровья
    private bool _isDead = false;
    private float _damageFlashTimer = 0f;
    private bool _isFlashing = false;

    // Свойства для доступа извне
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public bool IsDead => _isDead;
    public System.Action<int> OnDamageTaken;
    public System.Action OnDeath;

    private void Awake()
    {
        ReceivingComponents();
        CursorLocked();
        InitializeHealth();
    }

    private void Update()
    {
        if (_isDead) return; // Если мертв - не двигаемся

        if (!CheckScriptController()) return;

        GetMovementController();
        ApplyMovement();
        GetRotateCameraController();
        ApplyCameraRotation();
        SpeedController();
        GravityController();
        JumpController();

        // Обновление эффекта вспышки урона
        UpdateDamageFlash();
    }

    private void ReceivingComponents()
    {
        _playerMovementComponent = GetComponent<CharacterController>();
        _playerRenderer = GetComponent<Renderer>();
        _speedCapacity = _walkSpeed;

        if (_playerRenderer != null)
            _originalColor = _playerRenderer.material.color;
    }

    private void CursorLocked()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void InitializeHealth()
    {
        _currentHealth = _maxHealth;
        UpdateHealthBar();
    }

    // ==================== ДВИЖЕНИЕ ====================

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

    // ==================== ЗДОРОВЬЕ ====================

    public void TakeDamage(int damage)
    {
        if (_isDead) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Max(0, _currentHealth);

        UpdateHealthBar();
        OnDamageTaken?.Invoke(damage);

        // Эффект получения урона
        StartDamageFlash();

        Debug.Log($"Игрок получил {damage} урона. Осталось здоровья: {_currentHealth}");

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (_isDead) return;

        _currentHealth += amount;
        _currentHealth = Mathf.Min(_maxHealth, _currentHealth);
        UpdateHealthBar();

        Debug.Log($"Игрок восстановил {amount} здоровья. Текущее здоровье: {_currentHealth}");
    }

    public void RestoreFullHealth()
    {
        _currentHealth = _maxHealth;
        UpdateHealthBar();
        Debug.Log("Здоровье полностью восстановлено!");
    }

    private void UpdateHealthBar()
    {
        if (_healthBar != null)
        {
            _healthBar.value = (float)_currentHealth / _maxHealth;
        }
    }

    private void StartDamageFlash()
    {
        _isFlashing = true;
        _damageFlashTimer = _damageFlashDuration;

        if (_playerRenderer != null)
        {
            _playerRenderer.material.color = Color.red;
        }
    }

    private void UpdateDamageFlash()
    {
        if (!_isFlashing) return;

        _damageFlashTimer -= Time.deltaTime;

        if (_damageFlashTimer <= 0)
        {
            _isFlashing = false;
            if (_playerRenderer != null && !_isDead)
            {
                _playerRenderer.material.color = _originalColor;
            }
        }
    }

    private void Die()
    {
        _isDead = true;
        OnDeath?.Invoke();

        Debug.Log("Игрок умер!");

        // Показываем экран смерти
        if (_deathScreen != null)
            _deathScreen.SetActive(true);

        // Отключаем управление
        this.enabled = false;

        // Можно добавить перезагрузку сцены через несколько секунд
        // Invoke(nameof(RestartLevel), 3f);
    }

    private void RestartLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    // ==================== ПРОВЕРКИ ====================

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

    // ==================== ОТЛАДКА ====================

    private void OnDrawGizmos()
    {
        // Визуализация здоровья (опционально)
        // Можно добавить GUI или другие элементы
    }

    // Метод для тестирования урона через консоль
    [ContextMenu("Take 10 Damage")]
    private void TestTakeDamage()
    {
        TakeDamage(10);
    }

    [ContextMenu("Heal 20 Health")]
    private void TestHeal()
    {
        Heal(20);
    }

    [ContextMenu("Full Heal")]
    private void TestFullHeal()
    {
        RestoreFullHealth();
    }
}