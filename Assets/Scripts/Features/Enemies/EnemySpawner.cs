using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private GameObject _bigEnemyPrefab;
    [SerializeField] private GameObject _fastEnemyPrefab;
    [SerializeField] private int _spawnInterval = 3000;

    private BigEnemyFactory _bigEnemyFactory;
    private FastEnemyFactory _fastEnemyFactory;
    private GameSessionService _gameSessionService;
    
    [Inject]
    public void Construct(GameSessionService gameSessionService)
    {
        _gameSessionService = gameSessionService;
    }

    void Start()
    {
        _bigEnemyFactory = new BigEnemyFactory(_bigEnemyPrefab);
        _fastEnemyFactory = new FastEnemyFactory(_fastEnemyPrefab);

       SpawnEnemies();
    }

    private async void SpawnEnemies()
    {
        float minSpawnDistance = 5f; 

        while (_gameSessionService.GameStarted)
        {
            await UniTask.Delay(_spawnInterval);

            Vector3 spawnPos;
            
            do
            {
                spawnPos = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
            }
            while (Vector3.Distance(spawnPos, _playerTransform.position) < minSpawnDistance);

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
}