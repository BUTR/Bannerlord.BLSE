using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Bannerlord.LauncherEx.Helpers
{
    internal static class ConfigReader
    {
        private static readonly string BOMMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
        
        public static readonly string GameConfigPath =
            Path.Combine($@"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}", "Mount and Blade II Bannerlord", "Configs", "BannerlordConfig.txt");
        public static readonly string EngineConfigPath =
            Path.Combine($@"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}", "Mount and Blade II Bannerlord", "Configs", "engine_config.txt");

        public static Dictionary<string, string> GetGameOptions(Func<string, byte[]?> readFileContent)
        {
            var dict = new Dictionary<string, string>();
            if (readFileContent(GameConfigPath) is not { } data) return dict;
            try
            {
                var content = Encoding.UTF8.GetString(data);
                if (content.StartsWith(BOMMarkUtf8, StringComparison.Ordinal))
                    content = content.Remove(0, BOMMarkUtf8.Length);

                foreach (var keyValue in content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var split = keyValue.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length != 2) continue;
                    var key = split[0].Trim();
                    var value = split[1].Trim();
                    dict[key] = value;
                }
            }
            catch (Exception) { /* ignore */ }
            return dict;
        }
        public static Dictionary<string, string> GetEngineOptions(Func<string, byte[]?> readFileContent)
        {
            var dict = new Dictionary<string, string>();
            if (readFileContent(EngineConfigPath) is not { } data) return dict;
            try
            {
                var content = Encoding.UTF8.GetString(data);
                if (content.StartsWith(BOMMarkUtf8, StringComparison.Ordinal))
                    content = content.Remove(0, BOMMarkUtf8.Length);

                foreach (var keyValue in content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var split = keyValue.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length != 2) continue;
                    var key = split[0].Trim();
                    var value = split[1].Trim();
                    dict[key] = value;
                }
            }
            catch (Exception) { /* ignore */ }
            return dict;
        }
    }
}