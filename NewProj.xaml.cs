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
    public partial class NewProj : Window
    {
        public NewProj()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow; // Define a janela "MainWindow" como a proprietária da janela "NewProj"
            WindowStartupLocation = WindowStartupLocation.CenterOwner; // Define a posição inicial da janela "NewProj" como centrada em relação à "MainWindow"
            ResizeMode = ResizeMode.NoResize; // Impede que a janela seja redimensionável
        }
    }
}

