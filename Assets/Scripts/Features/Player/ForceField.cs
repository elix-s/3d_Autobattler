using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ForceField : MonoBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private GameObject _explosionEffect_FastEnemy;
    [SerializeField] private GameObject _explosionEffect_BigEnemy;
    
    private CancellationTokenSource _cancellationTokenSource;
    private bool _fieldIsActive = true;
    
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

    private async void CollisionCheck(Collider collider)
    {
        _fieldIsActive = false;
                
        if(collider.gameObject.GetComponent<FastEnemy>() != null) 
            SpawnExplosion(_explosionEffect_FastEnemy,collider.ClosestPoint(transform.position));
        else if(collider.gameObject.GetComponent<BigEnemy>() != null)
            SpawnExplosion(_explosionEffect_BigEnemy,collider.ClosestPoint(transform.position));
                
        Destroy(collider.gameObject);
        _meshRenderer.enabled = false;

        _cancellationTokenSource?.Cancel(); 
        _cancellationTokenSource = new CancellationTokenSource();

        await StartTimer(_cancellationTokenSource.Token);
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
            Debug.Log("Таймер сброшен");
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
