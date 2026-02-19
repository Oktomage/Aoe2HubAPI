using System.ComponentModel;

namespace AgeAPP.Classes
{
    public class Main_classes
    {
        public static string Local_app_Version = "5.4.7";
        public static bool Show_only_active_players = true;
        public static bool Show_expanded_players_list = false;

        public class Account()
        {
            public string Username { get; set; } = "NaN";
            public string Password { get; set; } = "NaN";
            public string Last_sessionDate { get; set; }
            public bool IsAdmin { get; set; } = false;
        }

        public class Player
        {
            public int Id { get; set; } = 0;
            public string Name { get; set; } = "system_error";
            public int Rating { get; set; } = 0;
            public int Matches { get; set; } = 0;
            public int Wins { get; set; } = 0;
            public float WinRate { get; set; }

            [Browsable(false)]
            public string AvatarId { get; set; } = "Player_icon1";

            [Browsable(false)]
            public string Last_time_played { get; set; }

            [Browsable(false)]
            public Dictionary<string, FavoriteMap> Favorite_maps { get; set; } = new Dictionary<string, FavoriteMap>();

            public Player Clone()
            {
                return new Player
                {
                    Name = this.Name,
                    Rating = this.Rating,
                    Wins = this.Wins,
                    Matches = this.Matches,
                    Last_time_played = this.Last_time_played,
                    AvatarId = this.AvatarId,
                    Id = this.Id
                };
            }
        }

        public class FavoriteMap
        {
            public string Name { get; set; }
            public int Times_played { get; set; }
        }
        public class Map
        {
            public string Name { get; set; }
            public int Matches { get; set; }
            public int Type { get; set; }
        }
        public static string GetMapTypeName(int type)
        {
            return type switch
            {
                0 => "Padrão",
                1 => "QS",
                2 => "Nômade",
                3 => "Arena",
                4 => "Híbrido",
                5 => "Água",
                _ => "Desconhecido"
            };
        }

        public class Log
        {
            public string Author_name { get; set; }
            public string Date { get; set; }
            public List<Player> All_players { get; set; } = new List<Player>();
            public List<Player> TeamA_players { get; set; } = new List<Player>();
            public List<Player> TeamB_players { get; set; } = new List<Player>();
            public Map Played_map { get; set; }
            public string Role { get; set; } = "default";
            public string Content { get; set; }
            public MatchResult Match_result { get; set; }
        }

        public class MatchResult
        {
            public List<Player> TeamA { get; set; } = new List<Player>();
            public List<Player> TeamB { get; set; } = new List<Player>();
            public int MatchSize { get; set; } = 2; //2 = 2v2, 3 = 3v3, 4 = 4v4
            public int DeltaRating { get; set; }
            public int PerPlayerDelta { get; set; }
            public bool TeamAWon { get; set; }
            public string PlayedMap_name { get; set; }
            public DateTime MatchDate { get; set; }
        }

        public class RatingHistory
        {
            public DateTime Date { get; set; }
            public int Rating { get; set; }
        }

        public class Updates
        {
            public string ChangeLogs { get; set; }
        }

        public class CrashLog
        {
            public string Username { get; set; }
            public string Message { get; set; }
            public string StackTrace { get; set; }
            public string Version { get; set; }
            public string Date { get; set; }
        }
    }
}
