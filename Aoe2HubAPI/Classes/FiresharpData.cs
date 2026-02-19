using AgeAPP;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using static AgeAPP.Classes.Main_classes;

namespace AgeAPP.Classes
{
    public class FiresharpData
    {
        // Conexão Firesharp
        public static string DataBasePath = "https://ageappv2-default-rtdb.firebaseio.com/";

        public IFirebaseConfig config = new FirebaseConfig
        {
            BasePath = DataBasePath,
            AuthSecret = ""
        };

        public IFirebaseClient client;

        // Account data
        public bool AccountLogged = false;
        public Account LocalAccount;

        public List<string> Admins_names = new List<string>();
        public List<string> All_accountsNames = new List<string>();

        public async Task<List<string>> Request_adminAccounts()
        {
            var response = await client.GetAsync("accounts");

            if (response.Body == "null")
            {
                Admins_names = new List<string>();
                return Admins_names;
            }

            var data = JsonConvert.DeserializeObject<Dictionary<string, Account>>(response.Body);

            Admins_names = data
                .Where(kvp => kvp.Value.IsAdmin)
                .Select(kvp => kvp.Key) // nome do usuário
                .ToList();

            return Admins_names;
        }

        public async Task<List<string>> Request_allAccountsNames()
        {
            var response = await client.GetAsync("accounts");

            if (response.Body == "null")
            {
                All_accountsNames = new List<string>();
                return All_accountsNames;
            }

            var data = JsonConvert.DeserializeObject<Dictionary<string, Account>>(response.Body);

            All_accountsNames = data.Keys.ToList();

            return All_accountsNames;
        }

        // CONNECTION

        public async Task<Account?> Try_login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            // 🔹 busca direta pela key
            var response = await client.GetAsync($"accounts/{username}");

            if (response.Body == "null")
                return null; // usuário não existe

            var account = JsonConvert.DeserializeObject<Account>(response.Body);

            if (account == null)
                return null;

            // 🔹 valida senha
            if (account.Password != password)
                return null;

            // 🔹 garante username correto
            account.Username = username;

            LocalAccount = account;

            return account;
        }

        public async Task Register_account_login_on_dataBase()
        {
            LocalAccount.Last_sessionDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            await client.SetAsync($"accounts/{LocalAccount.Username.ToLower()}", LocalAccount);
        }

        public void Connect_to_firesharp(string mode)
        {
            switch (mode.ToLower())
            {
                case "admin":
                    config.AuthSecret = "a1EylzvxpigRZYBKsDl9pLQcRJxiTxpde53z5S4I";
                    AccountLogged = true;
                    break;

                case "user":
                    AccountLogged = true;
                    break;
            }

            client = new FireSharp.FirebaseClient(config);
        }

        /*
        public async Task InitializeSettingsIfMissing()
        {
            var response = await client.GetAsync("settings");

            var defaultSettings = new AgeApp_settings.Settings
            {
                Kfactor = 26,
                Version = "5.4.4",
                maxInactiveDays = 10,
                MatchSize_multipliers = new Dictionary<string, float>
                {
                    { "1v1", 1.0f },
                    { "2v2", 2.0f },
                    { "3v3", 2.75f },
                    { "4v4", 3.5f },
                    { "FFA", 1.0f }
                }
            };

            await client.SetAsync("settings", defaultSettings);
        } // DEBUG PURPOSES ONLY

        public async Task<AgeApp_settings.Settings> Get_appSettings ()
        {
            FirebaseResponse response = await client.GetAsync("settings");

            if (response.Body == "null")
                return null;

            return response.ResultAs<AgeApp_settings.Settings>();
        }
        */

        public async Task<Updates> Get_updatesInfo()
        {
            FirebaseResponse response = await client.GetAsync("updates");

            if (response.Body == "null")
                return null;

            return response.ResultAs<Updates>();
        }

        public void Download_dataBase_Backup()
        {
            var response = client.GetAsync("").Result;

            string json = response.Body;

            // Formatar JSON
            string formattedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented);

            string backupFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Backups", $"database_backup_{DateTime.Now:dd-MM-yyyy}.json");

            // Salvar o JSON formatado no arquivo
            File.WriteAllText(backupFilePath, formattedJson);
        }
        
        public async Task Create_account(string username, string password, bool isAdmin)
        {
            var newAccount = new Account
            {
                Username = username,
                Password = password,
                IsAdmin = isAdmin,
                Last_sessionDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
            };

            await client.SetAsync($"accounts/{username.ToLower()}", newAccount);
        }

        // PLAYERS

        public async Task<List<Player>> GetAllPlayers()
        {
            FirebaseResponse response = await client.GetAsync("players");

            if (response.Body == "null")
                return new List<Player>();

            // Pega o conteudo da resposta e converte para dicionario
            var data = response.ResultAs<Dictionary<string, Player>>();

            var players_list = data.Values
                .OrderByDescending(p => p.Rating)
                .Select((p, index) =>
                {
                    p.Id = index + 1;

                    p.WinRate = p.Matches > 0 ? (float)Math.Round((float)p.Wins / p.Matches * 100, 2) : 0;

                    return p;
                })
                .ToList();

            return players_list;
        }

        public async Task<Player> Get_player(string player_name)
        {
            FirebaseResponse response = await client.GetAsync($"players/{player_name}");

            var player = response.ResultAs<Player>();
            return player;
        }

        public async Task Overwrite_player(Player player)
        {
            await client.SetAsync($"players/{player.Name}", player);
        }

        public async Task Overwrite_playerData(Player player, string field, object new_value)
        {
            var data = new Dictionary<string, object>
            {
                { field, new_value }
            };

            await client.UpdateAsync($"players/{player.Name}", data);
        }

        public async Task Add_new_player(Player player)
        {
            await client.SetAsync($"players/{player.Name.ToLower()}", player);
        }

        public async Task Delete_player(Player player)
        {
            await client.DeleteAsync($"players/{player.Name}");
        }

        // MAPS

        public async Task<List<Map>> GetAllMaps()
        {
            FirebaseResponse response = await client.GetAsync("maps");
            
            var data = response.ResultAs<Dictionary<string, Map>>();

            var maps_list = data.Values.ToList();

            return maps_list;
        }

        public async Task<Map> Get_map(string map_name)
        {
            FirebaseResponse response = await client.GetAsync($"maps/{map_name}");

            var map = response.ResultAs<Map>();
            return map;
        }

        public async Task Add_new_map(Map map)
        {
            await client.SetAsync($"maps/{map.Name.ToLower()}", map);
        }

        public async Task Overwrite_map(Map map)
        {
            await client.SetAsync($"maps/{map.Name}", map);
        }

        public async Task Delete_map(Map map)
        {
            await client.DeleteAsync($"maps/{map.Name}");
        }

        // LOGS

        public async Task Post_log_on_dataBase(Log log)
        {
            await client.PushAsync($"logs/{log.Author_name}/{log.Role}", log);
        }

        public async Task<List<Log>> GetMatchHistory(string admin)
        {
            var response = await client.GetAsync($"logs/{admin}/Match_results");

            if (response.Body == "null")
                return new List<Log>();

            var data = JsonConvert.DeserializeObject<Dictionary<string, Log>>(response.Body);

            return data.Values
                       .OrderByDescending(x => x.Match_result.MatchDate)
                       .ToList();
        }

        public async Task<List<Log>> GetGlobalMatchHistory(List<string> admins, int maxItems)
        {
            var allLogs = new List<Log>();

            foreach (var admin in admins)
            {
                var logs = await GetMatchHistory(admin);
                allLogs.AddRange(logs);
            }

            return allLogs
                .Where(l => l.Match_result != null)
                .OrderByDescending(l => l.Match_result.MatchDate)
                .Take(maxItems) // LIMITE AQUI
                .ToList();
        }

        public async Task<List<Log>> GetGlobalAdminLogs(List<string> admins, int maxItems)
        {
            var allLogs = new List<Log>();

            foreach (var admin in admins)
            {
                foreach (var role in new[] { "Player_changes", "Map_changes", "Match_results", "Split_changes" })
                {
                    var res = await client.GetAsync($"logs/{admin}/{role}");
                    if (res.Body == "null") continue;

                    var data = JsonConvert.DeserializeObject<Dictionary<string, Log>>(res.Body);
                    allLogs.AddRange(data.Values);
                }
            }

            return allLogs
                .OrderByDescending(l => DateTime.Parse(l.Date))
                .Take(maxItems)
                .ToList();
        }

        public async Task<List<RatingHistory>> BuildPlayerRatingHistory(string playerName)
        {
            var logs = await GetGlobalMatchHistory(Admins_names, 200);

            var history = new List<RatingHistory>();

            foreach (var log in logs.OrderBy(x => x.Match_result.MatchDate))
            {
                var match = log.Match_result;

                var player =
                    match.TeamA.FirstOrDefault(p => p.Name == playerName)
                    ??
                    match.TeamB.FirstOrDefault(p => p.Name == playerName);

                if (player != null)
                {
                    history.Add(new RatingHistory
                    {
                        Date = match.MatchDate,
                        Rating = player.Rating
                    });
                }
            }

            return history;
        }

        // CRASHES

        public async Task Post_crashLog_on_dataBase(CrashLog crashlog)
        {
            try
            {
                await client.PushAsync("crash_logs", crashlog);
            }
            catch
            {
                // Silencioso por segurança
            }
        }
    }
}
