using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using DublicateFileFinder.Properties;
using Brush = System.Windows.Media.Brush;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace DublicateFileFinder
{
    public class MainWindowWiewModel : INotifyPropertyChanged
    {
        #region Properties

        public double ListWith
        {
            get => _listWith;
            set
            {
                if (value.Equals(_listWith))
                {
                    return;
                }
                _listWith = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(listitem1witdht));
                OnPropertyChanged(nameof(listitem2with));
            }
        }

        private CancellationTokenSource _cancellationTokenSorceGetDublicates;

        public double listitem1witdht => ListWith * 90 / 100;

        public double listitem2with => ListWith * 10 / 100;

        public bool ShowLoadingIcon
        {
            get => _showLoadingIcon;
            set
            {
                if (value == _showLoadingIcon)
                {
                    return;
                }

                _showLoadingIcon = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Extentions { get; set; }

        public ObservableCollection<ObservableCollection<string>> Dublicates { get; set; }


        public ICommand AddExtentionCommand { get; set; }
        public ICommand GetDublicateCommand { get; set; }
        public ICommand RemoveExtentionCommand { get; set; }
        public ICommand ClearOnFocusCommand { get; set; }
        public ICommand DeleteFileCommeand { get; set; }
        public ICommand SaveExtentionListCommand { get; set; }
        public ICommand LoadExtentionListCommand { get; set; }
        public ICommand CancelGetDublicates { get; set; }
        public ICommand OpenFileInNotepadCommand { get; set; }
        public ICommand OpenFileLocationCommand { get; set; }
        public ICommand OpenFileCommand { get; set; }
        public ICommand CopyFilePathCommand { get; set; }

        private Brush _extentionOpacity;

        public Brush ExtentionOpacity
        {
            get =>
            _extentionOpacity;
            set
            {
                if (value == _extentionOpacity)
                {
                    return;
                }

                _extentionOpacity = value;
                OnPropertyChanged();
            }
        }

        private string _extention;

        public string Extention
        {
            get =>
            _extention;
            set
            {
                if (value == _extention)
                {
                    return;
                }

                _extention = value;
                OnPropertyChanged();
            }
        }

        #endregion

        private readonly string _extentionPlaceholder = "Extention to skip";
        private readonly double _listitem1Witdht;
        private readonly double _listitem2With;
        private double _listWith;
        private bool _showLoadingIcon;

        public MainWindowWiewModel()
        {
            ShowLoadingIcon = false;
            Extentions = new ObservableCollection<string>();
            Dublicates = new ObservableCollection<ObservableCollection<string>>();
            Extention = _extentionPlaceholder;
            _cancellationTokenSorceGetDublicates = new CancellationTokenSource();
            ExtentionOpacity = new SolidColorBrush(Colors.Black)
            {
                Opacity = 0.5
            };
            OpenFileCommand = new RelayCommand(OpenFile);
            OpenFileInNotepadCommand = new RelayCommand(OpenFileInNotepad);
            OpenFileLocationCommand = new RelayCommand(OpenFileLocation);
            CopyFilePathCommand = new RelayCommand(CopyPathToClipboard);
            GetDublicateCommand = new RelayCommandAsync(FindDublicates);
            CancelGetDublicates = new RelayCommand(CancelGetDublicatesmethod, AllowCancelGetDublicatesmethod);
            DeleteFileCommeand = new RelayCommand(DeleteFile);
            SaveExtentionListCommand = new RelayCommand(SaveExtentionList, AllowSaveExtentionList);
            LoadExtentionListCommand = new RelayCommand(LoadExtentionList);
            RemoveExtentionCommand = new RelayCommand(RemoveExtention, AllowSaveExtentionList);
            AddExtentionCommand = new RelayCommand(AddExtention, AllowAddExtention);
            ClearOnFocusCommand = new RelayCommand(ClearOnFocus);
        }

        public async Task FindDublicates(object obj)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            if (!Directory.Exists(dialog.SelectedPath))
            {
                MessageBox.Show("The Directory does not exist");
                return;
            }
            Dublicates.Clear();
            ShowLoadingIcon = true;
            _cancellationTokenSorceGetDublicates = new CancellationTokenSource();
            System.Tuple<System.Collections.Generic.List<string>, System.Collections.Generic.List<string>> skipped = await CheckForDublicates.FileTjeck(dialog.SelectedPath, Extentions.ToList(), Dublicates, _cancellationTokenSorceGetDublicates.Token);
            if (skipped.Item2.Count > 0)
            {
                StringBuilder stringbuilder = new StringBuilder();
                foreach (string dir in skipped.Item2)
                {
                    stringbuilder.AppendLine(dir);
                }
                MessageBoxResult msr =
                   MessageBox.Show(
                       $"Some directories was not searched because of authorization issues\n{stringbuilder.ToString()}",
                       "Info", MessageBoxButton.OK);
            }

            if (skipped.Item1.Count > 0)
            {
                StringBuilder stringbuilder = new StringBuilder();
                foreach (string dir in skipped.Item1)
                {
                    stringbuilder.AppendLine(dir);
                }
                MessageBoxResult msr =
                   MessageBox.Show(
                       $"Some files was not checked because of authorization issues\n{stringbuilder.ToString()}",
                       "Info", MessageBoxButton.OK);
            }

            ShowLoadingIcon = false;
        }

        public bool AllowSaveExtentionList(object onj)
        {
            return Extentions.Count > 0;
        }

        public void SaveExtentionList(object obj)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                OverwritePrompt = true,
                Filter = "File extention text| *.fet",
                DefaultExt = ".fet"
            };
            dialog.ShowDialog();
            string path = dialog.FileName;
            path = Path.ChangeExtension(path, "fet");
            File.WriteAllLines(path, Extentions);
        }



        public bool AllowCancelGetDublicatesmethod(object obj)
        {
            return Dublicates.Count > 0;
        }

        public void CancelGetDublicatesmethod(object obj)
        {
            _cancellationTokenSorceGetDublicates.Cancel();
        }

        public void DeleteFile(object obj)
        {
            MessageBoxResult msr =
                    MessageBox.Show(
                        $"Are you sure you want to delete this file: {obj.ToString()}",
                        "Confirmation", MessageBoxButton.YesNo);
            if (msr == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                File.Delete((string)obj);
                foreach (ObservableCollection<string> dublicate in Dublicates)
                {
                    if (dublicate.Contains(obj.ToString()))
                    {
                        dublicate.Remove(obj.ToString());
                    }
                }
            }
            catch (IOException)
            {
                MessageBox.Show("An Error occurred wile trying to delete the file, The File is probably open in another process");
            }

        }

        public void OpenFile(object obj)
        {
            Process.Start(obj.ToString());
        }

        public void OpenFileInNotepad(object obj)
        {
            Process.Start("notepad.exe", obj.ToString());
        }

        public void CopyPathToClipboard(object obj)
        {
            System.Windows.Clipboard.SetText(obj.ToString());
        }
        public void LoadExtentionList(object obj)
        {
            if (Extentions.Count > 0)
            {
                MessageBoxResult msr =
                    MessageBox.Show(
                        "This operation will remove the defined list of extentions\nAre you sure you want to continue",
                        "Confirmation", MessageBoxButton.YesNo);
                if (msr == MessageBoxResult.No)
                {
                    return;
                }
            }
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "File extention text| *.fet",
                Multiselect = false
            };
            dialog.ShowDialog();
            if (!File.Exists(dialog.FileName))
            {
                MessageBox.Show("The file does not exist");
                return;
            }
            if (Path.GetExtension(dialog.FileName) != ".fet")
            {
                MessageBox.Show("The file is invalid");
                return;
            }

            string[] lines = File.ReadAllLines(dialog.FileName);
            Extentions.Clear();
            foreach (string line in lines)
            {
                Extentions.Add(line);
            }
        }

        public void OpenFileLocation(object obj)
        {
            Process.Start("explorer.exe", "/select, " + obj.ToString());
        }

        public void ClearOnFocus(object obj)
        {
            if (Extention.Equals(_extentionPlaceholder))
            {
                Extention = "";
                ExtentionOpacity.Opacity = 1;
            }
        }

        private bool AllowAddExtention(object obj)
        {
            return !Extentions.Contains($".{Extention.TrimStart('.').Trim().TrimStart().ToLower()}") &&
                   !string.IsNullOrWhiteSpace(Extention) && !_extentionPlaceholder.ToLower().Equals(Extention.ToLower());
        }

        private void AddExtention(object obj)
        {
            Extentions.Add($".{Extention.TrimStart('.').Trim().TrimStart().ToLower()}");
            Extention = "";
        }

        private void RemoveExtention(object obj)
        {
            Extentions.Remove((string)obj);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}