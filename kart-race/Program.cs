using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

Console.WriteLine("Input total number of laps for the race (Default 4): ");
var input = Console.ReadLine();
input = string.IsNullOrWhiteSpace(input) ? "4": input;
int totalLaps = Convert.ToInt32(input);

Console.WriteLine("Input maximum number of karts allowed in race (Default 5): ");
input = Console.ReadLine();
input = string.IsNullOrWhiteSpace(input) ? "5": input;
int maxDriverCount = Convert.ToInt32(input);

Console.WriteLine("Input kart-times csv file path (Default '../../../karttimes.csv'): ");
var kartTimesCSVFilePath = Console.ReadLine();
kartTimesCSVFilePath = string.IsNullOrWhiteSpace(kartTimesCSVFilePath) ? "../../../karttimes.csv": kartTimesCSVFilePath;

ServiceProvider provider = new ServiceCollection()
    .AddLogging(builder => builder.AddConsole())
    .AddSingleton<Race>(provider => {
        return new Race(
            totalLaps: totalLaps,
            maxDriverCount: maxDriverCount,
            logger: provider.GetRequiredService<ILogger<Race>>()
        );
    })
    .BuildServiceProvider();

//Configure race.
Race race = provider.GetRequiredService<Race>();

race.OnRaceFinished += (_, winner) =>{
    Console.WriteLine($"Winner identified as kart {winner.KartNumber} by finishing lap {winner.LapNumber} in duration { new DateTime(winner.LapDuration!.Value.Ticks).ToString("HH:mm:ss.fffffff")}, starting at {winner.StartTime.ToString("HH:mm:ss.fffffff")}");
};

//Start feeding kart times.
using var inputStreamReader = new StreamReader(kartTimesCSVFilePath);

//Not written any safety code in reading input file intentionally, assuming this is not the focus.
//skipping header.
await inputStreamReader.ReadLineAsync();

while(!inputStreamReader.EndOfStream){
    string? line = await inputStreamReader.ReadLineAsync();
    var kartTimeSplit = line?.Split(",");
    var kartTime = new KartTime{
            KartNumber=Convert.ToInt32(kartTimeSplit?[0]), 
            PassingTime = Convert.ToDateTime(kartTimeSplit?[1])
        };
    race.ProcessKartTime(kartTime);
}
Console.WriteLine("Completed feeding all kart times to Race object.");
Console.WriteLine("Press any key to exit.");
Console.Read();


