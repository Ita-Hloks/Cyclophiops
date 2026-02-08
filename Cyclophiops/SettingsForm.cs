using System;
using System.Windows.Forms;
using Cyclophiops.Config;

namespace Cyclophiops
{
    public partial class SettingsForm : Form
    {
        private readonly string _defaultPath = AppDomain.CurrentDomain.BaseDirectory;

        public SettingsForm()
        {
            InitializeComponent();
            if (Owner != null)
            {
                Icon = Owner.Icon;
            }
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            var exportPath = ExportConfig.GetExportPath();

            if (exportPath == _defaultPath)
            {
                radioDefaultPath.Checked = true;
                txtCustomPath.Enabled = false;
                btnBrowse.Enabled = false;
            }
            else
            {
                radioCustomPath.Checked = true;
                txtCustomPath.Text = exportPath;
                txtCustomPath.Enabled = true;
                btnBrowse.Enabled = true;
            }
        }

        private void RadioCustomPath_CheckedChanged(object sender, EventArgs e)
        {
            txtCustomPath.Enabled = radioCustomPath.Checked;
            btnBrowse.Enabled = radioCustomPath.Checked;
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "选择导出文件保存路径";
                folderDialog.ShowNewFolderButton = true;

                if (!string.IsNullOrEmpty(txtCustomPath.Text))
                {
                    folderDialog.SelectedPath = txtCustomPath.Text;
                }

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtCustomPath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string pathToSave;

                if (radioDefaultPath.Checked)
                {
                    pathToSave = _defaultPath;
                }
                else
                {
                    pathToSave = txtCustomPath.Text;

                    if (string.IsNullOrWhiteSpace(pathToSave))
                    {
                        MessageBox.Show("请选择或输入有效的路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (!System.IO.Directory.Exists(pathToSave))
                    {
                        MessageBox.Show("所选路径不存在，请重新选择", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                ExportConfig.SetExportPath(pathToSave);
                MessageBox.Show("设置已保存", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存设置失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
