using Common.SavingSystem;
using Cysharp.Threading.Tasks;
using Features.EventDispatcher;

namespace Features.GameSessionService
{
    public class GameSessionService
    {
        public bool GameStarted { get; set; } = false;
        public int UserScore { get; set; } = 0;
        private SavingSystem _savingSystem;
        private EventsDispatcher _dispatcher;

        public GameSessionService(SavingSystem savingSystem, EventsDispatcher dispatcher)
        {
            _savingSystem = savingSystem;
            _dispatcher = dispatcher;
        }

        public void IncreaseScores(int value)
        {
            UserScore += value;
            
            var e = _dispatcher.GameDispatcher.Get<IncreaseScoreEvent>();
            e.SetScore(UserScore);
            _dispatcher.GameDispatcher.Invoke(e).Forget();
        }
        
        public async UniTask SaveUserScore()
        {
            var data = await _savingSystem.LoadDataAsync<AppData.AppData>();

            if (data.BestResult < UserScore)
            {
                data.BestResult = UserScore;
                _savingSystem.SaveDataAsync(data).Forget();
            }

            UserScore = 0;
        }
    }
}

