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


namespace Sapho_IDE_New
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            List<string> initialFilesAndFolders = new List<string>(); // Crie uma lista vazia
            LoadFileTree(initialFilesAndFolders); // Passe a lista vazia para o método LoadFileTree
            LoadOpenedFiles();
        }


        private bool isLightTheme = true;

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (isLightTheme)
            {
                // Alterar para o tema escuro
                Resources.MergedDictionaries.Clear();
                Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new System.Uri("LightTheme.xaml", System.UriKind.Relative) });
                isLightTheme = false;
            }
            else
            {
                // Alterar para o tema claro
                Resources.MergedDictionaries.Clear();
                Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new System.Uri("DarkTheme.xaml", System.UriKind.Relative) });
                isLightTheme = true;
            }
        }



        private List<string> openedFilePaths = new List<string>();
        private Dictionary<TextEditor, Stack<string>> undoHistory = new Dictionary<TextEditor, Stack<string>>();

        private void LoadFileTree(List<string> filesAndFolders)
        {
            FileTreeView.Items.Clear();
            foreach (var item in filesAndFolders)
            {
                if (Directory.Exists(item))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(item);
                    AddDirectoryNodes(directoryInfo, null);
                }
                else if (File.Exists(item))
                {
                    AddFileNode(item, null);
                }
            }
        }

        private void AddFileNode(string filePath, TreeViewItem parentNode)
        {
            TreeViewItem fileNode = new TreeViewItem();
            fileNode.Header = Path.GetFileName(filePath);
            fileNode.Tag = filePath; // Armazena o caminho do arquivo como tag do nó

            fileNode.MouseDoubleClick += (sender, e) =>
            {
                OpenFile(filePath);
            };

            if (parentNode == null)
            {
                // Adiciona ao nível superior do TreeView
                FileTreeView.Items.Add(fileNode);
            }
            else
            {
                // Adiciona como um nó filho do nó pai
                parentNode.Items.Add(fileNode);
            }
        }


        private void AddDirectoryNodes(DirectoryInfo directory, TreeViewItem parentNode)
        {
            TreeViewItem directoryNode = new TreeViewItem
            {
                Header = directory.Name,
                Tag = directory.FullName // Armazena o caminho completo do diretório
            };

            // Adiciona um placeholder para expansão
            directoryNode.Items.Add("Loading...");

            // Adiciona um manipulador de eventos para carregar conteúdo quando expandido
            directoryNode.Expanded += DirectoryNode_Expanded;

            if (parentNode == null)
            {
                FileTreeView.Items.Add(directoryNode);
            }
            else
            {
                parentNode.Items.Add(directoryNode);
            }
        }

        private void DirectoryNode_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item.Items.Count == 1 && item.Items[0] is string) // Verifica se já foi carregado
            {
                item.Items.Clear(); // Limpa o placeholder

                DirectoryInfo directory = new DirectoryInfo((string)item.Tag);
                try
                {
                    foreach (var subDirectory in directory.GetDirectories())
                    {
                        AddDirectoryNodes(subDirectory, item);
                    }
                    foreach (var file in directory.GetFiles())
                    {
                        AddFileNode(file.FullName, item);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // Tratamento da exceção
                    WpfMessageBox.Show($"Acesso negado: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Volte para a tela principal (MainWindow)
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    Close(); // Feche a janela atual, se necessário
                }
            }
        }




        private void LoadOpenedFiles()
        {
            try
            {
                string filePath = "opened_files.txt";
                if (File.Exists(filePath))
                {
                    openedFilePaths.AddRange(File.ReadAllLines(filePath));
                    foreach (string file in openedFilePaths)
                    {
                        OpenFile(file);
                    }
                }
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show($"Erro ao carregar arquivos abertos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveOpenedFiles()
        {
            try
            {
                string filePath = "opened_files.txt";
                File.WriteAllLines(filePath, openedFilePaths);
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show($"Erro ao salvar arquivos abertos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenFileMenu_Click(object sender, RoutedEventArgs e)
        {
            // Cria uma caixa de diálogo para seleção de arquivos
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = true, // Permite a seleção de múltiplos arquivos
                Filter = "Arquivos C e ASM (*.c;*.asm)|*.c;*.asm|Todos os Arquivos (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string fileName in openFileDialog.FileNames)
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(fileName);

                        // Verifica se o tamanho do arquivo é maior que 10 MB
                        if (fileInfo.Length > 10 * 1024 * 1024) // 10 MB em bytes
                        {
                            // Exibe um aviso se o arquivo for maior que 10 MB
                            WpfMessageBox.Show($"O arquivo \"{fileName}\" é muito grande (mais de 10 MB). Pode levar mais tempo para carregar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }

                        // Verifica o número de linhas do arquivo
                        int numLines = File.ReadLines(fileName).Count();
                        if (numLines > 30000000) // 30 milhões de linhas
                        {
                            // Exibe um aviso se o arquivo tiver mais de 30 milhões de linhas
                            WpfMessageBox.Show($"O arquivo \"{fileName}\" tem mais de 30 milhões de linhas de código. Pode levar mais tempo para carregar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }

                        // Carrega o arquivo se estiver dentro dos limites
                        OpenFile(fileName);
                    }
                    catch (Exception ex)
                    {
                        // Trata qualquer exceção durante o processo de abertura do arquivo
                        WpfMessageBox.Show($"Erro ao abrir o arquivo \"{fileName}\": {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }




        private void OpenFolderMenu_Click(object sender, RoutedEventArgs e)
        {
            // Cria uma caixa de diálogo para seleção de pasta
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string selectedPath = folderDialog.SelectedPath;

                    // Adiciona a pasta selecionada à lista de arquivos e pastas a serem carregados na árvore
                    List  < string > filesAndFolders = new List < string>
                    { selectedPath };

                    // Passa os arquivos e pastas selecionados para o método LoadFileTree
                    LoadFileTree(filesAndFolders);
                }
            }
        }




        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string selectedPath = folderDialog.SelectedPath;
                    List<string> filesAndFolders = new List<string> { selectedPath };
                    LoadFileTree(filesAndFolders);
                }
            }
        }



        private async Task OpenFile(string filePath)
        {
            try
            {
                const int bufferSize = 4096; // Tamanho do buffer de leitura em bytes
                const int linesPerPage = 100; // Número de linhas por página
                TabItem newTab = new TabItem();
                DockPanel headerPanel = new DockPanel();
                StackPanel innerPanel = new StackPanel { Orientation = WpfOrientation.Horizontal };

                TextBlock fileNameTextBlock = new TextBlock
                {
                    Text = Path.GetFileName(filePath),
                    Margin = new Thickness(0, 0, 5, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                innerPanel.Children.Add(fileNameTextBlock);

                TextBlock modifiedIndicator = new TextBlock
                {
                    Text = "•",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Red,
                    Margin = new Thickness(0, 0, 5, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Visibility = Visibility.Hidden
                };

                WpfButton closeButton = new WpfButton
                {
                    Content = "x",
                    Width = 24,
                    Height = 24,
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.Transparent,
                    Padding = new Thickness(0),
                    Margin = new Thickness(5, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Visibility = Visibility.Visible
                };

                bool isModified = false;

                closeButton.Click += (s, args) =>
                {
                    if (!isModified)
                    {
                        FileTabControl.Items.Remove(newTab);
                    }
                    else
                    {
                        WpfMessageBox.Show("Salve o arquivo antes de fechá-lo.");
                    }
                };

                innerPanel.Children.Add(closeButton);
                innerPanel.Children.Add(modifiedIndicator);
                headerPanel.Children.Add(innerPanel);

                newTab.Header = headerPanel;
                newTab.Tag = filePath;

                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize, FileOptions.SequentialScan);
                var reader = new StreamReader(fileStream);

                var textEditor = new TextEditor
                {
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 15,
                    Background = Brushes.LightGray,
                    Padding = new Thickness(10, 0, 10, 0),
                    ShowLineNumbers = true
                };

                newTab.Content = textEditor;
                FileTabControl.Items.Add(newTab);
                FileTabControl.SelectedItem = newTab;

                var buffer = new char[bufferSize];
                int bytesRead;
                int linesRead = 0;
                StringBuilder linesBuffer = new StringBuilder();

                while ((bytesRead = await reader.ReadAsync(buffer, 0, bufferSize)) > 0)
                {
                    for (int i = 0; i < bytesRead; i++)
                    {
                        if (buffer[i] == '\n')
                        {
                            linesRead++;
                            linesBuffer.Append(buffer[i]);

                            if (linesRead % linesPerPage == 0)
                            {
                                textEditor.AppendText(linesBuffer.ToString());
                                linesBuffer.Clear();
                            }
                        }
                        else
                        {
                            linesBuffer.Append(buffer[i]);
                        }
                    }
                }

                if (linesBuffer.Length > 0)
                {
                    textEditor.AppendText(linesBuffer.ToString());
                }

                reader.Close();
                fileStream.Close();
            }
            catch (OutOfMemoryException ex)
            {
                WpfMessageBox.Show($"Erro ao abrir o arquivo: {ex.Message}", "Erro de Memória", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show($"Erro ao abrir o arquivo: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void SaveCurrentFile(string filePath, TextEditor textEditor)
        {
            try
            {
                File.WriteAllText(filePath, textEditor.Text);
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show($"Ocorreu um erro ao salvar o arquivo:\n\n{ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseCurrentTab()
        {
            if (FileTabControl.SelectedItem != null)
            {
                FileTabControl.Items.Remove(FileTabControl.SelectedItem);
            }
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

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OpenNewProject_Click(object sender, RoutedEventArgs e)
        {
            NewProj newProjectWindow = new NewProj();
            newProjectWindow.ShowDialog();
        }

        private void Window_KeyDown(object sender, WpfKeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.W)
            {
                CloseCurrentTab();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            SaveOpenedFiles();
        }
    }
}

