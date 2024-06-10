using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using MahApps.Metro.Controls;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;


namespace Sapho_IDE_New
{
    public partial class MainWindow : MetroWindow
    {
        private CompletionWindow completionWindow;
        private const double MinFontSize = 10.0;  // Tamanho mínimo da fonte
        private const double MaxFontSize = 60.0; // Tamanho máximo da fonte
        private string currentDirectory = Directory.GetCurrentDirectory(); // Stores the current directory
        private readonly IList<MyCompletionData> completionDataList = new List<MyCompletionData>();

        public MainWindow()
        {
            InitializeComponent();
            InitializeTerminal();
            KeyDown += MainWindow_KeyDown;

            // Load syntax highlighting from XSHD file
            using (Stream s = File.OpenRead("CMM.xshd"))
            {
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    CodeEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }

            // Populate completion data list
            string[] keywords = { "for", "while", "if", "else", "do", "switch", "case", "default", "break", "continue", "return", "void", "int", "char", "float", "double", "bool", "true", "false", "null", "struct", "class" };
            foreach (string keyword in keywords)
            {
                completionDataList.Add(new MyCompletionData(keyword));
            }

            // Initial terminal configuration
            Terminal.ShowLineNumbers = false;
            Terminal.WordWrap = true;
            Terminal.FontFamily = new FontFamily("Consolas"); // Set the font
            Terminal.FontSize = 16; // Set the font size
            Terminal.Foreground = Brushes.White; // Set text color to white

            // Add event handler to detect Enter key
            Terminal.TextArea.KeyDown += Terminal_KeyDown;

            // Add event handler for auto-complete
            CodeEditor.TextArea.TextEntering += TextArea_TextEntering;
            CodeEditor.TextArea.TextEntered += TextArea_TextEntered;
        }

        private void InitializeTerminal()
        {
            // Configure terminal appearance
            Terminal.FontFamily = new FontFamily("Consolas");
            Terminal.FontSize = 16;
            Terminal.Foreground = Brushes.White;

            // Set initial terminal prompt
            DisplayPrompt();

            // Add event handler for Enter key press
            Terminal.KeyDown += Terminal_KeyDown;
        }

        private void DisplayPrompt()
        {
            Terminal.AppendText($"{currentDirectory}> ");
            Terminal.CaretOffset = Terminal.Text.Length;
            Terminal.IsReadOnly = false; // torna a área de texto somente leitura
        }

        private async void Terminal_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true; // Prevenir nova linha

                // Obter o comando inserido pelo usuário
                string command = Terminal.Text.TrimEnd(); // Remover espaços em branco extras

                if (!string.IsNullOrEmpty(command))
                {
                    // Exibir o comando no terminal Avalon antes de executá-lo
                    Terminal.AppendText($"{currentDirectory}> {command}{Environment.NewLine}");

                    // Se o comando for uma mudança de diretório, atualize o diretório atual
                    if (command.StartsWith("cd ", StringComparison.OrdinalIgnoreCase))
                    {
                        string newDirectory = command.Substring(3).Trim(); // Remove "cd " do início do comando
                        if (!Path.IsPathRooted(newDirectory))
                        {
                            // Se o novo diretório não for um caminho absoluto, combine-o com o diretório atual
                            newDirectory = Path.Combine(currentDirectory, newDirectory);
                        }
                        if (Directory.Exists(newDirectory))
                        {
                            // Se o novo diretório existir, atualize o diretório atual
                            currentDirectory = newDirectory;
                            // Exiba o novo diretório
                            Terminal.AppendText($"{currentDirectory}> ");
                        }
                        else
                        {
                            // Se o novo diretório não existir, exiba uma mensagem de erro
                            Terminal.AppendText($"Directory '{newDirectory}' not found{Environment.NewLine}");
                            // Exiba o diretório atual
                            Terminal.AppendText($"{currentDirectory}> ");
                        }
                    }
                    else
                    {
                        // Se o comando não for uma mudança de diretório, execute-o normalmente
                        await ExecuteCommand(command);
                        // Adicionar um novo prompt para o próximo comando
                        Terminal.AppendText(Environment.NewLine);
                        Terminal.AppendText($"{currentDirectory}> ");
                    }
                }
            }
            else if (e.Key == Key.Enter && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                e.Handled = true; // Prevenir nova linha
            }
        }



        private async Task ExecuteCommand(string command)
        {
            try
            {
                // Process settings
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c cd /d {currentDirectory} & {command}", // Change to the current directory and execute the command
                    RedirectStandardOutput = true, // Redirect output to read it
                    RedirectStandardError = true, // Redirect standard error to read it
                    UseShellExecute = false, // Required to redirect output
                    CreateNoWindow = true // Do not show the command prompt window
                };

                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    // Read standard output and standard error asynchronously
                    Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
                    Task<string> errorTask = process.StandardError.ReadToEndAsync();

                    // Wait for both tasks to complete
                    await Task.WhenAll(outputTask, errorTask);

                    // Get the output and error messages
                    string output = outputTask.Result;
                    string error = errorTask.Result;

                    // Combine output and error messages
                    string result = output + error;

                    Terminal.AppendText(result); // Add the response to the terminal
                }
            }
            catch (Exception ex)
            {
                Terminal.AppendText($"Error executing command: {ex.Message}");
            }
        }




        private enum Theme
        {
            Light,
            Dark,
            Cern,
            Amoled
        }

        private Theme currentTheme = Theme.Light;

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            Resources.MergedDictionaries.Clear();

            switch (currentTheme)
            {
                case Theme.Light:
                    Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("/Themes/DarkTheme.xaml", UriKind.Relative) });
                    currentTheme = Theme.Dark;
                    break;
                case Theme.Dark:
                    Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("/Themes/CernTheme.xaml", UriKind.Relative) });
                    currentTheme = Theme.Cern;
                    break;
                case Theme.Cern:
                    Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("/Themes/AmoledTheme.xaml", UriKind.Relative) });
                    currentTheme = Theme.Amoled;
                    break;
                case Theme.Amoled:
                    Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("/Themes/LightTheme.xaml", UriKind.Relative) });
                    currentTheme = Theme.Light;
                    break;
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && (e.Key == Key.OemPlus || e.Key == Key.Add))
            {
                AdjustZoom(2.0); // Increase zoom
            }
            else if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && (e.Key == Key.OemMinus || e.Key == Key.Subtract))
            {
                AdjustZoom(-2.0); // Decrease zoom
            }
        }

        private void AdjustZoom(double zoomDelta)
        {
            double newEditorFontSize = CodeEditor.FontSize + zoomDelta;
            double newTerminalFontSize = Terminal.FontSize + zoomDelta;

            // Ajuste o tamanho da fonte do editor de código dentro dos limites
            if (newEditorFontSize >= MinFontSize && newEditorFontSize <= MaxFontSize)
            {
                CodeEditor.FontSize = newEditorFontSize;
            }

            // Ajuste o tamanho da fonte do terminal dentro dos limites
            if (newTerminalFontSize >= MinFontSize && newTerminalFontSize <= MaxFontSize)
            {
                Terminal.FontSize = newTerminalFontSize;
            }
        }

        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (completionWindow != null)
            {
                return;
            }

            // Define common C keywords for autocomplete
            string[] keywords = { "for", "while", "if", "else", "do", "switch", "case", "default", "break", "continue", "return", "void", "int", "char", "float", "double", "bool", "true", "false", "null", "struct", "class" };

            if (char.IsLetter(e.Text[0]))
            {
                completionWindow = new CompletionWindow(CodeEditor.TextArea);
                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                foreach (string keyword in keywords)
                {
                    data.Add(new MyCompletionData(keyword));
                }
                // Filter the data based on the input text
                var filteredData = data.Cast<MyCompletionData>().Where(item => item.Text.StartsWith(e.Text, StringComparison.InvariantCultureIgnoreCase)).OrderBy(item => item.GetRelevance(e.Text)).ToList();
                completionWindow.CompletionList.CompletionData.Clear();
                foreach (var item in filteredData)
                {
                    completionWindow.CompletionList.CompletionData.Add(item);
                }

                completionWindow.Show();
                completionWindow.Closed += delegate
                {
                    completionWindow = null;
                };

                ApplyCompletionWindowStyles(completionWindow);
            }
        }



        private void ApplyCompletionWindowStyles(CompletionWindow completionWindow)
        {

            // Defina a cor de fundo
            completionWindow.Background = Brushes.White; // Define a cor de fundo como preto

            // Defina a cor do texto
            completionWindow.Foreground = Brushes.Black; // Define a cor do texto como branco
            completionWindow.CompletionList.FontFamily = new FontFamily("Arial"); // Define a fonte como Arial


            var resources = new ResourceDictionary();
            resources.MergedDictionaries.Add(Application.Current.Resources);
            completionWindow.Resources = resources;

            // Set the most relevant item as the selected item
            if (completionWindow.CompletionList.CompletionData.Count > 0)
            {
                completionWindow.CompletionList.SelectedItem = completionWindow.CompletionList.CompletionData[0];
            }

        }


        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }

            // Auto-closing brackets and quotes
            if (e.Text == "(")
            {
                CodeEditor.TextArea.Document.Insert(CodeEditor.TextArea.Caret.Offset, ")");
                CodeEditor.TextArea.Caret.Offset--;
            }
            else if (e.Text == "{")
            {
                CodeEditor.TextArea.Document.Insert(CodeEditor.TextArea.Caret.Offset, "}");
                CodeEditor.TextArea.Caret.Offset--;
            }
            else if (e.Text == "[")
            {
                CodeEditor.TextArea.Document.Insert(CodeEditor.TextArea.Caret.Offset, "]");
                CodeEditor.TextArea.Caret.Offset--;
            }
            else if (e.Text == "\"")
            {
                CodeEditor.TextArea.Document.Insert(CodeEditor.TextArea.Caret.Offset, "\"");
                CodeEditor.TextArea.Caret.Offset--;
            }
            else if (e.Text == "'")
            {
                CodeEditor.TextArea.Document.Insert(CodeEditor.TextArea.Caret.Offset, "'");
                CodeEditor.TextArea.Caret.Offset--;
            }
        }
    }

    // Define the completion data class
    public class MyCompletionData : ICompletionData
    {
        public MyCompletionData(string text)
        {
            Text = text;
        }

        public System.Windows.Media.ImageSource Image => null;

        public string Text { get; private set; }

        // Use this property if you want to show some fancy UIElement in the list
        public object Content => Text;

        public object Description => "C keyword";

        public double Priority => 0;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            int startOffset = completionSegment.Offset;

            // Find the beginning of the word to be replaced
            while (startOffset > 0 && char.IsLetterOrDigit(textArea.Document.GetCharAt(startOffset - 1)))
            {
                startOffset--;
            }

            // Replace the word
            textArea.Document.Replace(startOffset, completionSegment.EndOffset - startOffset, Text);
        }

        // Method to calculate relevance based on the similarity with the input
        public int GetRelevance(string input)
        {
            if (Text.StartsWith(input, StringComparison.InvariantCultureIgnoreCase))
            {
                return 0; // Exact match
            }
            if (Text.Contains(input, StringComparison.InvariantCultureIgnoreCase))
            {
                return 1; // Partial match
            }
            return 2; // No match
        }
    }
}
