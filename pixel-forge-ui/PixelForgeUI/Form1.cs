using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PixelForgeUI
{
    public partial class Form1 : Form
    {
        private string rustExecutablePath = "pxforge.exe"; // Path to the Rust executable    
        private bool isProcessing = false;

        public Form1()
        {
            InitializeComponent();
            SetupFormProperties();
            InitializeUI();
        }

        private void SetupFormProperties()
        {
            
            this.ClientSize = new Size(900, 700);
            this.Text = "PixelForge - Image Converter";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(900, 700);
            this.MaximumSize = new Size(900, 700);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }

        private void InitializeUI()
        {
            
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 6,
                ColumnCount = 3,
                Padding = new Padding(20)
            };

            
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70)); 
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 80)); 
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 160));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 80)); 
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70)); 
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); 

            
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            
            var titleLabel = new Label
            {
                Text = "PixelForge Image Converter",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            mainPanel.Controls.Add(titleLabel, 0, 0);
            mainPanel.SetColumnSpan(titleLabel, 3);

            
            var modeGroupBox = new GroupBox
            {
                Text = "Conversion Mode",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var singleRadio = new RadioButton
            {
                Text = "Single File",
                Checked = true,
                Location = new Point(20, 25),
                AutoSize = true,
                Tag = "single"
            };

            var batchRadio = new RadioButton
            {
                Text = "Batch (Folder)",
                Location = new Point(150, 25),
                AutoSize = true,
                Tag = "batch"
            };

            singleRadio.CheckedChanged += ModeRadio_CheckedChanged;
            batchRadio.CheckedChanged += ModeRadio_CheckedChanged;

            modeGroupBox.Controls.AddRange(new Control[] { singleRadio, batchRadio });
            mainPanel.Controls.Add(modeGroupBox, 0, 1);
            mainPanel.SetColumnSpan(modeGroupBox, 3);

            
            var filePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 3,
                Margin = new Padding(0, 5, 0, 5)
            };

            filePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33f)); 
            filePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33f)); 
            filePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33f)); 

            
            filePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            filePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            filePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

            
            var inputLabel = new Label
            {
                Text = "Input:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            var inputTextBox = new TextBox
            {
                Name = "inputTextBox",
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(3, 8, 3, 8)
            };
            var inputBrowseBtn = new Button
            {
                Text = "Browse File",
                Name = "inputBrowseBtn",
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(3, 5, 3, 5)
            };
            inputBrowseBtn.Click += InputBrowseBtn_Click;

            filePanel.Controls.Add(inputLabel, 0, 0);
            filePanel.Controls.Add(inputTextBox, 1, 0);
            filePanel.Controls.Add(inputBrowseBtn, 2, 0);

            
            var formatLabel = new Label
            {
                Text = "Format:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            var formatComboBox = new ComboBox
            {
                Name = "formatComboBox",
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(3, 8, 3, 8)
            };
            formatComboBox.Items.AddRange(new object[] { "png", "jpg", "jpeg", "gif", "bmp", "tiff", "webp" });
            formatComboBox.SelectedIndex = 0;

            
            var formatHelpLabel = new Label
            {
                Text = "← Choose first",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                ForeColor = Color.DarkBlue,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(3, 8, 3, 8)
            };

            filePanel.Controls.Add(formatLabel, 0, 1);
            filePanel.Controls.Add(formatComboBox, 1, 1);
            filePanel.Controls.Add(formatHelpLabel, 2, 1);

            
            var outputLabel = new Label
            {
                Text = "Output:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            var outputTextBox = new TextBox
            {
                Name = "outputTextBox",
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(3, 8, 3, 8)
            };
            var outputBrowseBtn = new Button
            {
                Text = "Save As",
                Name = "outputBrowseBtn",
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(3, 5, 3, 5)
            };
            outputBrowseBtn.Click += OutputBrowseBtn_Click;

            filePanel.Controls.Add(outputLabel, 0, 2);
            filePanel.Controls.Add(outputTextBox, 1, 2);
            filePanel.Controls.Add(outputBrowseBtn, 2, 2);

            mainPanel.Controls.Add(filePanel, 0, 2);
            mainPanel.SetColumnSpan(filePanel, 3);

            
            var optionsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 6,
                Margin = new Padding(0, 0, 0, 10)
            };

            optionsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            optionsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80)); 
            optionsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80)); 
            optionsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80)); 
            optionsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); 
            optionsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); 
            optionsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50)); 

            
            var qualityLabel = new Label
            {
                Text = "Quality:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            var qualityNumeric = new NumericUpDown
            {
                Name = "qualityNumeric",
                Dock = DockStyle.Fill,
                Minimum = 1,
                Maximum = 100,
                Value = 80,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(3, 8, 3, 8)
            };

            
            var resizeLabel = new Label
            {
                Text = "Resize:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            var resizeTextBox = new TextBox
            {
                Name = "resizeTextBox",
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(3, 8, 3, 8)
            };
            
            resizeTextBox.Text = "e.g., 800x600 (optional)";
            resizeTextBox.ForeColor = Color.Gray;
            resizeTextBox.Enter += (s, e) =>
            {
                if (resizeTextBox.Text == "e.g., 800x600 (optional)")
                {
                    resizeTextBox.Text = "";
                    resizeTextBox.ForeColor = Color.Black;
                }
            };
            resizeTextBox.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(resizeTextBox.Text))
                {
                    resizeTextBox.Text = "e.g., 800x600 (optional)";
                    resizeTextBox.ForeColor = Color.Gray;
                }
            };

            
            var stripMetadataCheckBox = new CheckBox
            {
                Name = "stripMetadataCheckBox",
                Text = "Strip Metadata",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(3, 8, 3, 8)
            };

            
            var spacerLabel = new Label { Dock = DockStyle.Fill };

            optionsPanel.Controls.Add(qualityLabel, 0, 0);
            optionsPanel.Controls.Add(qualityNumeric, 1, 0);
            optionsPanel.Controls.Add(resizeLabel, 2, 0);
            optionsPanel.Controls.Add(resizeTextBox, 3, 0);
            optionsPanel.Controls.Add(stripMetadataCheckBox, 4, 0);
            optionsPanel.Controls.Add(spacerLabel, 5, 0);

            mainPanel.Controls.Add(optionsPanel, 0, 3);
            mainPanel.SetColumnSpan(optionsPanel, 3);

            
            var convertBtn = new Button
            {
                Text = "Convert Images",
                Name = "convertBtn",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.DodgerBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(200, 50),
                Anchor = AnchorStyles.None
            };
            convertBtn.FlatAppearance.BorderSize = 0;
            convertBtn.Click += ConvertBtn_Click;

            mainPanel.Controls.Add(convertBtn, 1, 4);

            
            var logPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 10, 0, 0)
            };

            var progressBar = new ProgressBar
            {
                Name = "progressBar",
                Dock = DockStyle.Top,
                Height = 25,
                Style = ProgressBarStyle.Marquee,
                Visible = false,
                Margin = new Padding(0, 0, 0, 5)
            };

            var logTextBox = new RichTextBox
            {
                Name = "logTextBox",
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                Font = new Font("Consolas", 9),
                Text = "Ready to convert images...\n",
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            logPanel.Controls.Add(logTextBox);
            logPanel.Controls.Add(progressBar);

            mainPanel.Controls.Add(logPanel, 0, 5);
            mainPanel.SetColumnSpan(logPanel, 3);

            this.Controls.Add(mainPanel);
        }

        
        private Control FindControlByName(string name)
        {
            return FindControlRecursive(this, name);
        }

        private Control FindControlRecursive(Control parent, string name)
        {
            if (parent.Name == name)
                return parent;

            foreach (Control child in parent.Controls)
            {
                var result = FindControlRecursive(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }

        private RadioButton FindRadioButtonByTag(string tag)
        {
            return FindRadioButtonRecursive(this, tag);
        }

        private RadioButton FindRadioButtonRecursive(Control parent, string tag)
        {
            if (parent is RadioButton radio && radio.Tag != null && radio.Tag.ToString() == tag)
                return radio;

            foreach (Control child in parent.Controls)
            {
                var result = FindRadioButtonRecursive(child, tag);
                if (result != null)
                    return result;
            }
            return null;
        }

        
        private Task WaitForExitAsync(Process process)
        {
            var tcs = new TaskCompletionSource<object>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, e) => tcs.TrySetResult(null);
            if (process.HasExited)
                tcs.TrySetResult(null);
            return tcs.Task;
        }

        private void ModeRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton radio && radio.Checked)
            {
                var inputBrowseBtn = FindControlByName("inputBrowseBtn") as Button;
                var outputBrowseBtn = FindControlByName("outputBrowseBtn") as Button;

                if (radio.Tag != null && radio.Tag.ToString() == "single")
                {
                    inputBrowseBtn.Text = "Browse File";
                    outputBrowseBtn.Text = "Save As";
                }
                else
                {
                    inputBrowseBtn.Text = "Browse Folder";
                    outputBrowseBtn.Text = "Select Folder";
                }
            }
        }

        private void InputBrowseBtn_Click(object sender, EventArgs e)
        {
            var inputTextBox = FindControlByName("inputTextBox") as TextBox;
            var singleRadio = FindRadioButtonByTag("single");

            if (singleRadio != null && singleRadio.Checked)
            {
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.tiff;*.webp|All Files|*.*";
                    openFileDialog.Title = "Select Image File";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        inputTextBox.Text = openFileDialog.FileName;
                    }
                }
            }
            else
            {
                using (var folderBrowserDialog = new FolderBrowserDialog())
                {
                    folderBrowserDialog.Description = "Select Input Folder";

                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        inputTextBox.Text = folderBrowserDialog.SelectedPath;
                    }
                }
            }
        }

        private void OutputBrowseBtn_Click(object sender, EventArgs e)
        {
            var outputTextBox = FindControlByName("outputTextBox") as TextBox;
            var formatComboBox = FindControlByName("formatComboBox") as ComboBox;
            var singleRadio = FindRadioButtonByTag("single");

            if (singleRadio != null && singleRadio.Checked)
            {
                using (var saveFileDialog = new SaveFileDialog())
                {
                    
                    string selectedFormat = formatComboBox.SelectedItem?.ToString() ?? "png";

                    
                    string filter = "";
                    string defaultExt = "";

                    switch (selectedFormat.ToLower())
                    {
                        case "png":
                            filter = "PNG Files|*.png|All Files|*.*";
                            defaultExt = "png";
                            break;
                        case "jpg":
                        case "jpeg":
                            filter = "JPEG Files|*.jpg;*.jpeg|All Files|*.*";
                            defaultExt = "jpg";
                            break;
                        case "gif":
                            filter = "GIF Files|*.gif|All Files|*.*";
                            defaultExt = "gif";
                            break;
                        case "bmp":
                            filter = "BMP Files|*.bmp|All Files|*.*";
                            defaultExt = "bmp";
                            break;
                        case "tiff":
                            filter = "TIFF Files|*.tiff;*.tif|All Files|*.*";
                            defaultExt = "tiff";
                            break;
                        case "webp":
                            filter = "WebP Files|*.webp|All Files|*.*";
                            defaultExt = "webp";
                            break;
                        default:
                            filter = "All Files|*.*";
                            defaultExt = selectedFormat;
                            break;
                    }

                    saveFileDialog.Filter = filter;
                    saveFileDialog.DefaultExt = defaultExt;
                    saveFileDialog.Title = "Save Converted Image As";
                    saveFileDialog.AddExtension = true;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        outputTextBox.Text = saveFileDialog.FileName;
                    }
                }
            }
            else
            {
                using (var folderBrowserDialog = new FolderBrowserDialog())
                {
                    folderBrowserDialog.Description = "Select Output Folder";

                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        outputTextBox.Text = folderBrowserDialog.SelectedPath;
                    }
                }
            }
        }

        private async void ConvertBtn_Click(object sender, EventArgs e)
        {
            if (isProcessing) return;

            var inputTextBox = FindControlByName("inputTextBox") as TextBox;
            var outputTextBox = FindControlByName("outputTextBox") as TextBox;
            var formatComboBox = FindControlByName("formatComboBox") as ComboBox;
            var qualityNumeric = FindControlByName("qualityNumeric") as NumericUpDown;
            var resizeTextBox = FindControlByName("resizeTextBox") as TextBox;
            var stripMetadataCheckBox = FindControlByName("stripMetadataCheckBox") as CheckBox;
            var convertBtn = FindControlByName("convertBtn") as Button;
            var progressBar = FindControlByName("progressBar") as ProgressBar;
            var logTextBox = FindControlByName("logTextBox") as RichTextBox;

            
            if (string.IsNullOrWhiteSpace(inputTextBox.Text))
            {
                MessageBox.Show("Please select an input file or folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(outputTextBox.Text))
            {
                MessageBox.Show("Please select an output location.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!File.Exists(rustExecutablePath))
            {
                MessageBox.Show(string.Format("Rust executable not found at: {0}\nPlease ensure pxforge.exe is in the application directory.", rustExecutablePath),
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                isProcessing = true;
                convertBtn.Enabled = false;
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Marquee;

                LogMessage("Starting conversion...", Color.Yellow);

                var batchRadio = FindRadioButtonByTag("batch");

                
                var resizeValue = resizeTextBox.Text;
                if (resizeValue == "e.g., 800x600 (optional)")
                    resizeValue = "";

                await RunPixelForgeAsync(
                    inputTextBox.Text,
                    outputTextBox.Text,
                    formatComboBox.SelectedItem.ToString(),
                    (int)qualityNumeric.Value,
                    resizeValue,
                    stripMetadataCheckBox.Checked,
                    batchRadio != null && batchRadio.Checked,
                    logTextBox
                );

                LogMessage("Conversion completed successfully!", Color.LimeGreen);
                MessageBox.Show("Image conversion completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogMessage("Error: " + ex.Message, Color.Red);
                MessageBox.Show("An error occurred during conversion:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isProcessing = false;
                convertBtn.Enabled = true;
                progressBar.Visible = false;
            }
        }

        private void LogMessage(string message, Color color)
        {
            var logTextBox = FindControlByName("logTextBox") as RichTextBox;

            if (logTextBox.InvokeRequired)
            {
                logTextBox.Invoke(new Action(() => LogMessage(message, color)));
                return;
            }

            logTextBox.SelectionStart = logTextBox.TextLength;
            logTextBox.SelectionLength = 0;
            logTextBox.SelectionColor = color;
            logTextBox.AppendText(string.Format("[{0:HH:mm:ss}] {1}\n", DateTime.Now, message));
            logTextBox.SelectionColor = logTextBox.ForeColor;
            logTextBox.ScrollToCaret();
        }

        private async Task RunPixelForgeAsync(string input, string output, string format, int quality,
            string resize, bool stripMetadata, bool batch, RichTextBox logTextBox)
        {
            var arguments = string.Format("-i \"{0}\" -o \"{1}\" -f {2} -q {3}", input, output, format, quality);

            if (!string.IsNullOrWhiteSpace(resize))
                arguments += " -r " + resize;

            if (stripMetadata)
                arguments += " --strip-metadata";

            if (batch)
                arguments += " --batch";

            LogMessage("Running: " + rustExecutablePath + " " + arguments, Color.Cyan);

            var startInfo = new ProcessStartInfo
            {
                FileName = rustExecutablePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        LogMessage(e.Data, Color.White);
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        LogMessage("Error: " + e.Data, Color.Red);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await WaitForExitAsync(process);

                if (process.ExitCode != 0)
                    throw new Exception("Process exited with code " + process.ExitCode);
            }
        }
    }
}