﻿using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using ImageSorter2._0.Annotations;
using ImageSorter2._0.Model;

namespace ImageSorter2._0.ViewModel
{
    public class AddDirViewModel : INotifyPropertyChanged
    {
        private string _dirPath;

        public string DirPath
        {
            get => _dirPath;
            set
            {
                _dirPath = value;
                OnPropertyChanged();
            }
        }

        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _shortcut;

        public string Shortcut
        {
            get => _shortcut;
            set
            {
                _shortcut = value;
                OnPropertyChanged();
            }
        }

        private RelayCommand _chooseDirCommand;

        public RelayCommand ChooseDirCommand
        {
            get
            {
                return _chooseDirCommand ?? (
                    _chooseDirCommand = new RelayCommand(
                        (x) =>
                        {
                            DirPath = IOUtils.ChooseDir(DirPath);
                            if (string.IsNullOrEmpty(Name))
                            {
                                Name = DirPath.Split('\\').Last();
                            }
                        },
                        (x) => true));
            }
        }

        private RelayCommand _addDirCommand;

        public RelayCommand AddDirCommand
        {
            get
            {
                return _addDirCommand ?? (
                    _addDirCommand = new RelayCommand(
                        (x) =>
                        {
                            var mainViewModel = (MainViewModel) ((Window) x).Owner.DataContext;
                            mainViewModel.Directories.Add(new DirectoryModel
                            {
                                Name = Name, 
                                Path = DirPath,
                                Shortcut = "📂" + (string.IsNullOrEmpty(Shortcut) ? "" : " " + Shortcut),
                                Index = mainViewModel.Directories.Count
                            });
                            ((Window) x).Close();
                        },
                        (x) => !string.IsNullOrEmpty(_name) && !string.IsNullOrEmpty(_dirPath)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}