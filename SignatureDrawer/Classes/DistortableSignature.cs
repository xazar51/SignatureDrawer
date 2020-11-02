using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SignatureDrawer.Classes
{

    /// <summary>
    /// The descendant of Signature class with support for distortion
    /// </summary>
    [Serializable]
    public class DistortableSignature : Signature
    {

        //the greater the factor the bigger masses will be generated
        public static double MassFactor = 3;

        //Max number of mass centers
        public static int DefaultMassDensity = 10;
        public static int MassDensity = DefaultMassDensity;

        public static bool RandomizeMassDensity = true;

        
        /// <summary>
        /// stores original file name which is used for saving the distorted variation (a sequential number is added to the original name)
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The 1st line of the original file
        /// </summary>
        public string Header { get; set; }


        public DistortableSignature(string fileName, string header, string name, string index) : base(name, index)
        {
            FileName = fileName;
            Header = header;
        }

        private Random _rnd = new Random();
        

        #region Dimensions
        private bool _dimensionsCalculated;

        private void CalculateDimensions()
        {
            _maxX = Points.Max(p => p.X);
            _maxY = Points.Max(p => p.Y);
            _minX = Points.Min(p => p.X);
            _minY = Points.Min(p => p.Y);
            _dimensionsCalculated = true;
        }

        
        private double _maxX;
        private double _maxY;
        private double _minX;
        private double _minY;
        private List<GravityCenter> _gravityCenters;

        public double Width => MaxX - MinX;
        public double Height => MaxY - MinY;

        public double MaxX
        {
            get
            {
                if (!_dimensionsCalculated) CalculateDimensions();
                return _maxX;
            }
        }

        public double MaxY
        {
            get
            {
                if (!_dimensionsCalculated) CalculateDimensions();
                return _maxY;
            }
        }

        public double MinX
        {
            get
            {
                if (!_dimensionsCalculated) CalculateDimensions();
                return _minX;
            }
        }

        public double MinY
        {
            get
            {
                if (!_dimensionsCalculated) CalculateDimensions();
                return _minY;
            }
        }
        #endregion Dimensions


        #region Distortions

        public static DistortMode DistortMode { get; set; } = DistortMode.Gravity;


        
        private List<SignaturePoint> _distortedPoints;

        /// <summary>
        /// The result of distortion
        /// </summary>
        public List<SignaturePoint> DistortedPoints
        {
            get
            {
                if (_distortedPoints == null)
                {
                    Distort();
                }

                return _distortedPoints;
            }
            set => _distortedPoints = value;
        }

        
        /// <summary>
        /// Gravity and antigravity points which attract or repel wnything that comes near them
        /// </summary>
        public List<GravityCenter> GravityCenters
        {
            get
            {
                if (_gravityCenters == null) _gravityCenters = new List<GravityCenter>();
                return _gravityCenters;
            }
            set => _gravityCenters = value;
        }


        public void Reset()
        {
            GravityCenters.Clear();
            _distortedPoints = null;
        }

        /// <summary>
        /// Generates a few random gravity points
        /// </summary>
        public void GenerateGravityCenters()
        {
            Reset();

            var size = (int) (Width * MassFactor);

            var density = MassDensity;
            if (RandomizeMassDensity) density = _rnd.Next(MassDensity);

            for (var i = 0; i < density; i++)
            {
                var center = new GravityCenter { X = MinX + _rnd.Next((int)Width), Y = MinY + _rnd.Next((int)Height), Mass = _rnd.Next(size) - size / 2 };
                GravityCenters.Add(center);
            }
        }


        public Point GravityShift(Point p)
        {
            return GravityShift(p.X, p.Y);
        }


        /// <summary>
        /// Shifts a given point by combining forces of interaction with every existing gravity center
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Point GravityShift(double x, double y)
        {

            var result = new Point(x, y);

            foreach (var gravityCenter in GravityCenters)
            {
                var shift = gravityCenter.Shift(x, y);
                result.X += shift.X;
                result.Y += shift.Y;
            }

            return result;
        }

        /// <summary>
        /// Performs the distortion procedure upon Points array. The result is stored in DistortedPoints
        /// </summary>
        public void Distort()
        {
            
            switch (DistortMode)
            {
                case DistortMode.Gravity:
                    //gravity mode
                    GravityCenters = GravityCenters;//causes to be created if null
                    if (GravityCenters.Count == 0) GenerateGravityCenters();
                    _distortedPoints = new List<SignaturePoint>(); //generatecenters nulls this
                    foreach (var p in Points)
                    {
                        //shifting x and y coordinates
                        var xy = GravityShift(p.X, p.Y);
                        var p1 = new SignaturePoint((int) xy.X, (int) xy.Y);

                        
                        //determining how much we shifted
                        var xcoef = xy.X / p.X;
                        var ycoef = xy.Y / p.Y;

                        //applying same coefficients to "invisible" params
                        //there must be something better we could do. Perhaps treat these parameters as multi-dimenstional coordinates

                        p1.Altitude = (int) (p.Altitude * xcoef);
                        p1.Azimuth = (int) (p.Azimuth * ycoef);
                        p1.Pressure = (int) (p.Pressure * xcoef);
                        p1.Time = (int) (p.Time * ycoef);
                        p1.ButtonStatus = p.ButtonStatus;
                        
                        _distortedPoints.Add(p1);
                    }
                    break;
                case DistortMode.Randomize:
                    //"shaking hands" mode
                    _distortedPoints = new List<SignaturePoint>();
                    foreach (var p in Points)
                    {
                        int rx = (int)(_rnd.Next((int)((p.X - MinX) * .1)) - (p.X - MinX) * .05);
                        int ry = (int)(_rnd.Next((int)((p.Y - MinY) * .1)) - (p.Y - MinY) * .05);
                        var p1 = new SignaturePoint(p.X + rx, p.Y + ry);
                        _distortedPoints.Add(p1);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(DistortMode), DistortMode, null);
            }
            
        }

        #endregion  Distortions


        

        /// <summary>
        /// Saves the distorted signature to a file. File name is based on the orginal file plus a number
        /// </summary>
        /// <param name="fileNumber"></param>
        public void SaveDistorted(int fileNumber)
        {
            DistortedPoints = DistortedPoints;//causes to distort if not

            var prefix = DistortMode == DistortMode.Gravity ? "9" : "8";

            var newFileName =
                Path.Combine(
                    Path.GetDirectoryName(FileName),
                    Path.GetFileNameWithoutExtension(FileName) + prefix + fileNumber.ToString("D4") + 
                    Path.GetExtension(FileName)
                );


            var lines = DistortedPoints.Select(dp => dp.X + " " + dp.Y + " " + dp.Time + " " + dp.ButtonStatus + " " + dp.Azimuth + " " + dp.Altitude + " " + dp.Pressure).ToList();
            lines.Insert(0, Header);
            File.WriteAllLines(newFileName, lines);

            
        }
    }


    /// <summary>
    /// Supported distortion modes
    /// </summary>
    public enum DistortMode
    {
        Gravity, Randomize
    }

}
