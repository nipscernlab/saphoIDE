using System;
using System.Windows;

namespace Sapho_IDE_New
{
    public partial class NewProj : Window
    {
        public NewProj()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
            {
                ProjectDirectoryTextBox.Text = dialog.FileName;
            }
        }

        private void CreateProject_Click(object sender, RoutedEventArgs e)
        {
            string projectName = ProjectNameTextBox.Text;
            string projectDirectory = ProjectDirectoryTextBox.Text;

            if (!string.IsNullOrEmpty(projectName) && !string.IsNullOrEmpty(projectDirectory))
            {
                try
                {
                    string projectFilePath = System.IO.Path.Combine(projectDirectory, projectName + ".spf");

                    using (System.IO.FileStream fs = System.IO.File.Create(projectFilePath))
                    {
                        // O arquivo é criado vazio
                    }

                    // Exibe a mensagem de sucesso usando a janela personalizada
                    CustomMessageBox successMessage = new CustomMessageBox("Project created successfully!");
                    successMessage.ShowDialog();

                    // Fecha a janela "NewProj"
                    this.Close();
                }
                catch (Exception ex)
                {
                    // Verifica se a mensagem de erro ultrapassa 50 caracteres e corta, se necessário
                    string errorMessageText = ex.Message.Length > 100 ? ex.Message.Substring(0, Math.Min(ex.Message.Length, 75)) + "..." : ex.Message;

                    // Exibe a mensagem de erro usando a janela personalizada
                    CustomMessageBox errorMessage = new CustomMessageBox($"An error occurred while creating the project:\n{errorMessageText}");
                    errorMessage.ShowDialog();
                }
            }
            else
            {
                CustomMessageBox errorMessage = new CustomMessageBox("Please enter project name and select project directory.");
                errorMessage.ShowDialog();
            }
        }
    }
}
