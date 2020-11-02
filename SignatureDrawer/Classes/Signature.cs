using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SignatureDrawer
{

    [Serializable]
    public class SignaturePoint
    {
        
        public int X { get; set; }
        public int Y { get; set; }

        public int Time { get; set; }
        public int Azimuth { get; set; }
        public int Altitude { get; set; }
        public int Pressure { get; set; }

        public int ButtonStatus { get; set; }

        public SignaturePoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public SignaturePoint(int x, int y, int time, int buttonStatus, int azimuth, int altitude, int pressure)
        {
            X = x;
            Y = y;
            Time = time;
            ButtonStatus = buttonStatus;
            Azimuth = azimuth;
            Altitude = altitude;
            Pressure = pressure;
        }
    }

    [Serializable]
    public class Signature
    {
        
        public List<SignaturePoint> Points { get; set; }
        public List<int> Xs { get; set; }
        public List<int> Ys { get; set; }
        public string UserID { get; set; }
        public string SignatureID { get; set; }

        public void AddPoint( int x, int y)
        {
            Points.Add(new SignaturePoint(x, y));
            Xs.Add(x);
            Ys.Add(y);
        }

        public void AddPoint(int x, int y, int time, int buttonStatus, int azimuth, int altitude, int pressure)
        {
            Points.Add(new SignaturePoint(x, y, time, buttonStatus, azimuth, altitude, pressure));
        }
        public Signature()
        {
            Points = new List<SignaturePoint>();
            UserID = "N/D";
            SignatureID = "N/D";
        }
        public Signature(string name, string index)
        {
            Points = new List<SignaturePoint>();
            this.UserID = name;
            this.SignatureID = index;
        }
    }
}
