using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameStateContext : GameLifetimeScope
{
    [SerializeField] private EnemySpawner _enemySpawner;
    
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(_enemySpawner);
    }
}
