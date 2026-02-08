using System;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cyclophiops.Detail.Browser;
using Cyclophiops.Regedit;
using Cyclophiops.WMI;

namespace Cyclophiops
{
    public partial class Form1 : Form
    {
        private readonly StringBuilder _logBuilder = new StringBuilder();

        public Form1()
        {
            InitializeComponent();
        }

        private async void Button1_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                await ExecuteAllTasksAsync();
            }
            catch (Exception ex)
            {
                AppendLog($"[FATAL ERROR] {ex.Message}", Color.Red);
                SetStatus("Fatal error occurred", Color.Red);
                EnableAllButtons();
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            ExecuteTask("Basic Information", () => GetRegeditValue.Get());
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            ExecuteTask("Installed Software", () => GetUserSoftwareDetail.Get());
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AppendLog("System information collector started");
            AppendLog("Select a task to execute...");
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            ExecuteTask("Hardware Information", () => GetDeviceInfo.Export(string.Empty));
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            ExecuteTask("Browser History", () => GetHistory.Get());
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            ExecuteTask("Software Residuals", () => GetSoftwareResiduals.Get());
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            ExecuteTask("Browser Bookmarks", () => GetBookmarks.Get());
        }

        private async Task ExecuteAllTasksAsync()
        {
            AppendLog("========== Starting All Tasks ==========");
            SetStatus("Running all tasks...", Color.Blue);
            DisableAllButtons();

            var tasks = new[]
            {
                new { Name = "Basic Information", Action = (Func<bool>)(() => GetRegeditValue.Get()) },
                new { Name = "Installed Software", Action = (Func<bool>)(() => GetUserSoftwareDetail.Get()) },
                new { Name = "Hardware Information", Action = (Func<bool>)(() => GetDeviceInfo.Export(string.Empty)) },
                new { Name = "Browser History", Action = (Func<bool>)(() => GetHistory.Get()) },
                new { Name = "Software Residuals", Action = (Func<bool>)(() => GetSoftwareResiduals.Get()) },
                new { Name = "BookMarks", Action = (Func<bool>)(() => GetBookmarks.Get()) },
            };

            var successCount = 0;
            var failCount = 0;

            foreach (var task in tasks)
            {
                AppendLog($"Executing: {task.Name}...");

                var result = await Task.Run(() =>
                {
                    try
                    {
                        return task.Action();
                    }
                    catch (Exception ex)
                    {
                        AppendLog($"[ERROR] {task.Name} - {ex.Message}", Color.Red);
                        return false;
                    }
                });

                if (result)
                {
                    successCount++;
                    AppendLog($"[OK] {task.Name} - Completed", Color.Green);
                }
                else
                {
                    failCount++;
                    AppendLog($"[FAIL] {task.Name} - Failed", Color.Red);
                }
            }

            AppendLog($"========== All Tasks Completed ==========");
            AppendLog($"Success: {successCount} | Failed: {failCount}");

            if (failCount == 0)
            {
                SetStatus($"All tasks completed successfully ({successCount}/{tasks.Length})", Color.Green);
            }
            else
            {
                SetStatus($"Tasks completed with failures ({successCount}/{tasks.Length})", Color.Orange);
            }

            EnableAllButtons();
        }

        private void ExecuteTask(string taskName, Func<bool> action)
        {
            AppendLog($"Executing: {taskName}...");
            SetStatus($"Executing: {taskName}...", Color.Blue);

            try
            {
                var result = action();
                if (result)
                {
                    AppendLog($"[OK] {taskName} - Completed", Color.Green);
                    SetStatus($"{taskName} completed successfully", Color.Green);
                }
                else
                {
                    AppendLog($"[FAIL] {taskName} - Failed", Color.Red);
                    SetStatus($"{taskName} execution failed", Color.Red);
                }
            }
            catch (Exception ex)
            {
                AppendLog($"[ERROR] {taskName} - {ex.Message}", Color.Red);
                SetStatus($"{taskName} execution error", Color.Red);
            }
        }

        private void AppendLog(string message, Color? color = null)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var logMessage = $"[{timestamp}] {message}";

            _logBuilder.AppendLine(logMessage);

            if (logTextBox.InvokeRequired)
            {
                logTextBox.Invoke(new Action(() =>
                {
                    logTextBox.AppendText(logMessage + Environment.NewLine);
                    logTextBox.SelectionStart = logTextBox.Text.Length;
                    logTextBox.ScrollToCaret();
                }));
            }
            else
            {
                logTextBox.AppendText(logMessage + Environment.NewLine);
                logTextBox.SelectionStart = logTextBox.Text.Length;
                logTextBox.ScrollToCaret();
            }
        }

        private void SetStatus(string message, Color color)
        {
            if (statusLabel.InvokeRequired)
            {
                statusLabel.Invoke(new Action(() =>
                {
                    statusLabel.Text = message;
                    statusLabel.ForeColor = color;
                }));
            }
            else
            {
                statusLabel.Text = message;
                statusLabel.ForeColor = color;
            }
        }

        private void DisableAllButtons()
        {
            button1.Enabled = true;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
        }

        private void EnableAllButtons()
        {
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            button7.Enabled = true;
        }

        private void PathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var f = new SettingsForm())
            {
                f.ShowDialog();
            }
        }
    }
}
