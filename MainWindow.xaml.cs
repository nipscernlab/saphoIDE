using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.Reflection;
using ICSharpCode.AvalonEdit;


namespace Sapho_IDE_New
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Carrega a sintaxe de destaque do arquivo XSHD
            using (Stream s = File.OpenRead("CSharp.xshd"))
            {
                if (s != null)
                {
                    using (XmlTextReader reader = new XmlTextReader(s))
                    {
                        CodeEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                        CodeEditor2.SyntaxHighlighting = CodeEditor.SyntaxHighlighting; // Aplica a mesma sintaxe ao segundo TextEditor
                    }
                }
            }

            // Associa o evento KeyDown aos TextEditor's
            CodeEditor.KeyDown += CodeEditor_KeyDown;
            CodeEditor2.KeyDown += CodeEditor_KeyDown;
        }



        private void CodeEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                // Obtém o texto atual antes do cursor
                var textBeforeCursor = ((ICSharpCode.AvalonEdit.TextEditor)sender).Document.GetText(
                    ((ICSharpCode.AvalonEdit.TextEditor)sender).Document.GetLineByOffset(
                        ((ICSharpCode.AvalonEdit.TextEditor)sender).CaretOffset).Offset,
                    ((ICSharpCode.AvalonEdit.TextEditor)sender).CaretOffset -
                    ((ICSharpCode.AvalonEdit.TextEditor)sender).Document.GetLineByOffset(
                        ((ICSharpCode.AvalonEdit.TextEditor)sender).CaretOffset).Offset);

                // Conta o número de espaços antes do cursor
                int spacesBeforeCursor = textBeforeCursor.Length - textBeforeCursor.TrimStart().Length;

                // Obtém a quantidade de espaços necessária para a próxima tabulação
                int spacesToAdd = 4 - (spacesBeforeCursor % 4);

                // Insere os espaços necessários
                ((ICSharpCode.AvalonEdit.TextEditor)sender).Document.Insert(
                    ((ICSharpCode.AvalonEdit.TextEditor)sender).CaretOffset, new string(' ', spacesToAdd));

                // Move o cursor para a posição correta
                ((ICSharpCode.AvalonEdit.TextEditor)sender).CaretOffset += spacesToAdd;

                // Indica que o evento foi tratado
                e.Handled = true;
            }

            var textEditor = (TextEditor)sender;
            var caretOffset = textEditor.CaretOffset;

            switch (e.Key)
            {
                case Key.OemOpenBrackets: // '{'
                    textEditor.Document.Insert(caretOffset, "{}");
                    textEditor.CaretOffset = caretOffset + 1;
                    e.Handled = true;
                    break;

                case Key.OemQuotes: // '"'
                    textEditor.Document.Insert(caretOffset, "\"\"");
                    textEditor.CaretOffset = caretOffset + 1;
                    e.Handled = true;
                    break;

                case Key.OemComma: // '''
                    textEditor.Document.Insert(caretOffset, "''");
                    textEditor.CaretOffset = caretOffset + 1;
                    e.Handled = true;
                    break;

                // Adicione mais casos para outros caracteres, se necessário

                default:
                    break;
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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt && e.Key == Key.F4)
            {
                Close(); // Fecha a janela
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close(); // Fecha a janela principal
        }


        private void OpenNewProject_Click(object sender, RoutedEventArgs e)
        {
            NewProj newProjectWindow = new NewProj();
            newProjectWindow.ShowDialog();
        }



    }


}
