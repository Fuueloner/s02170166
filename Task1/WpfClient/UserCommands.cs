using System.Windows.Input;

namespace WpfWebClient
{
    public class UserCommands
    {
        public static RoutedCommand ShowStatisticsCommand { get; set; }
        static UserCommands()
        {
            ShowStatisticsCommand = new RoutedCommand("ShowStatisticsCommand", typeof(MainWindow));
        }
    }
}
