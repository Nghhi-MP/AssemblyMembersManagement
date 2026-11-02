using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;

namespace SessionMembersOrder
{
    public class SettingsFile
    {
        public static FileInfo DataFile = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Kanaf High Assembly", "settings.json"));
        public static string FileContents => File.ReadAllText(DataFile.FullName);
        
        [JsonPropertyName("comm_time")]
        public int[] CommencementTime = [10, 45];
    }
}
