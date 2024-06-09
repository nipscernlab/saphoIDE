using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using System.Windows.Forms; // Added this line at the top of the file
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Controls;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

using WpfButton = System.Windows.Controls.Button;
using FormsButton = System.Windows.Forms.Button;
using WpfMessageBox = System.Windows.MessageBox;
using FormsMessageBox = System.Windows.Forms.MessageBox;
using WpfOpenFileDialog = Microsoft.Win32.OpenFileDialog;
using FormsOpenFileDialog = System.Windows.Forms.OpenFileDialog;
using WpfOrientation = System.Windows.Controls.Orientation;
using FormsOrientation = System.Windows.Forms.Orientation;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using FormsKeyEventArgs = System.Windows.Forms.KeyEventArgs;
using System.Windows.Documents;
using System.Text;
using System.Windows.Media.Animation;
using System.Windows.Interop;
using System.Diagnostics;
using MahApps.Metro.Controls;


namespace Sapho_IDE_New
{
    public partial class MainWindow : MetroWindow
    {

        private string currentDirectory = Directory.GetCurrentDirectory(); // Armazena o diretório atual

        public MainWindow()
        {
            InitializeComponent();

            // Carrega a sintaxe de destaque do arquivo XSHD
            using (Stream s = File.OpenRead("CMM.xshd"))
            {
                if (s != null)
                {
                    using (XmlTextReader reader = new XmlTextReader(s))
                    {
                        CodeEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                        
                    }
                }
            }



            // Ajusta a configuração inicial do terminal
            Terminal.ShowLineNumbers = false;
            Terminal.WordWrap = true;
            Terminal.FontFamily = new FontFamily("Consolas"); // Define a fonte
            Terminal.FontSize = 16; // Ajusta o tamanho da fonte
            Terminal.Foreground = Brushes.White; // Define a cor do texto como branco

            // Adiciona manipulador de evento para detectar a tecla Enter
            Terminal.TextArea.KeyDown += Terminal_KeyDown;

            // Exibe uma mensagem de boas-vindas no terminal Avalon
            //ShowWelcomeMessage();
        }

        /*
        private void ShowWelcomeMessage()
        {
            // Mensagem de boas-vindas
            string welcomeMessage = $"Bem-vindo à Sapho IDE!\n\n" +
                                     $"Esta é uma ferramenta poderosa para desenvolvimento de software.\n" +
                                     $"Digite os comandos no terminal abaixo e pressione Enter para executá-los.\n" +
                                     $"Experimente digitar 'help' para obter ajuda.\n\n";

            // Adiciona a mensagem de boas-vindas ao terminal
            Terminal.AppendText(welcomeMessage);
        }*/

        private async void Terminal_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true; // Impede a inserção de nova linha no TextEditor

                // Obtém o comando digitado pelo usuário
                string command = Terminal.Document.Text.Trim();

                if (!string.IsNullOrEmpty(command))
                {
                    // Imprime o diretório atual e o último comando executado
                    Terminal.AppendText($"\n{currentDirectory}> {command}\n");

                    // Executa o comando e exibe a resposta no terminal
                    await ExecuteCommand(command);
                }
            }
        }

        private async Task ExecuteCommand(string command)
        {
            try
            {
                // Define as configurações do processo
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c cd /d {currentDirectory} & {command}", // Muda para o diretório atual e executa o comando
                    RedirectStandardOutput = true, // Redireciona a saída para que possamos lê-la
                    UseShellExecute = false, // Necessário para redirecionar a saída
                    CreateNoWindow = true // Não exibir a janela do prompt de comando
                };

                // Cria o processo
                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;

                    // Inicia o processo
                    process.Start();

                    // Lê a saída do processo de forma assíncrona
                    string output = await process.StandardOutput.ReadToEndAsync();

                    // Aguarda o término do processo
                    process.WaitForExit();

                    // Adiciona a resposta ao terminal
                    Terminal.AppendText(output);
                }
            }
            catch (Exception ex)
            {
                // Em caso de exceção, exibe uma mensagem de erro
                WpfMessageBox.Show($"Erro ao executar o comando: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private enum Theme
{
    Light,
    Dark,
    Purple,
    Amoled
}

private Theme currentTheme = Theme.Light;

private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
{
    switch (currentTheme)
    {
        case Theme.Light:
            // Alterar para o tema escuro
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new System.Uri("DarkTheme.xaml", System.UriKind.Relative) });
            currentTheme = Theme.Dark;
            break;
        case Theme.Dark:
            // Alterar para o tema roxo
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new System.Uri("PurpleTheme.xaml", System.UriKind.Relative) });
            currentTheme = Theme.Purple;
            break;
        case Theme.Purple:
            // Alterar para o tema amoled
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new System.Uri("AmoledTheme.xaml", System.UriKind.Relative) });
            currentTheme = Theme.Amoled;
            break;
        case Theme.Amoled:
            // Alterar para o tema claro
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new System.Uri("LightTheme.xaml", System.UriKind.Relative) });
            currentTheme = Theme.Light;
            break;
        default:
            break;
    }
}




    }
}

