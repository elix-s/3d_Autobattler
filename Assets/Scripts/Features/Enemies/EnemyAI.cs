using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent)), RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour, IEnemy
{
    private const string RunParameter = "Run";

    [Header("Movement Settings")]
    [SerializeField] private float _chaseDistance = 10f;
    [SerializeField] private float _stopDistance = 2f;
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private float _destinationUpdateRate = 0.2f;
    
    private NavMeshAgent _agent;
    private Animator _animator;
    private Transform _cachedTransform; 
    
    private Transform _player;
    
    private float _chaseDistanceSqr;
    private float _stopDistanceSqr;
    private float _timeSinceLastSet = 0f;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _cachedTransform = transform; 
        
        _chaseDistanceSqr = _chaseDistance * _chaseDistance;
        _stopDistanceSqr = _stopDistance * _stopDistance;
    }

    public void SetData(float speed)
    {
        if (_agent == null)
        {
            Debug.LogError("NavMeshAgent not found");
            return;
        }
        
        _agent.speed = speed;
    }

    public void SetTarget(Transform target)
    {
        _player = target;
    }
    
    private void FixedUpdate()
    {
        _timeSinceLastSet += Time.fixedDeltaTime;
        
        if (_timeSinceLastSet > _destinationUpdateRate)
        {
            _timeSinceLastSet = 0f;
            FollowPlayer();
        }
    }

    public void FollowPlayer()
    {
        if (_player == null)
        {
             if (!_agent.isStopped)
             {
                 _agent.isStopped = true;
                 _animator.SetBool(RunParameter, false);
             }
             
            return;
        }
        
        Vector3 directionToPlayer = _player.position - _cachedTransform.position;
        float distanceSqr = directionToPlayer.sqrMagnitude; 
        
        if (distanceSqr <= _chaseDistanceSqr)
        {
            _agent.SetDestination(_player.position);

            bool shouldStop = distanceSqr <= _stopDistanceSqr;
            _agent.isStopped = shouldStop;
            _animator.SetBool(RunParameter, !shouldStop);
        }
        else
        {
            if (!_agent.isStopped) 
            {
                _agent.isStopped = true;
                _animator.SetBool(RunParameter, false);
            }
        }
        
        _cachedTransform.position = _agent.nextPosition;
        
        if (_agent.velocity.sqrMagnitude > 0.01f) 
        {
            Quaternion lookRotation = Quaternion.LookRotation(_agent.velocity.normalized);
            _cachedTransform.rotation = Quaternion.Lerp(_cachedTransform.rotation, lookRotation, Time.fixedDeltaTime * _rotationSpeed);
        }
    }
}