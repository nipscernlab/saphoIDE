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

namespace Sapho_IDE_New
{
    public partial class CustomMessageBox : Window
    {
        public CustomMessageBox(string message)
        {
            InitializeComponent();
            DataContext = new CustomMessageBoxViewModel(message);
            Owner = Application.Current.MainWindow; // Define a janela "NewProj" como a proprietária da janela "CustomMessageBox"
            WindowStartupLocation = WindowStartupLocation.CenterOwner; // Define a posição inicial da janela "CustomMessageBox" como centrada em relação à "NewProj"
        }
    }

    public class CustomMessageBoxViewModel
    {
        public string Message { get; set; }

        public CustomMessageBoxViewModel(string message)
        {
            Message = message;
        }
    }
}
