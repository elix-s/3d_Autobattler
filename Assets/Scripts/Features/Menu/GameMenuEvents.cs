using Features.EventDispatcher;
public class IncreaseScoreEvent : EventsDispatcher.IGameEvent
{
    public int Score { get; set; }

    public void SetScore(int score)
    {
        this.Score = score;
    }
    
    public void Reset()
    {
        
    }
}
