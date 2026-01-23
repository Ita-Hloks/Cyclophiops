using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
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

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void Label1_Click(object sender, EventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GetRegeditFileName.Get();
            textBox1.Text = "B2已触发";
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}