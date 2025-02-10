using System;
using UnityEngine;

public class FastEnemy : MonoBehaviour, IEnemy
{
    [SerializeField] private EnemyAI _enemyAI;
    [SerializeField] private float _speed;
    [SerializeField] private Transform _target;

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