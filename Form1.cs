using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace test2
{
    public partial class Form1 : Form
    {
        Nanariya.Gps _gps = null;

        public Form1()
        {
            InitializeComponent();

            _gps = new Nanariya.Gps();
            _gps.GpsDataReceive += new Nanariya.Gps.GpsEventHandler(gps_GpsDataReceive);

            String[] ports = _gps.GetPortList();
            
            foreach (String port in ports)
            {
                comboBox1.Items.Add(port);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(comboBox1.SelectedItem != null)
            {
                _gps.Port = comboBox1.SelectedItem.ToString(); 
            }
            _gps.Start();
        }

        delegate void GpsReadDelegate(Nanariya.GpsReadEventArgs data);
        void gps_GpsDataReceive(object sender, Nanariya.GpsReadEventArgs e)
        {
            try
            {
                this.Invoke(new GpsReadDelegate(GpsRead), new object[] { e });
            }
            catch (Exception)
            {
            }
        }

        private void GpsRead(Nanariya.GpsReadEventArgs data)
        {
            richTextBox1.AppendText("Time " + data.Time + " N " + data.Latitude + ", E " + data.Longitude + "\r\n");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _gps.Stop();
        }

    }
}
