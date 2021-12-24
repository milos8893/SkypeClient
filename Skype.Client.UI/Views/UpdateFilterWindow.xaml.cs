﻿using MahApps.Metro.Controls;
using Skype.Client.UI.Models;
using Skype.Client.UI.ViewModels;
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

namespace Skype.Client.UI.Views
{
    /// <summary>
    /// Interaction logic for UpdateFilterWindow.xaml
    /// </summary>
    public partial class UpdateFilterWindow : MetroWindow
    {
        public UpdateFilterWindow()
        {
            InitializeComponent();
            this.DataContext = new UpdateFilterViewModel();
        }
        public UpdateFilterWindow(Filter filter) : this()
        {
            this.DataContext = new UpdateFilterViewModel(filter);
            this.Title = "Update Filter";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}