public class AppData : IVersionedData
{
    public int Version { get; set; } = 1;
    public int BestResult { get; set; }
}
