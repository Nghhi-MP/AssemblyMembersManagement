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
using static WarningManager.MainWindow;

namespace WarningManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum State
        {
            Normal = 0,
            Warned = 1,
            InLastWarning = 2,
            Ejected = 3,
            Raising = 4,
            HasSpeakingAuthority = 5
        }
        public class AssemblyMember : INotifyPropertyChanged
        {
            private string _name = string.Empty;
            private int _warningLevel = 0;

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

            public int WarningLevel
            {
                get => _warningLevel;
                set
                {
                    if (_warningLevel != value)
                    {
                        _warningLevel = value;
                        OnPropertyChanged(nameof(WarningLevel));
                        OnPropertyChanged(nameof(State));
                        OnPropertyChanged(nameof(StateTitle));
                    }
                }
            }

            //public string WarningLevelTitle
            //{
            //    get
            //    {
            //        return WarningLevel switch
            //        {
            //            0 => "רגיל",
            //            1 => "מוזהר/ת",
            //            2 => "באזהרה אחרונה",
            //            3 => "בחוץ.",
            //            _ => "???"
            //        };
            //    }
            //}

            public bool _IsRaising;
            public bool IsRaising
            {
                get { return _IsRaising; }
                set
                {
                    _IsRaising = value;
                    OnPropertyChanged(nameof(IsRaising));
                    OnPropertyChanged(nameof(State));
                    OnPropertyChanged(nameof(StateTitle));
                }
            }
            public bool _HasSpeakingAuthority;
            public bool HasSpeakingAuthority
            {
                get { return _HasSpeakingAuthority; }
                set
                {
                    _HasSpeakingAuthority = value;
                    OnPropertyChanged(nameof(HasSpeakingAuthority));
                    OnPropertyChanged(nameof(State));
                    OnPropertyChanged(nameof(StateTitle));
                }
            }

            public State State
            {
                get
                {
                    if (HasSpeakingAuthority) return State.HasSpeakingAuthority;
                    else if (IsRaising) return State.Raising;
                    else return WarningLevel switch
                    {
                        1 => State.Warned,
                        2 => State.InLastWarning,
                        3 => State.Ejected,
                        _ => State.Normal
                    };
                }
            }

            public string StateTitle
            {
                get
                {
                    return State switch
                    {
                        State.Raising => "מצביע/ה",
                        State.Normal => "רגיל",
                        State.Warned => "מוזהר/ת",
                        State.InLastWarning => "באזהרה אחרונה",
                        State.Ejected => "מורחק/ת.",
                        State.HasSpeakingAuthority => "מחזיק/ה ברשות דיבור",
                        _ => "???"
                    };
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
        private void MoveBehindRaising(int oldIndex, int padding = 0)
        {
            for (int i = 0; i < AssemblyMembers.Count; i++)
            {
                var member = AssemblyMembers[i];
                if (!member.IsRaising && !member.HasSpeakingAuthority)
                {
                    AssemblyMembers.Move(oldIndex, i + padding);
                    break;
                }
            }
        }
        private void ComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            string studentName = studentNameField.Text;

            switch (e.Key)
            {
                case Key.F3:
                    {
                        if (AssemblyMembers.FirstOrDefault(member => member.Name == studentName) is AssemblyMember assemblyMember)
                        {
                            int index = AssemblyMembers.IndexOf(assemblyMember);
                            if (index != 0)
                            {
                                AssemblyMembers.Move(index, index - 1);
                            }
                        }
                    }
                    break;
                case Key.F2:
                    {
                        if (AssemblyMembers.FirstOrDefault(member => member.Name == studentName) is AssemblyMember assemblyMember)
                        {
                            int index = AssemblyMembers.IndexOf(assemblyMember);
                            if (index < AssemblyMembers.Count - 1)
                            {
                                AssemblyMembers.Move(index, index + 1);
                            }
                        }
                    }
                    break;
                case Key.Enter:
                    {
                        if (string.IsNullOrWhiteSpace(studentName)) return;

                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            if (AssemblyMembers.FirstOrDefault(member => member.Name == studentName) is AssemblyMember assemblyMember)
                            {
                                if (assemblyMember.WarningLevel < 1)
                                {
                                }
                                else assemblyMember.WarningLevel--;
                            }
                        }
                        else if (Keyboard.IsKeyDown(Key.RightShift))
                        {
                            if (AssemblyMembers.FirstOrDefault(member => member.Name == studentName) is AssemblyMember assemblyMember)
                            {
                                int index = AssemblyMembers.IndexOf(assemblyMember);
                                if (assemblyMember.IsRaising)
                                {
                                    if (!AssemblyMembers.Any(member => !member.IsRaising))
                                    {
                                        AssemblyMembers.Move(index, AssemblyMembers.Count - 1);
                                    }
                                    else
                                    {
                                        MoveBehindRaising(index, -1);
                                    }
                                    assemblyMember.IsRaising = false;
                                }
                                else
                                {
                                    assemblyMember.IsRaising = true;
                                    MoveBehindRaising(index);
                                }
                            }
                        }
                        else
                        {

                            if (AssemblyMembers.FirstOrDefault(member => member.Name == studentName) is AssemblyMember member)
                            {
                                if (member.WarningLevel >= 3)
                                {

                                }
                                else member.WarningLevel++;
                            }
                            else
                            {
                                AssemblyMembers.Add(new() { Name = studentName });
                                studentNameField.SelectedIndex = AssemblyMembers.Count - 1;
                            }
                        }
                        SelectItemInBox();
                    }
                    break;
                case Key.F9:
                    {
                        if (AssemblyMembers.FirstOrDefault(member => member.Name == studentName) is AssemblyMember member)
                        {
                            MoveBehindRaising(AssemblyMembers.IndexOf(member));
                            member.HasSpeakingAuthority = !member.HasSpeakingAuthority;
                        }
                    }
                    break;
                case Key.F1:
                    {
                        if (AssemblyMembers.FirstOrDefault(member => member.Name == studentName) is AssemblyMember member)
                        {
                            AssemblyMembers.Remove(member);
                        }
                    }
                    break;
            }

        }
    }
}