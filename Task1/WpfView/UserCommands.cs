using System.Windows.Input;

namespace WpfView
{
    public class UserCommands
    {
        public static RoutedCommand LaunchProcessingCommand { get; set; }
        public static RoutedCommand ShowStatisticsCommand { get; set; }
        public static RoutedCommand ClearStatisticsCommand { get; set; }
        static UserCommands()
        {
            LaunchProcessingCommand = new RoutedCommand("LaunchProcessingCommand", typeof(MainWindow));
            ShowStatisticsCommand = new RoutedCommand("ShowStatisticsCommand", typeof(MainWindow));
            ClearStatisticsCommand = new RoutedCommand("ClearStatisticsCommand", typeof(StatisticsWindow));
        }
    }
}