using AgeAPP.Classes;
using static AgeAPP.Classes.Main_classes;

namespace Aoe2HubAPI
{
    public class Worker : BackgroundService
    {
        private FiresharpData data = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            data.Connect_to_firesharp("admin");

            while (!stoppingToken.IsCancellationRequested)
            {
                await TakeSnapshot();

                // espera 24h
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task TakeSnapshot()
        {
            var players = await data.GetAllPlayers();
            var maps = await data.GetAllMaps();

            var snapshot = new DailySnapshot
            {
                Date = DateTime.Now.ToString("dd-MM-yyyy HH:mm"),
                Players = players,
                Maps = maps
            };

            await data.client.SetAsync(
                $"daily_snapshots/{snapshot.Date}",
                snapshot);

            Console.WriteLine($"Snapshot salvo: {snapshot.Date}");
        }
    }

    public class DailySnapshot
    {
        public string Date { get; set; }

        public List<Player> Players { get; set; }
        public List<Map> Maps { get; set; }
    }
}
