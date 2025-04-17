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
    [SerializeField] private Transform _cameraTransform;
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
    
    private Vector3 _inputDirection = Vector3.zero;     
    private Vector3 _worldMoveDirection = Vector3.zero;
    private Vector3 _groundPosition = Vector3.zero;
    private bool _isGrounded = false;
    private bool _isDead = false;

    [Inject]
    public void Construct(GameSessionService gameSessionService, GameStateService gameStateService,
        SavingSystem savingSystem)
    {
        _gameSessionService = gameSessionService;
        _gameStateService = gameStateService;
        _savingSystem = savingSystem;
    }

    private void Awake()
    {
        if (_rb == null) Debug.LogError("Rigidbody not found on Player", this);
        if (_animator == null) Debug.LogError("Animator not found on Player", this);
        if (_cameraTransform == null) Debug.LogError("Camera Transform not assigned.", this);
        
        if (_groundCheckPoint == null)
        {
             Debug.LogWarning("Ground Check Point not assigned, creating one at player's base.", this);
             _groundCheckPoint = new GameObject("GroundCheckPoint").transform;
             _groundCheckPoint.SetParent(transform);
             _groundCheckPoint.localPosition = _groundPosition;
        }
        
        _rb.freezeRotation = true;
        _isDead = false;
    }

    private void Start()
    {
        _gameSessionService.GameStarted = true;
    }

    private void Update()
    {
        if (_isDead) return;
        
        float moveX = Input.GetAxisRaw(HorizontalAxis); 
        float moveZ = Input.GetAxisRaw(VerticalAxis);
        _inputDirection = new Vector3(moveX, 0, moveZ).normalized;
        
        if (_cameraTransform != null)
        {
            Vector3 cameraForward = _cameraTransform.forward;
            Vector3 cameraRight = _cameraTransform.right;
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
            
        var effect = Instantiate(_explosionEffect, other.contacts[0].point, Quaternion.identity);
        Destroy(effect, 1.0f);

        await SaveUserScore();
            
        await UniTask.Delay(1000);
        _gameStateService.ChangeState<MenuState>().Forget();
    }

    private async UniTask SaveUserScore()
    {
        var data = await _savingSystem.LoadDataAsync<AppData>();

        if (data.BestResult < _gameSessionService.UserScore)
        {
            data.BestResult = _gameSessionService.UserScore;
            _savingSystem.SaveDataAsync(data).Forget();
        }

        _gameSessionService.UserScore = 0;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (_groundCheckPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_groundCheckPoint.position, _groundCheckRadius);
        }
         
        if (Application.isPlaying && !_isDead)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position + Vector3.up * 0.1f, 
                 _worldMoveDirection * _moveSpeed * 0.2f); 
        }
    }
}