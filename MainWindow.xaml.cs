using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;

namespace Sapho_IDE_New
{
    public partial class MainWindow : Window
    {
        private List<string> openedFilePaths = new List<string>();
        private Dictionary<TextEditor, Stack<string>> undoHistory = new Dictionary<TextEditor, Stack<string>>();

        public MainWindow()
        {
            InitializeComponent();
            LoadOpenedFiles();
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
                MessageBox.Show($"Erro ao carregar arquivos abertos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"Erro ao salvar arquivos abertos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenFileMenu_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Arquivos C e ASM (*.c;*.asm)|*.c;*.asm|Todos os Arquivos (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;

                if (File.Exists(selectedFilePath))
                {
                    OpenFile(selectedFilePath);
                    openedFilePaths.Add(selectedFilePath);
                    SaveOpenedFiles();
                }
                else
                {
                    MessageBox.Show($"O arquivo '{selectedFilePath}' não foi encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenFile(string filePath)
        {
            TabItem newTab = new TabItem();
            DockPanel headerPanel = new DockPanel();
            StackPanel innerPanel = new StackPanel { Orientation = Orientation.Horizontal };

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

            Button closeButton = new Button
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
                    MessageBox.Show("Salve o arquivo antes de fechá-lo.");
                }
            };

            innerPanel.Children.Add(closeButton);
            innerPanel.Children.Add(modifiedIndicator);
            headerPanel.Children.Add(innerPanel);

            newTab.Header = headerPanel;
            newTab.Tag = filePath;

            string fileContent = File.ReadAllText(filePath);
            TextEditor textEditor = new TextEditor
            {
                FontFamily = new FontFamily("Consolas"),
                FontSize = 15,
                Background = Brushes.LightGray,
                Padding = new Thickness(10, 0, 10, 0),
                ShowLineNumbers = true,
                Text = fileContent
            };

            textEditor.TextChanged += (s, args) =>
            {
                isModified = true;
                modifiedIndicator.Visibility = Visibility.Visible;
                closeButton.Content = "•";
                if (!undoHistory.ContainsKey(textEditor))
                {
                    undoHistory.Add(textEditor, new Stack<string>());
                }
                undoHistory[textEditor].Push(textEditor.Text);
            };

            newTab.Content = textEditor;
            FileTabControl.Items.Add(newTab);
            FileTabControl.SelectedItem = newTab;

            textEditor.KeyDown += (s, args) =>
            {
                if ((args.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control) ||
                    (args.Key == Key.S && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift)))
                {
                    SaveCurrentFile(filePath, textEditor);
                    isModified = false;
                    modifiedIndicator.Visibility = Visibility.Hidden;
                    closeButton.Content = "x";
                    args.Handled = true;
                }
                else if ((args.Key == Key.Z && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift)))
                {
                    if (undoHistory.ContainsKey(textEditor) && undoHistory[textEditor].Count > 1)
                    {
                        undoHistory[textEditor].Pop(); // Ignora o estado atual
                        textEditor.Text = undoHistory[textEditor].Pop(); // Restaura o estado anterior
                    }
                    args.Handled = true;
                }
            };
        }

        private void SaveCurrentFile(string filePath, TextEditor textEditor)
        {
            try
            {
                File.WriteAllText(filePath, textEditor.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro ao salvar o arquivo:\n\n{ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void Window_KeyDown(object sender, KeyEventArgs e)
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
