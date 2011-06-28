using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;

namespace Nanariya
{
    class GpsEmu
    {
        #region イベントハンドラの定義
        //イベントハンドラの定義
        public delegate void GpsEmuEventHandler(object sender, GpsEmuReadEventArgs e);
        //イベントハンドラの名前
        public event GpsEmuEventHandler GpsEmuDataReceive;
        /// <summary>
        /// イベントハンドラを発生させる為に呼ぶメソッド
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnGpsEmuDataReceive(GpsEmuReadEventArgs e)
        {
            GpsEmuDataReceive(this, e);
        }
        #endregion

        private System.Windows.Forms.Timer _timer = null;

        public String Port { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public Parity Parity { get; set; }
        public StopBits StopBits { get; set; }
        public Handshake Handshake { get; set; }

        private Double Ido { get; set; }
        private Double Keido { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GpsEmu()
        {
            //デフォルト
            this.Port = "COM4";
            this.BaudRate = 4800;   //PGM-111はボーレート4800がデフォ
            this.DataBits = 8;
            this.Parity = Parity.None;
            this.StopBits = StopBits.One;
            this.Handshake = Handshake.None;
        }

        public void Start()
        {
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 3000;
            _timer.Tick += new EventHandler(_serialPort_DataReceived);

            /*
            Random rnd = new Random();

            int r_ido = rnd.Next(250797); //経度 日本の幅
            int r_keido = rnd.Next(310310); //緯度　日本の高さ

            r_ido += 202531;    //日本の最南端
            r_keido += 1225601; //日本の最西端

            double ido = double.Parse(r_ido.ToString()) / 10000;
            double keido = double.Parse(r_keido.ToString()) / 10000;

            this.Ido = ido;
            this.Keido = keido;
            */

            this.Ido = 35.7004;
            this.Keido = 139.771;

            _timer.Start();
        }
        public void Stop()
        {
            _timer.Stop();
            _timer.Dispose();
        }

        public String[] GetPortList()
        {
            String[] ports = SerialPort.GetPortNames();
            if (ports == null)
            {
                new Exception("Serialポートがないよエラー");
            }
            return ports;
        }
        /// <summary>
        /// SerialPortのイベント受け取りに見せかけたTimerのTick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _serialPort_DataReceived(object sender, EventArgs e)
        {
            try
            {
                Random rnd = new Random();
                int deg = rnd.Next(359);
                double radian = deg * 3.141592 / 180.0;
                double sin = Math.Sin(radian);
                double cos = Math.Cos(radian);

                this.Ido += (sin / 1000);
                this.Keido += (cos / 1000);
                
                Dictionary<String, String> eventData = new Dictionary<string, string>();
                eventData.Add("Enable", "true");
                eventData.Add("Time", "0");
                eventData.Add("Latitude", this.Ido.ToString());
                eventData.Add("NS", "N");
                eventData.Add("Longitude", this.Keido.ToString());
                eventData.Add("EW", "E");
                eventData.Add("Quality", "test");
                eventData.Add("Satellites", "test");
                eventData.Add("GeoidHeight", "test");;

                GpsEmuReadEventArgs gpsEvent = new GpsEmuReadEventArgs(eventData);
                OnGpsEmuDataReceive(gpsEvent);
            }
            catch (Exception)
            {
            }
        }
    }
    #region イベントハンドラのデータ受け渡し
    /// <summary>
    /// イベントハンドラでデータを受け渡す為のクラス
    /// </summary>
    class GpsEmuReadEventArgs : EventArgs
    {
        private readonly Dictionary<String, String> _args = null;

        public GpsEmuReadEventArgs(Dictionary<String, String> args)
        {
            this._args = args;
        }

        public Dictionary<String, String> args
        {
            get
            {
                return _args;
            }
        }

        public bool Enable { get { return System.Convert.ToBoolean(_args["Enable"]); } }
        public String Time { get { return _args["Time"]; } }
        public String NS { get { return _args["NS"]; } }
        public String Latitude { get { return _args["Latitude"]; } }
        public String EW { get { return _args["EW"]; } }
        public String Longitude { get { return _args["Longitude"]; } }
        public String Quality { get { return _args["Quality"]; } }
        public String Satellites { get { return _args["Satellites"]; } }
        public String GeoidHeight { get { return _args["GeoidHeight"]; } }

    }
    #endregion
}
