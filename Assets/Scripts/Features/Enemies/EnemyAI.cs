using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent)), RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour, IEnemy
{
    private const string RunParameter = "Run"; 
    
    [SerializeField] private float _chaseDistance = 10f;
    [SerializeField] private float _stopDistance = 2f;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Animator _animator;
    private Transform _player;
    
    public void SetData(float speed)
    {
        _agent.updatePosition = false;
        _agent.updateRotation = false;
        _agent.speed = speed;
    }

    public void SetTarget(Transform target)
    {
        _player = target;
    }

    private void FixedUpdate()
    {
        FollowPlayer();
    }

    public void FollowPlayer()
    {
        if (_player == null) return;

        float distance = Vector3.Distance(transform.position, _player.position);

        if (distance <= _chaseDistance)
        {
            _agent.SetDestination(_player.position);
            _agent.isStopped = distance <= _stopDistance;
            _animator.SetBool(RunParameter, distance > _stopDistance);
        }
        else
        {
            _agent.isStopped = true;
            _animator.SetBool(RunParameter, false);
        }
        
        transform.position = _agent.nextPosition;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_agent.velocity.normalized), Time.deltaTime * 10f);
    }
}