using Common.GameStateService;
using Common.SavingSystem;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using Features.AppData;
using Features.GameSessionService;

[RequireComponent(typeof(Rigidbody), typeof(Animator)), RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    private const string HorizontalAxis = "Horizontal";
    private const string VerticalAxis = "Vertical";
    private const string RunParameter = "Run"; 
    private const string EnemyLayer = "Enemy";
    private const string DeadZoneLayer = "DeadZone";

    [Header("Dependencies")]
    [SerializeField] private GameObject _explosionEffect;
    [SerializeField] private ForceField _forceField; 
    [SerializeField] private GameCamera _camera;
    [SerializeField] private EnemySpawner _spawner;

    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;         
    [SerializeField] private float _rotationSpeed = 15f;    
    [SerializeField] private float _moveThreshold = 0.1f; 
    [SerializeField] private float _rotationThreshold = 10f;

    [Header("Ground Check")]
    [SerializeField] private Transform _groundCheckPoint;   
    [SerializeField] private float _groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask _groundLayers; 
    
    [Header("Components")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Animator _animator;
    [SerializeField] private CapsuleCollider _capsuleCollider;
    
    private GameSessionService _gameSessionService;
    private GameStateService _gameStateService;
    private SavingSystem _savingSystem;
    private Logger _logger;
    
    private Vector3 _inputDirection = Vector3.zero;     
    private Vector3 _worldMoveDirection = Vector3.zero;
    private Vector3 _groundPosition = Vector3.zero;
    private bool _isGrounded = false;
    private bool _isDead = false;
    
    [Inject]
    public void Construct(GameSessionService gameSessionService, GameStateService gameStateService,
        SavingSystem savingSystem, Logger logger)
    {
        _gameSessionService = gameSessionService;
        _gameStateService = gameStateService;
        _savingSystem = savingSystem;
        _logger = logger;
    }

    private void Awake()
    {
        if (_rb == null) _logger.LogError("Rigidbody not found on Player");
        if (_animator == null) _logger.LogError("Animator not found on Player");
        if (_camera == null) _logger.LogError("Camera Transform not assigned.");
        
        if (_groundCheckPoint == null)
        {
            _logger.LogWarning("Ground Check Point not assigned, creating one at player's base.");
            _groundCheckPoint = new GameObject("GroundCheckPoint").transform;
            _groundCheckPoint.SetParent(transform);
            _groundCheckPoint.localPosition = _groundPosition;
        }
        
        _rb.freezeRotation = true;
        _isDead = false;
    }
    
    private void Update()
    {
        if (_isDead) return;
        
        float moveX = Input.GetAxisRaw(HorizontalAxis); 
        float moveZ = Input.GetAxisRaw(VerticalAxis);
        _inputDirection = new Vector3(moveX, 0, moveZ).normalized;

        if (_inputDirection != Vector3.zero)
        {
            if (!_gameSessionService.GameStarted)
            {
                _gameSessionService.GameStarted = true;
            }
        }
        
        if (_camera != null)
        {
            Vector3 cameraForward = _camera.transform.forward;
            Vector3 cameraRight = _camera.transform.right;
            cameraForward.y = 0; 
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();
            _worldMoveDirection = (cameraForward * _inputDirection.z + cameraRight * _inputDirection.x).normalized;
        }
        else
        {
            _worldMoveDirection = _inputDirection;
        }

        CheckGroundStatus();
        _animator.SetBool(RunParameter, (_worldMoveDirection.magnitude > 0.1f && _isGrounded));
    }

    private void FixedUpdate()
    {
        if (_isDead) return; 
        
        Vector3 displacement = _worldMoveDirection * _moveSpeed * Time.fixedDeltaTime;
        Vector3 targetPosition = _rb.position + displacement;
        _rb.MovePosition(targetPosition);
        
        if (_worldMoveDirection.magnitude > _moveThreshold) 
        {
            Quaternion targetRotation = Quaternion.LookRotation(_worldMoveDirection);
            Quaternion newRotation = Quaternion.RotateTowards(_rb.rotation, targetRotation, 
                _rotationSpeed * Time.fixedDeltaTime * _rotationThreshold);
            _rb.MoveRotation(newRotation); 
        }
    }

    private void CheckGroundStatus()
    {
        _isGrounded = Physics.CheckSphere(_groundCheckPoint.position, _groundCheckRadius, 
            _groundLayers, QueryTriggerInteraction.Ignore);

        if (!_isGrounded)
        {
            _rb.constraints = RigidbodyConstraints.None;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (_isDead) return;

        if (other.gameObject.layer == LayerMask.NameToLayer(EnemyLayer) || 
            other.gameObject.layer == LayerMask.NameToLayer(DeadZoneLayer))
        {
            Loss(other).Forget();
        }
    }

    private async UniTask Loss(Collision other)
    {
        _isDead = true;
        _gameSessionService.GameStarted = false;
        gameObject.SetActive(false);
        _forceField.CancelToken();
        _spawner.CancelToken();
        
        _camera.ShakeCamera(0.75f, 0.2f).Forget();
        var effect = Instantiate(_explosionEffect, other.contacts[0].point, Quaternion.identity);
        Destroy(effect, 1.0f);

        await _gameSessionService.SaveUserScore();
            
        await UniTask.Delay(1000);
        _gameStateService.ChangeState<MenuState>().Forget();
    }
}