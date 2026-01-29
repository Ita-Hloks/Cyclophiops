using System;
using System.Drawing;
using System.Windows.Forms;
using Cyclophiops.Regedit;

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
            var resB1C = GetRegeditValue.Get();
            if (resB1C)
            {
                textBox1.ForeColor = Color.Green;
                textBox1.Text = "B1执行成功";
            }
            else
            {
                textBox1.ForeColor = Color.Red;
                textBox1.Text = "b1执行失败";
            }
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void Label1_Click(object sender, EventArgs e)
        {
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            var resB2C = GetUserSoftwareDetail.Get();
            if (resB2C)
            {
                textBox1.ForeColor = Color.Green;
                textBox1.Text = "B2执行成功";
            }
            else
            {
                textBox1.ForeColor = Color.Red;
                textBox1.Text = "B2执行失败";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }
}
