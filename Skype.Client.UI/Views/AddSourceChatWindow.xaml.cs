using MahApps.Metro.Controls;
using Skype.Client.UI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Skype.Client.UI.Views
{
    /// <summary>
    /// Interaction logic for AddSourceChatWindow.xaml
    /// </summary>
    public partial class AddSourceChatWindow : MetroWindow
    {
        public AddSourceChatWindow(ObservableCollection<SourceProfileVM> profiles)
        {
            InitializeComponent();
            this.DataContext = new ViewModels.AddSourceChatViewModel(profiles);
        }
        public AddSourceChatWindow(ObservableCollection<DestinationProfileVM> profiles)
        {
            InitializeComponent();
            this.DataContext = new ViewModels.AddSourceChatViewModel(profiles);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
