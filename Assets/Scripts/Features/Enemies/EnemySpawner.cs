using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;
using Features.GameSessionService;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private GameObject _bigEnemyPrefab;
    [SerializeField] private GameObject _fastEnemyPrefab;
    [SerializeField] private int _spawnInterval = 2000;
    [Range(2,9)][SerializeField] private float _minSpawnDistance = 9f;

    private BigEnemyFactory _bigEnemyFactory;
    private FastEnemyFactory _fastEnemyFactory;
    private GameSessionService _gameSessionService;
    
    private CancellationTokenSource _cancellationTokenSource;
    
    private bool _isSpawning;
    
    [Inject]
    private void Construct(GameSessionService gameSessionService)
    {
        _gameSessionService = gameSessionService;
    }

    private void Awake()
    {
        _bigEnemyFactory = new BigEnemyFactory(_bigEnemyPrefab);
        _fastEnemyFactory = new FastEnemyFactory(_fastEnemyPrefab);
        _cancellationTokenSource = new CancellationTokenSource();
    }

    private async void Update()
    {
        if (!_cancellationTokenSource.IsCancellationRequested)
        {
            if (_gameSessionService.GameStarted)
            {
                if (!_isSpawning)
                {
                    await SpawnEnemies();
                }
            }
        }
    }

    private async UniTask SpawnEnemies()
    {
        _isSpawning = true;
        
        try
        {
            await UniTask.Delay(_spawnInterval, cancellationToken: _cancellationTokenSource.Token);
            Vector3 spawnPos;

            do
            {
                spawnPos = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
            } while (Vector3.Distance(spawnPos, _playerTransform.position) < _minSpawnDistance);

            if (_playerTransform != null)
            {
                if (Random.value > 0.5f)
                {
                    IEnemy enemy = _bigEnemyFactory.CreateEnemy(_playerTransform, spawnPos, gameObject.transform);
                }
                else
                {
                    IEnemy enemy = _fastEnemyFactory.CreateEnemy(_playerTransform, spawnPos, gameObject.transform);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Spawner canceled");
        }
        
        _isSpawning = false;
    }
    
    public void CancelToken()
    {
        _cancellationTokenSource?.Cancel(); 
    }
}