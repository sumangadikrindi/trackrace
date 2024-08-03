public class KartState
{
    public int KartNumber { get; private set; }
    public int CurrentLapNumber { get; set; }

    public KartState(int kartNumber, int currentLapNumber)
    {
        KartNumber = kartNumber;
        CurrentLapNumber = currentLapNumber;
    }
}