using Common.GameStateService;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    private const string HorizontalAxis = "Horizontal";
    private const string VerticalAxis = "Vertical";
    private const string RunParameter = "Run"; 
    private const string DeathTrigger = "Die";    
    private const string EnemyLayer = "Enemy";

    [Header("Dependencies")]
    [SerializeField] private GameObject _explosionEffect;
    [SerializeField] private ForceField _forceField; 
    [SerializeField] private Transform _cameraTransform; 

    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;         
    [SerializeField] private float _rotationSpeed = 15f;    
    [SerializeField] private float _moveThreshold = 0.1f;   

    [Header("Ground Check")]
    [SerializeField] private Transform _groundCheckPoint;   
    [SerializeField] private float _groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask _groundLayers; 
    
    [Header("Components")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Animator _animator;
    
    private GameSessionService _gameSessionService;
    private GameStateService _gameStateService;
    
    private Vector3 _inputDirection = Vector3.zero;     
    private Vector3 _worldMoveDirection = Vector3.zero; 
    private bool _isGrounded = false;
    private bool _isDead = false;

    [Inject]
    public void Construct(GameSessionService gameSessionService, GameStateService gameStateService)
    {
        _gameSessionService = gameSessionService;
        _gameStateService = gameStateService;
    }

    private void Awake()
    {
        if (_rb == null) Debug.LogError("Rigidbody not found on Player", this);
        if (_animator == null) Debug.LogError("Animator not found on Player", this);
        if (_cameraTransform == null) Debug.LogError("Camera Transform not assigned. Movement will be world-based.", this);
        if (_groundCheckPoint == null)
        {
             Debug.LogWarning("Ground Check Point not assigned, creating one at player's base.", this);
             _groundCheckPoint = new GameObject("GroundCheckPoint").transform;
             _groundCheckPoint.SetParent(transform);
             Collider col = GetComponent<Collider>();
             _groundCheckPoint.localPosition = col != null ? Vector3.down * col.bounds.extents.y : Vector3.zero;
        }
        
        if (!_rb.isKinematic)
        {
            Debug.LogWarning("Rigidbody is not set to Kinematic. MovePosition might behave unexpectedly. Consider unchecking IsKinematic if physics interaction is desired.", this);
        }
    }

    private void Start()
    {
        _rb.freezeRotation = true;
        _gameSessionService.GameStarted = true;
        _isDead = false;
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
        
        _animator.SetBool(RunParameter, (_worldMoveDirection.magnitude > 0.1f));
        
        CheckGroundStatus();
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
                _rotationSpeed * Time.fixedDeltaTime * 25f);
            _rb.MoveRotation(newRotation); 
        }
        
        /*
        if (_worldMoveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_worldMoveDirection);
            _rb.rotation = Quaternion.Lerp(_rb.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
        }
        */
    }

    private void CheckGroundStatus()
    {
        _isGrounded = Physics.CheckSphere(_groundCheckPoint.position, _groundCheckRadius, 
            _groundLayers, QueryTriggerInteraction.Ignore);
    }

    private async void OnTriggerEnter(Collider other)
    {
        if (_isDead) return;

        if (other.gameObject.layer == LayerMask.NameToLayer(EnemyLayer))
        {
            _isDead = true;
            _gameSessionService.GameStarted = false;
            
            var effect = Instantiate(_explosionEffect, other.ClosestPoint(transform.position), Quaternion.identity);
            Destroy(effect, 2.0f); 
            gameObject.SetActive(false);
            _forceField.CancelToken();
            
            await UniTask.Delay(2000);
            _gameStateService.ChangeState<MenuState>().Forget();
        }
    }
    
     void OnDrawGizmosSelected()
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