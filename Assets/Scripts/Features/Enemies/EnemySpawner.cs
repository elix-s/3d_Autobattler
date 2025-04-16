using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private GameObject _bigEnemyPrefab;
    [SerializeField] private GameObject _fastEnemyPrefab;
    [SerializeField] private int _spawnInterval = 3000;

    private BigEnemyFactory _bigEnemyFactory;
    private FastEnemyFactory _fastEnemyFactory;
    private GameSessionService _gameSessionService;
    
    private CancellationTokenSource _cancellationTokenSource;
    
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

    private void Start()
    {
        if (!_cancellationTokenSource.IsCancellationRequested)
        {
            SpawnEnemies();
        }
    }

    private async void SpawnEnemies()
    {
        float minSpawnDistance = 5f; 

        while (_gameSessionService.GameStarted)
        {
            try
            {
                await UniTask.Delay(_spawnInterval, cancellationToken: _cancellationTokenSource.Token);
                Vector3 spawnPos;

                do
                {
                    spawnPos = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
                } while (Vector3.Distance(spawnPos, _playerTransform.position) < minSpawnDistance);

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
        }
    }
    
    public void CancelToken()
    {
        _cancellationTokenSource?.Cancel(); 
    }
}