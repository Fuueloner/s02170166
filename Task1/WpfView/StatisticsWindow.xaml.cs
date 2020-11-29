using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;

namespace WpfView
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class StatisticsWindow : Window
    {
        private readonly string mStats = "";

        private readonly ObservableCollection<Image> mGlobalImageList = new ObservableCollection<Image>();
        public StatisticsWindow()
        {
            InitializeComponent();

            using (NetAutumnClassLibrary.ImageInfoContext db = new NetAutumnClassLibrary.ImageInfoContext())
            {
                var statsList = from obj in db.ImageInfos.AsNoTracking().AsQueryable()
                                group obj by obj.ClassName;

                foreach (var imageClass in statsList)
                    mStats += (imageClass.Key.ToString() + " -- " + imageClass.Count().ToString() + " \n");

                foreach (var obj in db.ImageInfos.AsNoTracking().AsEnumerable())
                {
                    MemoryStream byteStream = new MemoryStream(obj.Image);
                    BitmapImage bitMapImage = new BitmapImage();
                    bitMapImage.BeginInit();
                    bitMapImage.StreamSource = byteStream;
                    bitMapImage.EndInit();
                    Image newImage = new Image
                    {
                        Source = bitMapImage,
                        Stretch = Stretch.Uniform,
                        StretchDirection = StretchDirection.DownOnly,
                        Width = 100,
                        Height = 100
                    };
                    mGlobalImageList.Add(newImage);
                }

            }

            StatsTextBlock.Text = mStats;
            ImagesListBox.ItemsSource = mGlobalImageList;

            CommandBinding commandBinding = new CommandBinding
            {
                Command = UserCommands.ClearStatisticsCommand
            };
            commandBinding.Executed += this.ClearDBAndLeave;
            this.CommandBindings.Add(commandBinding);

        }

        private void ClearDBAndLeave(object sender, RoutedEventArgs e)
        {
            using (NetAutumnClassLibrary.ImageInfoContext db = new NetAutumnClassLibrary.ImageInfoContext())
            {
                //! Не самое эффективное решение, но ничего лучше я пока не придумал :)
                var images = db.ImageInfos;
                foreach (var image in images)
                    images.Remove(image);
                db.SaveChanges();
            }

            this.Close();
        }
    }
}
