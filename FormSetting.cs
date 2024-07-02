using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsSetPrice
{
    public partial class FormSetting : Form
    {
        public FormSetting()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default["webaddress"] = textBox1.Text;
            Properties.Settings.Default["user"] = textBox2.Text;
            Properties.Settings.Default["pass"] = textBox3.Text;
            Properties.Settings.Default["time"] = int.Parse(textBox4.Text);

            Properties.Settings.Default.Save();
            Close();
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}
