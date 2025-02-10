using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, IEnemy
{
    private Transform _player;
    private NavMeshAgent _agent;
    private Animator _animator;

    [SerializeField] private float _chaseDistance = 10f;
    [SerializeField] private float _stopDistance = 2f;
    
    public void SetData(float speed)
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        
        _agent.updatePosition = false;
        _agent.updateRotation = false;
        _agent.speed = speed;
    }

    public void SetTarget(Transform target)
    {
        _player = target;
    }

    void FixedUpdate()
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

            if (distance <= _stopDistance)
            {
                _agent.isStopped = true;
                _animator.SetBool("Run", false);
            }
            else
            {
                _agent.isStopped = false;
                _animator.SetBool("Run", true);
            }
        }
        else
        {
            _agent.isStopped = true;
            _animator.SetBool("Run", false);
        }
        
        transform.position = _agent.nextPosition;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_agent.velocity.normalized), Time.deltaTime * 10f);
    }
}