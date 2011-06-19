using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO.Ports;

namespace Nanariya
{
    class Gps
    {
        #region イベントハンドラの定義
        //イベントハンドラの定義
        public delegate void GpsEventHandler(object sender, GpsReadEventArgs e);
        //イベントハンドラの名前
        public event GpsEventHandler GpsDataReceive;
        /// <summary>
        /// イベントハンドラを発生させる為に呼ぶメソッド
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnGpsDataReceive(GpsReadEventArgs e)
        {
            GpsDataReceive(this, e);
        }
        #endregion

        SerialPort _serialPort = null;

        public String Port { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public Parity Parity { get; set; }
        public StopBits StopBits { get; set; }
        public Handshake Handshake { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Gps()
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
            _serialPort = new SerialPort(this.Port, this.BaudRate, this.Parity, this.DataBits, this.StopBits);
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);
            //ハンドシェイクだけ後から設定
            _serialPort.Handshake = this.Handshake;

            _serialPort.Open();
        }

        public void Stop()
        {
            _serialPort.Close();
            _serialPort.Dispose();
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
        /// SerialPortのイベント受け取り
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Dictionary<String, String> eventData = new Dictionary<string, string>();

                //生データを加工
                eventData = ParseGpsPos(_serialPort.ReadLine());

                //中身あるときだけイベント発生させる（仮
                if (eventData.Count > 0)
                {
                    GpsReadEventArgs gpsEvent = new GpsReadEventArgs(eventData);
                    OnGpsDataReceive(gpsEvent);
                }
            }
            catch(Exception)
            {
            }
        }

        /// <summary>
        /// GPGGAを抜き出してフォーマット調整
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        private Dictionary<String, String> ParseGpsPos(String rawData)
        {
            Dictionary<String, String> temp = new Dictionary<string, string>();

            string[] rawLines = rawData.Split('\n');
            foreach (string line in rawLines)
            {
                Regex regex = new Regex("^\\$GPGGA,\\d+,\\d+.\\d+,\\w,\\d+.\\d+,\\w,1");
                if (regex.IsMatch(line))
                {
                    //0:$GPGGA
                    //1:UTC
                    //2:Latitude
                    //3:N or S
                    //4:Longitude
                    //5:E or W
                    //6:Dis Enable
                    //7:キャッチしてる衛星の数
                    //8:?
                    //9:高度
                    //10:M
                    //11:ジオイド高
                    string[] cols = line.Split(',');
                    temp.Add("Enable", "true");
                    temp.Add("Time", cols[1]);
                    temp.Add("Latitude", Convert60to10(cols[2]));
                    temp.Add("NS", cols[3]);
                    temp.Add("Longitude", Convert60to10(cols[4]));
                    temp.Add("EW", cols[5]);
                    temp.Add("Quality", cols[6]);
                    temp.Add("Satellites", cols[7]);
                    temp.Add("GeoidHeight", cols[11]);

                }
                Regex gpsDisable = new Regex("^\\$GPGGA,\\d+,\\d+.\\d+,\\w,\\d+.\\d+,\\w,0");
                if (gpsDisable.IsMatch(line))
                {
                    temp.Add("Enable", "false");
                }
            }

            return temp;
            
        }

        /// <summary>
        /// GPS位置情報を60進数から10進数へ変換
        /// </summary>
        /// <param name="gpsPos"></param>
        /// <returns></returns>
        private String Convert60to10(String gpsPos)
        {
            //13599.12345 みたいになってる → 135と99.12345に分けて99.12345を60で割る
            int searchPos = gpsPos.IndexOf('.');
            if (searchPos > 0)
            {
                //小数点以下保持のためにdecimalでやってみた
                decimal degree;
                decimal.TryParse(gpsPos.Substring(0, searchPos - 2), out degree);
                decimal minutes;
                decimal.TryParse(gpsPos.Substring(searchPos - 2), out minutes);
                decimal minutes10 = minutes / 60;
                gpsPos = (degree + minutes10).ToString();
            }
            return gpsPos;
        }
        
    }


    #region イベントハンドラのデータ受け渡し
    /// <summary>
    /// イベントハンドラでデータを受け渡す為のクラス
    /// </summary>
    class GpsReadEventArgs : EventArgs
    {
        private readonly Dictionary<String, String> _args = null;

        public GpsReadEventArgs(Dictionary<String, String> args)
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
