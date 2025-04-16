using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

public class ForceField : MonoBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private GameObject _explosionEffect_FastEnemy;
    [SerializeField] private GameObject _explosionEffect_BigEnemy;
    
    private CancellationTokenSource _cancellationTokenSource;
    private bool _fieldIsActive = true;
    
    private GameSessionService _gameSessionService;
    private EventsDispatcher _eventDispatcher;

    [Inject]
    private void Construct(GameSessionService gameSessionService, EventsDispatcher eventDispatcher)
    {
        _gameSessionService = gameSessionService;
        _eventDispatcher = eventDispatcher;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (_fieldIsActive)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                CollisionCheck(other);
            }
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (_fieldIsActive)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                CollisionCheck(other);
            }
        }
    }

    private void CollisionCheck(Collider collider)
    {
        _fieldIsActive = false;

        if (collider.gameObject.GetComponent<FastEnemy>() != null)
        {
            SpawnExplosion(_explosionEffect_FastEnemy,collider.ClosestPoint(transform.position));
        }
        else if (collider.gameObject.GetComponent<BigEnemy>() != null)
        {
            SpawnExplosion(_explosionEffect_BigEnemy,collider.ClosestPoint(transform.position));
        }
        
        _gameSessionService.IncreaceScores(10); 
        var e = _eventDispatcher.GameDispatcher.Get<IncreaseScoreEvent>();
        e.SetScore(_gameSessionService.UserScore);
       _eventDispatcher.GameDispatcher.Invoke(e).Forget();
        
        Destroy(collider.gameObject);
        _meshRenderer.enabled = false;

        _cancellationTokenSource?.Cancel(); 
        _cancellationTokenSource = new CancellationTokenSource();

        StartTimer(_cancellationTokenSource.Token).Forget();
    }
    
    private async UniTask StartTimer(CancellationToken token)
    {
        try
        {
            await UniTask.Delay(3000, cancellationToken: token);

            if (!token.IsCancellationRequested) 
            {
                _fieldIsActive = true;
                _meshRenderer.enabled = true;
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Timer canceled");
        }
    }

    private void SpawnExplosion(GameObject explosionPrefab, Vector3 position)
    {
        var effect = Instantiate(explosionPrefab, position, Quaternion.identity);
        Destroy(effect, 2.0f); 
    }

    public void CancelToken()
    {
        _cancellationTokenSource?.Cancel(); 
    }
}
