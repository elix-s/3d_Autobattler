using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using Features.GameSessionService;

public class ForceField : MonoBehaviour
{
    private const string EnemyLayerName = "Enemy";
    
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private GameObject _explosionEffect_FastEnemy;
    [SerializeField] private GameObject _explosionEffect_BigEnemy;
    [SerializeField] private GameCamera _camera;
    
    private CancellationTokenSource _cancellationTokenSource;
    private bool _fieldIsActive = true;
    
    private GameSessionService _gameSessionService;
    
    [Inject]
    private void Construct(GameSessionService gameSessionService)
    {
        _gameSessionService = gameSessionService;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (_fieldIsActive)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer(EnemyLayerName))
            {
                CollisionCheck(other);
            }
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (_fieldIsActive)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer(EnemyLayerName))
            {
                CollisionCheck(other);
            }
        }
    }

    private void CollisionCheck(Collider collider)
    {
        _fieldIsActive = false;
        _camera.ShakeCamera(0.5f, 0.1f).Forget();

        if (collider.gameObject.GetComponent<FastEnemy>() != null)
        {
            SpawnExplosion(_explosionEffect_FastEnemy,collider.ClosestPoint(transform.position));
        }
        else if (collider.gameObject.GetComponent<BigEnemy>() != null)
        {
            SpawnExplosion(_explosionEffect_BigEnemy,collider.ClosestPoint(transform.position));
        }
        
        _gameSessionService.IncreaseScores(10); 
        
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
