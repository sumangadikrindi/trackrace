public class LapKartStat{
    public int KartNumber { get; set; }
    public int LapNumber { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan? LapDuration{ get; set; }
}