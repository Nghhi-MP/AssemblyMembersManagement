using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WarningManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum WarningLevel
        {
            Normal = 0,
            Warned = 1,
            FinalWarning = 2,
            Ejected = 3
        }
        public class AssemblyMember : INotifyPropertyChanged
        {
            private string _name = string.Empty;
            private WarningLevel _warningLevel;

            public required string Name
            {
                get => _name;
                set
                {
                    if (_name != value)
                    {
                        _name = value;
                        OnPropertyChanged(nameof(Name));
                    }
                }
            }

            public WarningLevel WarningLevel
            {
                get => _warningLevel;
                set
                {
                    if (_warningLevel != value)
                    {
                        _warningLevel = value;
                        OnPropertyChanged(nameof(WarningLevel));
                        OnPropertyChanged(nameof(WarningNumber));
                        OnPropertyChanged(nameof(WarningLevelTitle));
                    }
                }
            }

            public string WarningLevelTitle
            {
                get
                {
                    switch (WarningLevel)
                    {
                        case WarningLevel.Normal: return "Normal";
                        case WarningLevel.Warned: return "Warned";
                        case WarningLevel.FinalWarning: return "Final Warning";
                        case WarningLevel.Ejected: return "EJECTED.";
                        default: return "???";
                    }
                }
            }

            public int WarningNumber
            {
                get
                {
                    return (int)WarningLevel;
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ObservableCollection<AssemblyMember> AssemblyMembers { get; set; } = [];

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)studentNameField.Template.FindName("PART_EditableTextBox", studentNameField);
            if (textBox != null)
            {
                textBox.TextChanged += ComboBoxTextChanged;
            }
        }

        private void SelectItemInBox()
        {
            string studentName = studentNameField.Text;

            if (AssemblyMembers.FirstOrDefault(member => member.Name == studentName) is { } member)
            {
                int i = warningInformationBox.Items.IndexOf(member);
                warningInformationBox.SelectedIndex = i;
                warningInformationBox.ScrollIntoView(member);
            }
        }

        private void ComboBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            SelectItemInBox();
        }

        private void ComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            string studentName = studentNameField.Text;

            switch (e.Key)
            {
                case Key.Enter:
                    {
                        if (string.IsNullOrWhiteSpace(studentName)) return;

                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            if (AssemblyMembers.FirstOrDefault(member => member.Name == studentName) is AssemblyMember assemblyMember)
                            {
                                if ((int)assemblyMember.WarningLevel < 1)
                                {
                                    AssemblyMembers.Remove(assemblyMember);
                                }
                                else assemblyMember.WarningLevel--;
                            }
                        }
                        else
                        {

                            if (AssemblyMembers.FirstOrDefault(member => member.Name == studentName) is AssemblyMember member)
                            {
                                if ((int)member.WarningLevel >= 3)
                                {

                                }
                                else member.WarningLevel++;
                            }
                            else
                            {
                                AssemblyMembers.Add(new() { Name = studentName, WarningLevel = WarningLevel.Warned });
                            }
                        }
                    }
                    SelectItemInBox();
                    break;
            }

        }
    }
}