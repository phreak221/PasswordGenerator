using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Timers;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using System.IO;

namespace PasswordGenerator.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ICommand BtnGenerate { get; set; }
        public ICommand BtnSaveList { get; set; }
        public ICommand BtnClearList { get; set; }
        public ICommand MnuCopyToClipboard { get; set; }

        private string UpperLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string LowerLetters = "abcdefghijklmnopqrstuvwxyz";
        private string DigitsLetters = "1234567890";
        private string SpecialLetters = "!@#$%^&*-\\//?+=";
        private string BracketLetters = "[]{}()<>";
        private System.Timers.Timer tmClipboard;
        private System.Windows.Forms.Timer clipboardTimer;

        private bool _upperCaseLetters;
        public bool UpperCaseLetters
        {
            get => _upperCaseLetters;
            set
            {
                if (_upperCaseLetters == value) return;
                _upperCaseLetters = value;
                OnPropertyChanged("UpperCaseLetters");
            }
        }

        private int _minUpperCase;
        public int MinUpperCase
        {
            get => _minUpperCase;
            set
            {
                if (_minUpperCase == value) return;
                _minUpperCase = value;
                OnPropertyChanged("MinUpperCase");
            }
        }

        private bool _lowerCaseLetters;
        public bool LowerCaseLetters
        {
            get => _lowerCaseLetters;
            set
            {
                if (_lowerCaseLetters == value) return;
                _lowerCaseLetters = value;
                OnPropertyChanged("LowerCaseLetters");
            }
        }

        private bool _digits;
        public bool Digits
        {
            get => _digits;
            set
            {
                if (_digits == value) return;
                _digits = value;
                OnPropertyChanged("Digits");
            }
        }

        private int _minDigits;
        public int MinDigits
        {
            get => _minDigits;
            set
            {
                if (_minDigits == value) return;
                _minDigits = value;
                OnPropertyChanged("MinDigits");
            }
        }

        private bool _special;
        public bool Special
        {
            get => _special;
            set
            {
                if (_special == value) return;
                _special = value;
                OnPropertyChanged("Special");
                MinSpecial = (_special) ? 1 : 0;
            }
        }

        private int _minSpecial;
        public int MinSpecial
        {
            get => _minSpecial;
            set
            {
                if (_minSpecial == value) return;
                _minSpecial = value;
                OnPropertyChanged("MinSpecial");
            }
        }

        private bool _brackets;
        public bool Brackets
        {
            get => _brackets;
            set
            {
                if (_brackets == value) return;
                _brackets = value;
                OnPropertyChanged("Brackets");
            }
        }

        private int _passwordLength;
        public int PasswordLength
        {
            get => _passwordLength;
            set
            {
                if (_passwordLength == value) return;
                _passwordLength = value;
                OnPropertyChanged("PasswordLength");
            }
        }

        private int _numPasswords;
        public int NumPasswords
        {
            get => _numPasswords;
            set
            {
                if (_numPasswords == value) return;
                _numPasswords = value;
                OnPropertyChanged("NumPasswords");
            }
        }

        private string _selectedPassword;
        public string SelectedPassword
        {
            get => _selectedPassword;
            set
            {
                if (_selectedPassword == value) return;
                _selectedPassword = value;
                OnPropertyChanged("SelectedPassword");
            }
        }

        private string _eventMessage;
        public string EventMessage
        {
            get => _eventMessage;
            set
            {
                if (_eventMessage == value) return;
                _eventMessage = value;
                OnPropertyChanged("EventMessage");
            }
        }

        private double _progressValue;
        public double ProgressValue
        {
            get => _progressValue;
            set
            {
                if (_progressValue == value) return;
                _progressValue = value;
                OnPropertyChanged("ProgressValue");
            }
        }

        private string _isProgressVisible;
        public string IsProgressVisible
        {
            get => _isProgressVisible;
            set
            {
                if (_isProgressVisible == value) return;
                _isProgressVisible = value;
                OnPropertyChanged("IsProgressVisible");
            }
        }

        private ObservableCollection<string> _passwordList;
        public ObservableCollection<string> PasswordList
        {
            get
            {
                if (_passwordList == null)
                {
                    _passwordList = new ObservableCollection<string>();
                    _passwordList.CollectionChanged += OnPasswordListChanged;
                }
                return _passwordList;
            }
            set
            {
                if (_passwordList == value) return;
                _passwordList = value;
                OnPropertyChanged("PasswordList");
            }
        }

        private void OnPasswordListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            
        }

        private ObservableCollection<ViewModelBase> _workspaces;
        public MainViewModel(ObservableCollection<ViewModelBase> workspaces)
        {
            BtnGenerate = new RelayCommand(GenerateList);
            BtnSaveList = new RelayCommand(SaveGeneratedList);
            BtnClearList = new RelayCommand(ClearGeneratedList);
            MnuCopyToClipboard = new RelayCommand(CopyItemToClipboard);
            MinUpperCase = 0;
            MinDigits = 0;
            MinSpecial = 0;

            clipboardTimer = new System.Windows.Forms.Timer();
            clipboardTimer.Interval = 1000;
            clipboardTimer.Tick += ClipboardTimer_Tick;
            IsProgressVisible = "Hidden";

            _workspaces = workspaces;
        }

        private void ClipboardTimer_Tick(object sender, EventArgs e)
        {
            ProgressValue += 10;
        }

        private void ClearGeneratedList(object obj)
        {
            PasswordList.Clear();
            PasswordLength = 0;
            NumPasswords = 0;
            UpperCaseLetters = false;
            LowerCaseLetters = false;
            Digits = false;
            Special = false;
            Brackets = false;

            EventMessage = "Password list has been cleared";
        }

        private void SaveGeneratedList(object obj)
        {
            TextWriter tw = new StreamWriter("PasswordList.txt");
            foreach (string s in PasswordList)
            {
                tw.WriteLine(s);
            }
            tw.Close();
            EventMessage = "Passwords have been saved to a file";
        }

        private void CopyItemToClipboard(object obj)
        {
            Clipboard.SetText(SelectedPassword);
            SetTimer();
            clipboardTimer.Start();
            IsProgressVisible = "Visible";
            EventMessage = "Password has been saved to the clipboard";
        }

        private void SetTimer()
        {
            tmClipboard = new System.Timers.Timer(10*1000);
            tmClipboard.Elapsed += OnTimeEvent;
            tmClipboard.AutoReset = false;
            tmClipboard.Enabled = true;
        }

        private void OnTimeEvent(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                Clipboard.Clear();
                clipboardTimer.Stop();
                IsProgressVisible = "Hidden";
                ProgressValue = 0;
            }));
            tmClipboard.Enabled = false;
            EventMessage = "Clipboard has been cleared.";
        }

        private void GenerateList(object obj)
        {
            Clipboard.Clear();
            PasswordList.Clear();
            int tempPasswordLength = PasswordLength;
            string randomLetters = "";
            if (UpperCaseLetters)
                randomLetters += UpperLetters;
            if (LowerCaseLetters)
                randomLetters += LowerLetters;
            if (Digits)
                randomLetters += DigitsLetters;
            if (Special)
                randomLetters += SpecialLetters;
            if (Brackets)
                randomLetters += BracketLetters;

            for (int i = 1; i <= NumPasswords; i++)
            {
                StringBuilder sb = new StringBuilder();
                using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                {
                    byte[] uintBuffer = new byte[sizeof(uint)];
                    while (PasswordLength-- > 0)
                    {
                        rng.GetBytes(uintBuffer);
                        sb.Append(randomLetters[GetInt(rng, randomLetters.Length)]);
                        //uint num = BitConverter.ToUInt32(uintBuffer, 0);
                        //sb.Append(randomLetters[(int)(num % (uint)randomLetters.Length)]);
                    }
                }
                if (TestString(sb.ToString()))
                    PasswordList.Add(sb.ToString());
                else
                    i--;
                PasswordLength = tempPasswordLength;
            }
        }

        private bool TestString(string pass)
        {
            string pattern = @"[0-9]|[A-Z]|(!|@|#|$|%|\^|&|\*|-|\\|\/|\?|\+|=)";
            int upperCount = 0;
            int specialCount = 0;
            int numberCount = 0;

            foreach (Match m in Regex.Matches(pass, pattern))
            {
                if (!string.IsNullOrEmpty(m.Value))
                {
                    if (Char.IsUpper(m.Value.ToCharArray()[0]))
                        upperCount++;
                    if (Char.IsNumber(m.Value.ToCharArray()[0]))
                        numberCount++;
                    if (Char.IsPunctuation(m.Value.ToCharArray()[0]))
                        specialCount++;
                    if (Char.IsSymbol(m.Value.ToCharArray()[0]))
                        specialCount++;
                }
            }
            if (upperCount >= MinUpperCase && numberCount >= MinDigits && specialCount >= MinSpecial)
                return true;

            return false;
        }

        private int GetInt(RNGCryptoServiceProvider rnd, int max)
        {
            byte[] r = new byte[4];
            int value;
            do
            {
                rnd.GetBytes(r);
                value = BitConverter.ToInt32(r, 0) & Int32.MaxValue;
            } while (value >= max * (Int32.MaxValue / max));
            return value % max;
        }
    }
}
