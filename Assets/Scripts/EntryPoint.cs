using Common.GameStateService;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

public class EntryPoint : IStartable
{
   private readonly GameStateService _stateMachine;
   
   public EntryPoint(GameStateService stateMachine)
   {
      _stateMachine = stateMachine;
   }
   
   public void Start()
   {
      Debug.Log("Start");
      _stateMachine.ChangeState<StartLoadingState>().Forget();
   }
}
