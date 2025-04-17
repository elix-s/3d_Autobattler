using UnityEngine;

public class EnemyBase : MonoBehaviour, IEnemy
{
    [SerializeField] private EnemyAI _enemyAI;
    [SerializeField] private float _speed;

    private void Awake()
    {
        _enemyAI.SetData(_speed);
    }
    
    public void FollowPlayer()
    {
        _enemyAI.FollowPlayer();
    }

    private void FixedUpdate()
    {
        FollowPlayer();
    }
}
