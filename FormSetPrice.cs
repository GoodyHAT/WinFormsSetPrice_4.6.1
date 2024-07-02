using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsSetPrice
{
    public partial class FormSetPrice : Form
    {
        public FormSetPrice()
        {
            InitializeComponent();
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var d = decimal.Parse(textBox4.Text.Replace(",", "."), CultureInfo.InvariantCulture) - decimal.Parse(textBox3.Text.Replace(",","."), CultureInfo.InvariantCulture);
            bool res = await ExtDataClass.SetPrice(textBox2.Text, d);
            Close();
        }
    }
}
