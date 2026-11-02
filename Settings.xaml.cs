using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace SessionMembersOrder
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();

            if (!SettingsFile.DataFile.Exists)
            {
                File.WriteAllText(SettingsFile.DataFile.FullName, JsonSerializer.Serialize<SettingsFile>(new()));
            }

            var json = JsonSerializer.Deserialize<SettingsFile>(File.ReadAllText(SettingsFile.DataFile.FullName));

            HoursBox.Text = json?.CommencementTime[0].ToString() ?? "--";
            MinutesBox.Text = json?.CommencementTime[1].ToString() ?? "--";
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var settings = JsonSerializer.Deserialize<SettingsFile>(File.ReadAllText(SettingsFile.DataFile.FullName));

            if (settings == null)
                return;

            if (int.TryParse(HoursBox.Text, out int hours))
            {
                settings.CommencementTime[0] = hours;
            }
            if (int.TryParse(MinutesBox.Text, out int minutes))
            {
                settings.CommencementTime[1] = minutes;
            }

            File.WriteAllText(SettingsFile.DataFile.FullName, JsonSerializer.Serialize(settings));

            Close();
        }
    }
}
