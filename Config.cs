using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace checker
{
    public static class Config
    {
        public static bool DEBUG_ENABLED = true;
        public static string SKIN_FILE_PATH = System.IO.Path.Combine("data", "skins.json");
        public static string SKIN_DATA_URL = "https://raw.communitydragon.org/latest/plugins/rcp-be-lol-game-data/global/default/v1/skins.json";
        public static string CHAMPION_FILE_PATH = System.IO.Path.Combine("data", "champions.json");
        public static string CHAMPION_DATA_URL = "https://raw.communitydragon.org/pbe/plugins/rcp-be-lol-game-data/global/default/v1/champion-summary.json";
        public static string LOOT_DATA_FILE_PATH = System.IO.Path.Combine("data", "loot.json");
        public static string LOOT_DATA_URL = "https://raw.communitydragon.org/latest/plugins/rcp-fe-lol-loot/global/default/trans.json";
        public static string LOOT_ITEMS_FILE_PATH = System.IO.Path.Combine("data", "lootItems.json");
        public static string LOOT_ITEMS_URL = "https://raw.communitydragon.org/latest/plugins/rcp-be-lol-game-data/global/default/v1/loot.json";

        
        public static string RAW_DATA_PATH = System.IO.Path.Combine("data", "raw");
        public static string EXPORT_PATH = "export";
        public static string TEMPLATE_PATH = "templates";
        public static string FAILED_ACCOUNT_PATH = System.IO.Path.Combine("export", "failedAccounts.txt");
        public static string UNFINISHED_ACCOUNT_PATH = "uncheckedAccounts.txt";

        // riot client
        public static int RIOT_CLIENT_LAUNCH_COOLDOWN = 15;
        public static int RIOT_CLIENT_LOADING_RETRY_COUNT = 3;
        public static int RIOT_CLIENT_LOADING_RETRY_COOLDOWN = 3;

        // league client
        public static int LEAGUE_CLIENT_LAUNCH_COOLDOWN = 15;

        // failure handling
        public static int LAUNCH_COOLDOWN_ON_INVALID_CREDENTIALS = 30;
        public static int RATE_LIMITED_COOLDOWN = 300;
        public static int MAX_RATE_LIMITED_ATTEMPTS = 5;
        public static int MAX_FAILED_ATTEMPTS = 5;

        // lol store (refunds)
        public static int LOL_STORE_REQUEST_COOLDOWN = 30;

        // login captcha
        public static string LOGIN_URL = "https://authenticate.riotgames.com/api/v1/login";

        // client watcher
        public static int CLIENT_WATCHER_CHECK_COOLDOWN = 1;

        // date format used for dates in export (banned until, last game time and so on)
        public static string DATE_FORMAT = "yyyy-MM-dd HH\\hmm\\mss\\s";
    }
}
