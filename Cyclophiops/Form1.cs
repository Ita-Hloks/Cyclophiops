using System;
using System.Drawing;
using System.Windows.Forms;
using Cyclophiops.Detail.Browser;
using Cyclophiops.Regedit;
using Cyclophiops.WMI;

namespace Cyclophiops
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Display_Info(GetRegeditValue.Get(), "B1");
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void Label1_Click(object sender, EventArgs e)
        {
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Display_Info(GetUserSoftwareDetail.Get(), "B2");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            Display_Info(GetDeviceInfo.Export(string.Empty), "B3");
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            Display_Info(GetHistory.Get(), "B4");
        }

        private void Display_Info(bool result, string text)
        {
            if (result)
            {
                textBox1.ForeColor = Color.Green;
                textBox1.Text = $"{text}执行成功";
            }
            else
            {
                textBox1.ForeColor = Color.Red;
                textBox1.Text = $"{text}执行失败";
            }
        }
    }
}
