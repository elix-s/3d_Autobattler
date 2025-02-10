using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject _explosionEffect;
    [SerializeField] private ForceField _forceField;
    [SerializeField] private float _speed = 5f;  
    [SerializeField] private float _interpolationSpeed = 10f;
    
    private GameSessionService _gameSessionService;
    private GameStateService _gameStateService;
    private Rigidbody _rb;  
    private Vector3 _moveDirection;  
    private Animator _animator;

    [Inject]
    public void Construct(GameSessionService gameSessionService, GameStateService gameStateService)
    {
        _gameSessionService = gameSessionService;
        _gameStateService = gameStateService;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();  
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _gameSessionService.GameStarted = true;
    }

    private void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 targetDirection = new Vector3(moveX, 0, moveZ).normalized;
        
        _moveDirection = Vector3.Lerp(_moveDirection, targetDirection, _interpolationSpeed * Time.deltaTime);
        
        if (_moveDirection.magnitude > 0.1f)
            _animator.SetBool("Run", true);
        else
            _animator.SetBool("Run", false);
    }

    private void FixedUpdate()
    {
        _rb.MovePosition(_rb.position + _moveDirection * _speed * Time.fixedDeltaTime);
        
        if (_moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);
            _rb.rotation = Quaternion.Lerp(_rb.rotation, targetRotation, _interpolationSpeed * Time.fixedDeltaTime);
        }
    }
    
    private async void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            _gameSessionService.GameStarted = false;
            
            var effect = Instantiate(_explosionEffect, other.ClosestPoint(transform.position), Quaternion.identity);
            Destroy(effect, 2.0f); 
            gameObject.SetActive(false);
            _forceField.CancelToken();
            
            await UniTask.Delay(2000);
            _gameStateService.ChangeState<MenuState>();
        }
    }
}

