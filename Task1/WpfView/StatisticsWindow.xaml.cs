using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfView
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class StatisticsWindow : Window
    {
        private string mStats = "";
        public StatisticsWindow()
        {
            InitializeComponent();

            using (NetAutumnClassLibrary.ImageInfoContext db = new NetAutumnClassLibrary.ImageInfoContext())
            {
                var statsList = from obj in db.ImageInfos.AsNoTracking().AsQueryable()
                                group obj by obj.ClassName;

                foreach (var imageClass in statsList)
                    mStats += (imageClass.Key.ToString() + " -- " + imageClass.Count().ToString() + " \n");
            }

            StatsTextBlock.Text = mStats;

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
