using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsSetPrice.Models;
using Timer = System.Windows.Forms.Timer;

namespace WinFormsSetPrice
{
    public partial class Form1 : Form
    {
        Timer timer1;
        Timer timer2;
        List<Models.agzsClass> listAgzs = new List<Models.agzsClass>();
        List<Models.shedulerSetPriceClass> sheduler = new List<shedulerSetPriceClass>();
        int timePeriod;

        public Form1()
        {
            ExtDataClass.webaddress = Properties.Settings.Default["webaddress"].ToString();
            ExtDataClass.user = Properties.Settings.Default["user"].ToString();
            ExtDataClass.pass = Properties.Settings.Default["pass"].ToString();
            timePeriod = (int)Properties.Settings.Default["time"];

            // читаем список планировщика
            var shedulerString = Properties.Settings.Default["sheduler"].ToString();
            try
            {
                sheduler = JsonConvert.DeserializeObject<List<shedulerSetPriceClass>>(shedulerString);
            }
            catch
            {
            }
            finally
            {
                if (sheduler == null)
                {
                    sheduler = new List<shedulerSetPriceClass>();
                    var contentsToWriteToSettings = JsonConvert.SerializeObject(sheduler);
                    Properties.Settings.Default["sheduler"] = contentsToWriteToSettings;
                    Properties.Settings.Default.Save();
                }
            }

            InitializeComponent();

            timer1 = new Timer();
            timer1.Tick += timer1_Tick;
            timer1.Interval = timePeriod;
            timer1.Start();

            timer2 = new Timer();
            timer2.Tick += timer2_Tick;
            timer2.Interval = 500;
            timer2.Start();
        }        

        private async void timer1_Tick(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[13].Value != null && !string.IsNullOrEmpty(row.Cells[13].Value.ToString()))
                {
                    if (TimeSpan.Parse(row.Cells[13].Value.ToString()) <= DateTime.Now.TimeOfDay)
                    {
                        decimal newPrice = 0;
                        if (string.IsNullOrEmpty(row.Cells[12].Value.ToString()))
                            newPrice = 0;
                        else
                            newPrice = decimal.Parse(row.Cells[12].Value.ToString());

                        if (decimal.Parse(row.Cells[7].Value.ToString()) != newPrice && newPrice>0)
                        {
                            try
                            {
                                bool res = await ExtDataClass.SetPrice(
                                    row.Cells[11].Value.ToString(),
                                    decimal.Parse(row.Cells[12].Value.ToString()) - decimal.Parse(row.Cells[7].Value.ToString())
                                );
                                if (res)
                                {
                                    //MessageBox.Show($"AGZS: {row.Cells[11].Value.ToString()} set price {row.Cells[12].Value.ToString()}");
                                    listAgzs = await ExtDataClass.GetAgzsAsync();
                                    dataGridView1.DataSource = listAgzs.OrderBy(x => x.agzsid).ToList();
                                }
                                else
                                {
                                    //MessageBox.Show($"AGZS: {row.Cells[11].Value.ToString()} NOT set price {row.Cells[12].Value.ToString()}");
                                }

                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                        }
                    }
                }
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            btn.Enabled = false;
            timer1.Stop();
            try
            {
                listAgzs = await ExtDataClass.GetAgzsAsync();
                dataGridView1.DataSource = listAgzs.OrderBy(x => x.agzsid).ToList();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            btn.Enabled = true;
            timer1.Start();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null) return;

            Button btn = (Button)sender;
            btn.Enabled = false;
            timer1.Stop();

            FormSetPrice formSetPrice = new FormSetPrice();
            formSetPrice.textBox1.Text = dataGridView1.CurrentRow.Cells[2].Value.ToString();
            formSetPrice.textBox2.Text = dataGridView1.CurrentRow.Cells[11].Value.ToString();
            formSetPrice.textBox3.Text = dataGridView1.CurrentRow.Cells[7].Value.ToString();
            formSetPrice.ShowDialog();

            try
            {
                listAgzs = await ExtDataClass.GetAgzsAsync();
                dataGridView1.DataSource = listAgzs.OrderBy(x => x.agzsid).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            btn.Enabled = true;
            timer1.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            FormSetting formSetting = new FormSetting();
            formSetting.textBox1.Text = ExtDataClass.webaddress;
            formSetting.textBox2.Text = ExtDataClass.user;
            formSetting.textBox3.Text = ExtDataClass.pass;
            formSetting.textBox4.Text = timePeriod.ToString();

            formSetting.StartPosition = FormStartPosition.CenterScreen;
            formSetting.ShowDialog();
            ExtDataClass.webaddress = Properties.Settings.Default["webaddress"].ToString();
            ExtDataClass.user = Properties.Settings.Default["user"].ToString();
            ExtDataClass.pass = Properties.Settings.Default["pass"].ToString();
            timePeriod = (int)Properties.Settings.Default["time"];
            timer1.Interval = timePeriod;

            ExtDataClass.token_expires_in = 0;
            timer1.Start();
        }

        private void AnyColumnKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != ',')
            {
                e.Handled = true;
            }

            // allow 1 dot:


            if ((e.KeyChar == ',') && ((sender as TextBox).Text.IndexOf(',') > -1))
            {
                e.Handled = true;
            }
        }

        private void AnyColumnKeyPress2(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != ':')
            {
                e.Handled = true;
            }

            // allow 1 dot:


            if ((e.KeyChar == ':') && ((sender as TextBox).Text.Where(x => x == ':').ToArray().Length > 1))
            {
                e.Handled = true;
            }
        }

        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress -= new KeyPressEventHandler(AnyColumnKeyPress);
            e.Control.KeyPress -= new KeyPressEventHandler(AnyColumnKeyPress2);
            if (dataGridView1.CurrentCell.ColumnIndex == 12) //Desired Column
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress += new KeyPressEventHandler(AnyColumnKeyPress);
                }
            }
            else if (dataGridView1.CurrentCell.ColumnIndex == 13)
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress += new KeyPressEventHandler(AnyColumnKeyPress2);
                }
            }
        }

        private void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            decimal newPrice = 0;
            TimeSpan newTime = TimeSpan.MinValue;

            if (dataGridView1.Rows[e.RowIndex].Cells[12].Value != null)
            {
                try
                {
                    newPrice = decimal.Parse(dataGridView1.Rows[e.RowIndex].Cells[12].Value.ToString());
                }
                catch { }
            }

            if (dataGridView1.Rows[e.RowIndex].Cells[13].Value != null)
            {
                try
                {
                    newTime = TimeSpan.Parse(dataGridView1.Rows[e.RowIndex].Cells[13].Value.ToString());
                }
                catch { }
            }

            if (!string.IsNullOrEmpty(e.FormattedValue?.ToString()))
            {
                if (e.ColumnIndex == 12) //Desired Column
                {
                    if (!decimal.TryParse(e.FormattedValue?.ToString().Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out newPrice))
                    {
                        e.Cancel = true;
                    }
                    if (!e.Cancel) dataGridView1.Rows[e.RowIndex].Cells[12].Value = newPrice;
                }
                else if (e.ColumnIndex == 13)
                {
                    if (!TimeSpan.TryParse(e.FormattedValue?.ToString(), CultureInfo.InvariantCulture, out newTime))
                    {
                        e.Cancel = true;
                    }
                    if (!e.Cancel) dataGridView1.Rows[e.RowIndex].Cells[13].Value = newTime;
                }

                if (!e.Cancel)
                {
                    var agzsId = dataGridView1.Rows[e.RowIndex].Cells[11].Value;
                    //var newPrice = dataGridView1.Rows[e.RowIndex].Cells[12].Value;
                    //var newTime = dataGridView1.Rows[e.RowIndex].Cells[13].Value;
                    ChangeSchedulerEntry(agzsId, newPrice, newTime);
                }
            }
        }

        private void ChangeSchedulerEntry(object agzsId, decimal newPrice, TimeSpan newTime)
        {
            var entry = sheduler.FirstOrDefault(x => x.agzsid == agzsId.ToString());
            var price = newPrice; // (newPrice == null ? 0 : decimal.Parse(newPrice.ToString()));
            var time = newTime; // (newTime == null ? TimeOnly.MinValue : TimeOnly.Parse(newTime.ToString()));

            if (entry == null)
            {
                sheduler.Add(new shedulerSetPriceClass()
                {
                    agzsid = agzsId.ToString(),
                    new_price = price,
                    scheduled_time = time
                });
            }
            else
            {
                entry.new_price = price;
                entry.scheduled_time = time;
            }

            var contentsToWriteToSettings = JsonConvert.SerializeObject(sheduler);
            Properties.Settings.Default["sheduler"] = contentsToWriteToSettings;
            Properties.Settings.Default.Save();
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells[11].Value == null) continue;

                    var sh = sheduler.FirstOrDefault(x => x.agzsid == row.Cells[11].Value.ToString());
                    if (sh != null)
                    {
                        if (sh.new_price != 0 && sh.scheduled_time.Ticks > 0)
                        {
                            row.Cells[12].Value = sh.new_price;
                            row.Cells[13].Value = sh.scheduled_time;
                        }
                    }

                    //if (row.Cells[13].Value != null)
                    //{
                    //    if ((decimal)row.Cells[7].Value == (decimal)row.Cells[12].Value)
                    //    {
                    //        row.DefaultCellStyle.BackColor = Color.LightGreen;
                    //    }
                    //    else
                    //    {
                    //        row.DefaultCellStyle.BackColor = Color.OrangeRed;
                    //    }
                    //}

                }
            }
            catch { }
        }

        private void dataGridView1_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[13].Value != null)
                {
                    decimal.TryParse(row.Cells[7].Value?.ToString(), out decimal oldPrice);
                    decimal.TryParse(row.Cells[12].Value?.ToString(), out decimal newPrice);
                    if (newPrice != 0)
                    {
                        if (oldPrice == newPrice)
                        {
                            row.DefaultCellStyle.BackColor = Color.LightGreen;
                        }
                        else
                        {
                            row.DefaultCellStyle.BackColor = Color.OrangeRed;
                        }
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.White;
                    }
                }
            }
        }

        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            timer1.Stop();
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 12 || e.ColumnIndex == 13)
            {
                if(string.IsNullOrEmpty(((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString()))
                {
                    //remove record
                    var agzsid = ((DataGridView)sender).Rows[e.RowIndex].Cells[11].Value.ToString();
                    var rec = sheduler.FirstOrDefault(x => x.agzsid == agzsid);
                    sheduler.Remove(rec);
                    
                    //Save settings
                    var contentsToWriteToSettings = JsonConvert.SerializeObject(sheduler);
                    Properties.Settings.Default["sheduler"] = contentsToWriteToSettings;
                    Properties.Settings.Default.Save();
                }
            }
            timer1.Start();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (!timer1.Enabled)
            {
                textBox1.Visible = true;
                textBox1.BackColor = textBox1.BackColor == Color.Red ? Color.Green : Color.Red;
            }
            else
            {
                textBox1.Visible = false;
            }
        }
    }
}
