using Microsoft.Win32;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Sapho_IDE_New
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFileMenu(object sender, RoutedEventArgs e)
        {
            ContextMenu fileMenu = (ContextMenu)this.Resources["FileMenu"];
            fileMenu.PlacementTarget = sender as UIElement;
            fileMenu.IsOpen = true;
        }

        private void OpenHelpMenu(object sender, RoutedEventArgs e)
        {
            ContextMenu helpMenu = (ContextMenu)this.Resources["HelpMenu"];
            helpMenu.PlacementTarget = sender as UIElement;
            helpMenu.IsOpen = true;
        }

        private void OpenFileMenu_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Arquivos de Projeto (*.spf)|*.spf|Todos os Arquivos (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                // Lógica para lidar com o arquivo selecionado
                string selectedFilePath = openFileDialog.FileName;
                // Implemente aqui o código para abrir o projeto
            }
        }

        private void OpenNewProject_Click(object sender, RoutedEventArgs e)
        {
            NewProj newProjectWindow = new NewProj();
            newProjectWindow.ShowDialog();
        }


    }
}
