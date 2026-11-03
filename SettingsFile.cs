using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SessionMembersOrder
{
    public class SettingsFile
    {
        public static string DataFile => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Kanaf High Assembly", "settings.json");

        public static SettingsFile? Settings
        {
            get
            {
                if (!File.Exists(DataFile))
                    Settings = new();

                return JsonSerializer.Deserialize<SettingsFile>(File.ReadAllText(DataFile));
            }
            set
            {
                Directory.CreateDirectory(Path.GetDirectoryName(DataFile)!);
                File.WriteAllText(DataFile, JsonSerializer.Serialize(value));
            }
        }

        [JsonPropertyName("comm_time")]
        public int[] CommencementTime { get; set; } = [11, 30];
    }
}
