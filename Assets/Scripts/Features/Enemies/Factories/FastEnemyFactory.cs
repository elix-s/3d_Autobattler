using UnityEngine;

public class FastEnemyFactory : EnemyFactory<FastEnemy>
{
    public FastEnemyFactory(GameObject prefab) : base(prefab) { }
}
