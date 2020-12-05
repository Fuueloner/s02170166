using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfWebClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window//, INotifyPropertyChanged
    {
        public readonly ObservableCollection<Image> mGlobalImagesList = new ObservableCollection<Image>();
        private readonly Client                     mClient;
        private readonly OpenFileDialog             mSelectImageDialog = new OpenFileDialog() { Filter = "Images | *.jpg;*.jpeg;*.png" };

        //public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();

            mClient = new Client();
            mClient.OnAllImagesRecieved += RefreshGlobalImagesList;
            mClient.OnConnectionFailed += NotifyAboutConnectionFailed;
            mClient.OnDbCleared += NotifyAboutDbCleared;
            mClient.OnImageProcessed += ShowImageProcessingResult;
            mClient.OnStatisticsRecieved += ShowStatistics;

            CommandBinding commandBinding = new CommandBinding {
                Command = ApplicationCommands.Open
            };
            commandBinding.Executed += this.SendRecognitionRequest;
            this.CommandBindings.Add(commandBinding);

            commandBinding = new CommandBinding {
                Command = UserCommands.ShowStatisticsCommand
            };
            commandBinding.Executed += this.SendStatisticsRequest;
            this.CommandBindings.Add(commandBinding);

            commandBinding = new CommandBinding
            {
                Command = ApplicationCommands.Delete
            };
            commandBinding.Executed += this.SendClearingDbRequest;
            this.CommandBindings.Add(commandBinding);

            ImagesListBox.ItemsSource = mGlobalImagesList;

            mClient.GetAllImages();
        }

        private void SendGettingAllImagesRequest(object sender, ExecutedRoutedEventArgs e)
        {
            mClient.GetAllImages();
        }

        private void SendRecognitionRequest(object sender, ExecutedRoutedEventArgs e)
        {
            if (mSelectImageDialog.ShowDialog() == true)
                mClient.ProcessImage(mSelectImageDialog.FileName);
        }

        private void SendStatisticsRequest(object sender, ExecutedRoutedEventArgs e)
        {
            mClient.GetStatistics();
        }

        private void SendClearingDbRequest(object sender, ExecutedRoutedEventArgs e)
        {
            mClient.ClearDb();
        }

        private void RefreshGlobalImagesList(ImageView[] source)
        {
            mGlobalImagesList.Clear();
            foreach (var obj in source)
            {
                BitmapImage bitmapImage = FromBytesToBitMapImage(obj.ImageData);
                Image newImage = new Image
                {
                    Source = bitmapImage,
                    Stretch = Stretch.Uniform,
                    StretchDirection = StretchDirection.DownOnly,
                    Width = 100,
                    Height = 100
                };
                mGlobalImagesList.Add(newImage);
            }
        }

        private void ShowStatistics(Dictionary<string, int> stats)
        {

            StatisticsWindow statisticsWindow = new StatisticsWindow(stats);
            statisticsWindow.DataContext = this;
            statisticsWindow.Show();
        }

        private void NotifyAboutConnectionFailed()
        {
            MessageBox.Show("For unknown reason connection to server has been failed.", "Error!",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void NotifyAboutDbCleared()
        {
            MessageBox.Show("Images database has been successfully cleared!", "Notification",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowImageProcessingResult(ImageView imageView)
        {
            BitmapImage bitmapImage = FromBytesToBitMapImage(imageView.ImageData);

            ProcessedImage.Source = bitmapImage;

            ProcessedImageInfoTextBlock.Text = imageView.ImagePath + "\nPrediction: " + imageView.ClassOfImage + "\nConfidence: " + (Math.Round(imageView.Confidence * 100.0, 2)).ToString();
            ProcessedImageInfoTextBlock.Text += " %";

        }

        private BitmapImage FromBytesToBitMapImage(byte[] input)
        {
            MemoryStream byteStream = new MemoryStream(input);
            BitmapImage bitMapImage = new BitmapImage();
            bitMapImage.BeginInit();
            bitMapImage.StreamSource = byteStream;
            bitMapImage.EndInit();

            return bitMapImage;
        }
    }
}
