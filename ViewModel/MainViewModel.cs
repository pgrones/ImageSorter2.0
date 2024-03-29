﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using ImageSorter2._0.Annotations;
using ImageSorter2._0.Model;
using Microsoft.VisualBasic.FileIO;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ImageSorter2._0.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        private readonly List<HistoryObject> _history;
        private readonly MainLogic _logic;

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

        private bool _alwaysOverride;

        public bool AlwaysOverride
        {
            get => _alwaysOverride;
            set
            {
                _alwaysOverride = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage _imageSource;

        public BitmapImage ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged();
            }
        }

        private string _gifSource;

        public string GifSource
        {
            get => _gifSource;
            set
            {
                _gifSource = value;
                OnPropertyChanged();
            }
        }

        private string _page;

        public string Page
        {
            get => _page;
            set
            {
                _page = value;
                OnPropertyChanged();
            }
        }

        private string _imageName;

        public string ImageName
        {
            get => _imageName;
            set
            {
                _imageName = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DirectoryModel> Directories { get; }

        private RelayCommand _nextCommand;

        public RelayCommand NextCommand
        {
            get
            {
                return _nextCommand ?? (
                    _nextCommand = new RelayCommand(
                        (x) =>
                        {
                            _logic.CurrentImage++;
                            if (_logic.CurrentImage == _logic.Images.Count)
                            {
                                _logic.CurrentImage = 0;
                            }

                            SetMetaInfos();
                            SetImage();
                        },
                        (x) => _logic.Images.Count > 1));
            }
        }

        private RelayCommand _prevCommand;

        public RelayCommand PrevCommand
        {
            get
            {
                return _prevCommand ?? (
                    _prevCommand = new RelayCommand(
                        (x) =>
                        {
                            _logic.CurrentImage--;
                            if (_logic.CurrentImage == -1)
                            {
                                _logic.CurrentImage = _logic.Images.Count - 1;
                            }

                            SetMetaInfos();
                            SetImage();
                        },
                        (x) => _logic.Images.Count > 1));
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
                            var dialog = new CommonOpenFileDialog
                            {
                                InitialDirectory = _logic.Path, IsFolderPicker = true
                            };
                            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                            {
                                _logic.Path = dialog.FileName;
                                _logic.LoadImages();
                                _logic.CurrentImage = 0;
                                DirPath = _logic.Path;
                                SetMetaInfos();
                                if (_logic.HasImages())
                                {
                                    SetImage();
                                }
                                else
                                {
                                    ImageSource = null;
                                }
                            }
                        },
                        (x) => true));
            }
        }

        private RelayCommand _deleteFileCommand;

        public RelayCommand DeleteFileCommand
        {
            get
            {
                return _deleteFileCommand ?? (
                    _deleteFileCommand = new RelayCommand(
                        (x) =>
                        {
                            var originalGifSource = GifSource;
                            try
                            {
                                GifSource = null;
                                
                                FileSystem.DeleteFile(_logic.GetCurrentImage(),
                                    UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                                _logic.Images.RemoveAt(_logic.CurrentImage);
                                if (_logic.CurrentImage == _logic.Images.Count)
                                {
                                    _logic.CurrentImage--;
                                }

                                SetImage();
                                SetMetaInfos();
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show("Something went wrong while deleting the file.\n" + e.Message,
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                GifSource = originalGifSource;
                            }
                        },
                        (x) => _logic.HasImages()));
            }
        }

        private RelayCommand _undoCommand;

        public RelayCommand UndoCommand
        {
            get
            {
                return _undoCommand ?? (
                    _undoCommand = new RelayCommand(
                        (x) =>
                        {
                            var info = _history.Last();
                            try
                            {
                                File.Move(info.NewPath, info.OldPath);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(
                                    "Something went wrong while undoing the last command.\n" + e.Message,
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }

                            _logic.Images.Insert(info.Index, info.OldPath);
                            _logic.CurrentImage = info.Index;
                            SetImage();
                            SetMetaInfos();

                            if (string.IsNullOrEmpty(DirPath))
                            {
                                DirPath = info.DirPath;
                            }

                            _history.RemoveAt(_history.Count - 1);
                        },
                        (x) => _history.Count > 0));
            }
        }

        private RelayCommand _moveCommand;

        public RelayCommand MoveCommand
        {
            get
            {
                return _moveCommand ?? (
                    _moveCommand = new RelayCommand(
                        (x) =>
                        {
                            var dir = Directories[(int) x];
                            var source = _logic.GetCurrentImage();
                            var oldName = source.Split('\\').Last();
                            var dest = dir.Path;
                            if (string.IsNullOrWhiteSpace(ImageName))
                            {
                                dest += $"\\{oldName}";
                            }
                            else
                            {
                                dest += $"\\{ImageName}.{oldName.Split('.').Last()}";
                            }

                            var originalGifSource = GifSource;
                            try
                            {
                                if (oldName.Split('.').Last() == "gif")
                                {
                                    GifSource = null;
                                }

                                if (File.Exists(dest) && AlwaysOverride)
                                {
                                    File.Copy(source, dest, true);
                                    File.Delete(source);
                                }
                                else
                                {
                                    File.Move(source, dest);
                                }
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show("Something went wrong while moving the file.\n" + e.Message,
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                GifSource = originalGifSource;
                                return;
                            }

                            _history.Add(new HistoryObject
                            {
                                OldPath = source,
                                NewPath = dest,
                                Index = _logic.CurrentImage,
                                DirPath = DirPath
                            });

                            _logic.RemoveCurrentImage();
                            SetImage();
                            SetMetaInfos();
                        },
                        (x) => _logic.HasImages()));
            }
        }

        private RelayCommand _deleteDirCommand;

        public RelayCommand DeleteDirCommand
        {
            get
            {
                return _deleteDirCommand ?? (
                    _deleteDirCommand = new RelayCommand(
                        (x) =>
                        {
                            Directories.RemoveAt((int) x);
                            IOUtils.Save(Directories.ToList());
                        },
                        (x) => true));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainViewModel(MainLogic logic)
        {
            _history = new List<HistoryObject>();
            _logic = logic;
            Directories = new ObservableCollection<DirectoryModel>(IOUtils.Load());
            _logic.Path = IOManager.ReadSetting("DefaultPath");
            AlwaysOverride = IOManager.ReadSetting("AlwaysOverride") == "True";
            DirPath = _logic.Path;
            logic.LoadImages();
            SetMetaInfos();
            SetImage();
        }

        private void SetImage()
        {
            if (_logic.HasImages())
            {
                var currImage = _logic.GetCurrentImage();
                if (currImage.EndsWith("gif"))
                {
                    ImageSource = null;
                    GifSource = currImage;
                }
                else
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = new Uri(currImage);
                    image.EndInit();
                    ImageSource = image;
                    GifSource = null;
                }
            }
            else
            {
                ImageSource = null;
                GifSource = null;
            }
        }

        private void SetPage()
        {
            Page = _logic.HasImages() ? $"{(_logic.CurrentImage + 1)}/{_logic.Images.Count}" : "0/0";
        }

        private void SetImageName()
        {
            ImageName = _logic.HasImages()
                ? _logic.GetCurrentImage().Split('\\').Last().Split('.')
                    .First()
                : "";
        }

        private void SetMetaInfos()
        {
            SetPage();
            SetImageName();
            if (_logic.Images.Count == 0)
            {
                DirPath = "";
            }
        }
    }
}