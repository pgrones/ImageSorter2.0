﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using ImageSorter2._0.Annotations;

namespace ImageSorter2._0.ViewModel
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private string _alwaysOverride;

        public string AlwaysOverride
        {
            get => _alwaysOverride;
            set
            {
                _alwaysOverride = value;
                IOManager.AddUpdateAppSettings("AlwaysOverride", AlwaysOverride);
                OnPropertyChanged();
            }
        }

        private string _defaultPath;

        public string DefaultPath
        {
            get => _defaultPath;
            set
            {
                _defaultPath = value;
                IOManager.AddUpdateAppSettings("DefaultPath", DefaultPath);
                OnPropertyChanged();
            }
        }

        private string _saveFilePath;

        public string SaveFilePath
        {
            get => _saveFilePath;
            set
            {
                _saveFilePath = value;
                IOManager.AddUpdateAppSettings("SaveFilePath", SaveFilePath);
                OnPropertyChanged();
            }
        }
        
        private string _undo;

        public string Undo
        {
            get => _undo;
            set
            {
                _undo = value;
                IOManager.AddUpdateAppSettings("Undo", Undo);
                OnPropertyChanged();
            }
        }
        
        private string _delete;

        public string Delete
        {
            get => _delete;
            set
            {
                _delete = value;
                IOManager.AddUpdateAppSettings("Delete", Delete);
                OnPropertyChanged();
            }
        }
        
        private string _left;

        public string Left
        {
            get => _left;
            set
            {
                _left = value;
                IOManager.AddUpdateAppSettings("Left", Left);
                OnPropertyChanged();
            }
        }
        
        private string _right;

        public string Right
        {
            get => _right;
            set
            {
                _right = value;
                IOManager.AddUpdateAppSettings("Right", Right);
                OnPropertyChanged();
            }
        }

        private RelayCommand _chooseDefaultDirCommand;

        public RelayCommand ChooseDefaultDirCommand
        {
            get
            {
                return _chooseDefaultDirCommand ?? (
                    _chooseDefaultDirCommand = new RelayCommand(
                        (x) => { DefaultPath = IOUtils.ChooseDir(DefaultPath); },
                        (x) => true));
            }
        }

        private RelayCommand _chooseSaveDirCommand;

        public RelayCommand ChooseSaveDirCommand
        {
            get
            {
                return _chooseSaveDirCommand ?? (
                    _chooseSaveDirCommand = new RelayCommand(
                        (x) => { SaveFilePath = IOUtils.ChooseDir(SaveFilePath); },
                        (x) => true));
            }
        }

        private RelayCommand _deleteDefaultDirCommand;

        public RelayCommand DeleteDefaultDirCommand
        {
            get
            {
                return _deleteDefaultDirCommand ?? (
                    _deleteDefaultDirCommand = new RelayCommand(
                        (x) => { DefaultPath = ""; },
                        (x) => !string.IsNullOrWhiteSpace(DefaultPath)));
            }
        }

     

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SettingsViewModel()
        {
            AlwaysOverride = IOManager.ReadSetting("AlwaysOverride");
            DefaultPath = IOManager.ReadSetting("DefaultPath");
            SaveFilePath = IOManager.ReadSetting("SaveFilePath");
            Undo = IOManager.ReadSetting("Undo");
            Delete = IOManager.ReadSetting("Delete");
            Left = IOManager.ReadSetting("Left");
            Right = IOManager.ReadSetting("Right");
        }
    }
}