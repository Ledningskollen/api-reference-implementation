using System.Configuration;

namespace ApiClient.Helper {
    /// <summary>
    ///     Provide globla settings from config file
    /// </summary>
    public static class GlobalSettings {
        public static readonly string Version;
        public static readonly string Path;
        public static readonly string UserName_Co;
        public static readonly string Password_Co;
        public static readonly string UserName_Inq;
        public static readonly string Password_Inq;
        public static readonly string LemmyToken;
        public static readonly string LemmyVersion;

        static GlobalSettings() {
            Version = ReadString("Version");
            UserName_Co = ReadString("UserName_Co");
            Password_Co = ReadString("Password_Co");
            UserName_Inq = ReadString("UserName_Inq");
            Password_Inq = ReadString("Password_Inq");
            LemmyToken = ReadString("LemmyToken");
            LemmyVersion = ReadString("LemmyVersion");
            Path = ResolvePath();
        }

        private static string ResolvePath() {
            var path = ReadString("Host") + "/" + ReadString("ApiBase") + "/" + ReadString("Version") + "/";
            return path;
        }

        private static string ReadString(string key, string defaultValue = "") {
            return ConfigurationManager.AppSettings[key] ?? defaultValue;
        }

        private static int ReadInt(string key, string defaultValue = "0") {
            return int.Parse(ReadString(key, defaultValue));
        }

        private static bool ReadBool(string key, string defaultValue = "false") {
            return bool.Parse(ReadString(key, defaultValue));
        }
    }
}
