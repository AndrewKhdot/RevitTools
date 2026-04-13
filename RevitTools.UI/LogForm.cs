using System;
using System.Diagnostics;
using System.Windows.Forms;
using RevitTools.Core.Services;

namespace RevitTools.UI
{
    public class LogForm : Form
    {
        private TextBox _textBox;
        private Button _btnClear;
        private Button _btnOpenFolder;

        public LogForm()
        {
            Text = "RevitTools Log";
            Width = 800;
            Height = 500;

            // Панель кнопок
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40
            };

            _btnClear = new Button
            {
                Text = "Clear",
                Left = 10,
                Top = 8,
                Width = 100
            };
            _btnClear.Click += (s, e) => LoggingService.Clear();

            _btnOpenFolder = new Button
            {
                Text = "Open folder",
                Left = 120,
                Top = 8,
                Width = 120
            };
            _btnOpenFolder.Click += (s, e) => OpenLogFolder();

            panel.Controls.Add(_btnClear);
            panel.Controls.Add(_btnOpenFolder);

            // TextBox логов
            _textBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill
            };

            Controls.Add(_textBox);
            Controls.Add(panel);

            // ✅ Подписки
            LoggingService.OnLog += WriteLine;
            LoggingService.OnClear += ClearView;
        }

        private void WriteLine(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => WriteLine(text)));
                return;
            }

            _textBox.AppendText(text + Environment.NewLine);
        }

        private void ClearView()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ClearView));
                return;
            }

            _textBox.Clear();
        }

        private void OpenLogFolder()
        {
            string dir = LoggingService.GetLogDirectory();

            Process.Start(new ProcessStartInfo
            {
                FileName = dir,
                UseShellExecute = true
            });
        }
    }
}
