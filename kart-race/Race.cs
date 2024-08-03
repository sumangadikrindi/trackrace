using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

public class Race{
    private readonly int _totalLaps;
    private readonly int _maxDriverCount;
    private readonly ILogger<Race>? _logger;

    private List<LapKartStat> _lapKartStats;
    private List<KartState> _kartStates;
    private bool _isRaceFinished;
    
    /// <summary>
    /// Winner of the race
    /// </summary>
    public LapKartStat? Winner { get; private set; }

    /// <summary>
    /// Race object to configure, and process kart times.
    /// </summary>
    /// <param name="totalLaps">Numbre of total laps</param>
    /// <param name="maxDriverCount">Maximum allowed driver count - Later entered karts's times ignored.</param>
    /// <param name="logger">Optional ILogger for capturing logs.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public Race(int totalLaps, int maxDriverCount, ILogger<Race>? logger=null)
    {
        if(totalLaps < 1) throw new ArgumentOutOfRangeException("Laps must be 1 or more..");

        _logger = logger;

        _isRaceFinished = false;
        _totalLaps = totalLaps;
        this._totalLaps = totalLaps;
        this._maxDriverCount = maxDriverCount;
        _lapKartStats = new List<LapKartStat>();
        _kartStates = new List<KartState>();
    }

    /// <summary>
    /// Event raised on race finished, with winner passed as event args.
    /// </summary>
    public event EventHandler<LapKartStat>? OnRaceFinished;

    /// <summary>
    /// Process kart time
    /// </summary>
    /// <param name="kartTime">kart time</param>
    public void ProcessKartTime(KartTime kartTime)
    {

        if (_isRaceFinished)
        {
            ProcessCartTimeReceivedAfterRaceFinished(kartTime);
            return;
        }

        int enteringLapNumber = 1;
        KartState? kartState = GetKartCurrentState(kartTime);

        if (kartState != null)
        { //Kart has already entered into race. 

            FinishLap(kartState.CurrentLapNumber, kartTime);

            //Check if the completed lap is the last lap to declare race completion
            if (kartState.CurrentLapNumber == _totalLaps)
            {
                FinishRace(kartTime);
                return;
            }

            //Note the entering lap nubmer
            enteringLapNumber = kartState.CurrentLapNumber + 1;
            //Update current lap number of existing kartstate
            kartState.CurrentLapNumber = enteringLapNumber;
        }
        else
        {
            kartState = TrackNewKartState(kartTime, enteringLapNumber);
        }

        //Add new lap kart stat with only start time - The entering lap
        if(kartState != null)
        {
            _lapKartStats.Add(new LapKartStat { LapNumber = enteringLapNumber, KartNumber = kartState.KartNumber, StartTime = kartTime.PassingTime });
            _logger?.LogInformation($"Kart {kartTime.KartNumber} entered lap {enteringLapNumber} at {kartTime.PassingTime}.");
        }
    }

    private void ProcessResult(){
        Winner = _lapKartStats
            .Where(lks => lks.LapDuration != null)
            .OrderByDescending(lks => lks.LapDuration)
            .First();
    }

    private void FinishRace(KartTime kartTime)
    {
        _isRaceFinished = true;
        _logger?.LogInformation($"Race finished by kart {kartTime.KartNumber} at {kartTime.PassingTime}.");
        ProcessResult();
        _logger?.LogInformation($"Race result processing finished.");
        if(Winner != null)
        {
            _logger?.LogInformation($"Winner identified as kart {Winner.KartNumber} by finishing lap {Winner.LapNumber} in duration { new DateTime(Winner.LapDuration!.Value.Ticks).ToString("HH:mm:ss.fffffff")}, starting at {Winner.StartTime.ToString("HH:mm:ss.fffffff")}");
            
            OnRaceFinished?.Invoke(this, Winner);
        }
        else
            throw new ApplicationException("Race processing failed. No winner identified.");
    }

    private void FinishLap(int lapNumber, KartTime kartTime)
    {
        //Get statistics of kart's current lap by current lap nubmer and kart number
        LapKartStat lapKartStat = _lapKartStats.First(lks => lks.LapNumber == lapNumber && lks.KartNumber == kartTime.KartNumber);

        //Update end time of completed lap's kart statistics.
        lapKartStat.EndTime = kartTime.PassingTime;
        lapKartStat.LapDuration = lapKartStat.EndTime - lapKartStat.StartTime;
        _logger?.LogInformation($"Kart {lapKartStat.KartNumber} finished lap {lapKartStat.LapNumber} in duration of {new DateTime(lapKartStat.LapDuration.Value.Ticks).ToString("HH:mm:ss.fffffff")}.");
    }

    private KartState? GetKartCurrentState(KartTime kartTime)
    {
        return _kartStates.FirstOrDefault(ks => ks.KartNumber == kartTime.KartNumber);
    }

    private KartState? TrackNewKartState(KartTime kartTime, int enteringLapNumber)
    {
        if(_kartStates.Count == _maxDriverCount)
        {
            _logger?.LogWarning($"Number of karts entered into race are above maximum limit.");
            return null;
        }
        //Kart entering into first lap. Add new KartState to KartStates to hold current lap number.
        KartState? kartState = new KartState(kartTime.KartNumber, enteringLapNumber);
        _kartStates.Add(kartState);
        return kartState;
    }

    private void ProcessCartTimeReceivedAfterRaceFinished(KartTime kartTime)
    {
        _logger?.LogInformation($"Race is already finished. Ignoring kart {kartTime.KartNumber} passing at {kartTime.PassingTime}.");
    }
}