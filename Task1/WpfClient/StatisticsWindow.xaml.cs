using System.Collections.Generic;
using System.Windows;

namespace WpfWebClient
{
    /// <summary>
    /// Логика взаимодействия для StatisticsWindow.xaml
    /// </summary>
    public partial class StatisticsWindow : Window
    {
        private Dictionary<string, int> mStats;
        public StatisticsWindow(Dictionary<string, int> stats)
        {
            mStats = stats;
            InitializeComponent();
            StatisticsListView.ItemsSource = mStats;
        }
    }
}
