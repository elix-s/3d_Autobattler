namespace Features.GameSessionService
{
    public class GameSessionService
    {
        public bool GameStarted { get; set; } = false;
        public int UserScore { get; set; } = 0;

        public void IncreaceScores(int value)
        {
            UserScore += value;
        }
    }
}

