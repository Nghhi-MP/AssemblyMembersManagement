using SAQu;
using SessionMembersOrder;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

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

        public static Color AssemblyBlue => Color.FromRgb(0, 157, 255);

        public static TimeOnly CommencementTime => TimeOnly.FromDateTime(DateTime.Now);
        public static TimeOnly ConclusionTime => new(SettingsFile.Settings?.CommencementTime[0] ?? 11, SettingsFile.Settings?.CommencementTime[1] ?? 30);
        public int? SubjectsAmount = null;
        public static TimeSpan SessionLength => ConclusionTime - CommencementTime;
        public DispatcherTimer SubjectTimer = new()
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        public TimeSpan? SubjectTimer_Destination = null;
        public TimeSpan SubjectTimer_Passed = TimeSpan.Zero;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            SubjectTimer.Tick += SubjectTimer_Tick;

        }

        private void SubjectTimer_LblUpdate()
        {
            if (SubjectTimer_Passed <= TimeSpan.FromSeconds(11))
            {
                TimerLbl.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                TimerLbl.Foreground = new SolidColorBrush(AssemblyBlue);
            }
            TimerLbl.Text = $"{SubjectTimer_Passed.Minutes} : {SubjectTimer_Passed.Seconds}";
        }

        private void SubjectTimer_Tick(object? sender, EventArgs e)
        {
            SubjectTimer_Passed -= TimeSpan.FromSeconds(1);

            SubjectTimer_LblUpdate();

            if (SubjectTimer_Passed <= TimeSpan.Zero)
            {
                SubjectTimer.Stop();
                if (SubjectsAmount == 1)
                {
                    SubjectNameLbl.Text = "על הישיבה להסתכם.";
                }
            }
        }

        private void SubjectTimer_Reset()
        {
            SubjectsAmount = null;
            SubjectTimer_Passed = TimeSpan.Zero;
            SubjectTimer.Stop();
            SubjectTimerBtn.Content = "התחל נושא";
            TimerLbl.Text = "0 : 0";
            SubjectNameLbl.Text = "";
            ConcludeSubjectBtn.Visibility = Visibility.Hidden;
        }

        private void SubjectTimer_Start()
        {
            SubjectTimer_Destination = SessionLength / SubjectsAmount!.Value;

            SubjectTimer_Passed = SubjectTimer_Destination.Value;

            SubjectTimer_LblUpdate();

            SubjectTimer.Start();

            if (SubjectsAmount == 1)
            {
                SubjectNameLbl.Text = $"נותר נושא אחרון.";
                SubjectNameLbl.Foreground = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                SubjectNameLbl.Text = $"{SubjectsAmount} נושאים נותרו.";
                SubjectNameLbl.Foreground = new SolidColorBrush(Colors.White);
            }

            SubjectTimerBtn.Content = "איפוס";

            ConcludeSubjectBtn.Visibility = Visibility.Visible;
        }

        private void SubjectTimerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SessionLength < TimeSpan.Zero)
            {
                MessageBox.Show("The Session's length is out of it's currently set bounds in the code!", Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (SubjectsAmount == null)
            {
                var input = new InputBox()
                {
                    Title = Title
                };
                input.CaptionBlock.Text = "כתוב את כמות הנושאים:";

                input.EnterButton.Click += (sender, args) =>
                {
                    if (!int.TryParse(input.InputField.Text, out int subjectsAmount) || subjectsAmount < 0) return;
                    SubjectsAmount = subjectsAmount;
                    SubjectTimerBtn_Click(sender, args);
                };

                input.ShowDialog();

                return;
            }

            if (SubjectTimer_Passed > TimeSpan.Zero)
            {
                SubjectTimer_Reset();
                return;
            }

            SubjectTimer_Start();

        }

        private void ConcludeSubjectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SubjectsAmount > 1)
            {
                SubjectsAmount -= 1;
                SubjectTimer.Stop();
                SubjectTimer_Start();
            }
            else
            {
                SubjectTimer_Reset();
            }
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
        private void MoveBehindRaising(int oldIndex)
        {
            for (int i = 0; i < AssemblyMembers.Count; i++)
            {
                var member = AssemblyMembers[i];
                if (!member.IsRaising && !member.HasSpeakingAuthority)
                {
                    AssemblyMembers.Move(oldIndex, i);
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
                                        MoveBehindRaising(index);
                                    }
                                    assemblyMember.IsRaising = false;
                                }
                                else
                                {
                                    MoveBehindRaising(index);
                                    assemblyMember.IsRaising = true;
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

        private void MainWindow1_Closed(object sender, EventArgs e)
        {
            SubjectTimer.Stop();
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new Settings()
                .ShowDialog();
        }
    }
}