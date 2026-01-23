using System;
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
            GetRegeditValue.Get();
            textBox1.Text = "B1已触发";
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void Label1_Click(object sender, EventArgs e)
        {
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            GetRegeditFileName.Get();
            textBox1.Text = "B2已触发";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }
}