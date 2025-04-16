using UnityEngine;

public abstract class EnemyFactory<T> where T : MonoBehaviour, IEnemy
{
    private GameObject _enemyPrefab;
    
    protected EnemyFactory(GameObject prefab)
    {
        _enemyPrefab = prefab;
    }

    public T CreateEnemy(Transform player, Vector3 position, Transform parent)
    {
        var enemyAI = GameObject.Instantiate(_enemyPrefab, position, Quaternion.identity)
            .GetComponent<EnemyAI>();
        enemyAI.transform.SetParent(parent);
        enemyAI.SetTarget(player);
        return enemyAI.GetComponent<T>();
    }
}

