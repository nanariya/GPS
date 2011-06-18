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
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Nanariya.Gps gps = new Nanariya.Gps();
            gps.GpsDataReceive += new Nanariya.Gps.GpsEventHandler(gps_GpsDataReceive);

            gps.Start();
            
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
            richTextBox1.AppendText("N " + data.Latitude + ", E " + data.Longitude + "\r\n");
        }

    }
}
