using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SignatureDrawer.Classes
{
    /// <summary>
    /// Class that encapsulates drawing logic for DistortableSignature. It can draw either Points or DistortedPoints depending on the mode,
    /// it also draws gravity centers and a mesh
    /// </summary>
    public class Drawer
    {

        /// <summary>
        /// This is the signature we'll be drawing
        /// </summary>
        public DistortableSignature Signature { get; set; }
        
        /// <summary>
        /// The canvas to draw upon
        /// </summary>
        public Canvas Canvas { get; set; }

        public Drawer(DistortableSignature signature, Canvas canvas)
        {
            Signature = signature;
            Canvas = canvas;
        }
        
        private void DrawCicrle(double x, double y, double size, Brush color)
        {
            var el = new Ellipse();
            el.Width = size;
            el.Height = size;
            el.Fill = color;
            Canvas.Children.Add(el);
            el.SetValue(Canvas.LeftProperty, x - size / 2);
            el.SetValue(Canvas.TopProperty, y - size / 2);
        }

        private double CanvasHeight => Canvas.ActualHeight;
        private double CanvasWidth => Canvas.ActualWidth;

        

        
        private void DrawGravityCenters()
        {
            
            foreach (var center in Signature.GravityCenters)
            {
                var diameter =  Math.Round(20 * Math.Abs(center.Mass / Signature.Width));

                DrawCicrle(TransformX(center.X), TransformY(center.Y), diameter, center.Mass > 0 ? Brushes.Blue : Brushes.Red);
            }

        }


        private void DrawMesh()
        {
            
            var scaledGap = 20; //screen coordinates

            var gap = scaledGap * _coef; //"real world" coordinates


            var xCount = (Signature.Width) / gap;
            var yCount = (Signature.Height) / gap;

            //MessageBox.Show(gap.ToString() + " " + xCount + " " + yCount);



            //looping through every intersection point of the mesh and drawing 2 lines from it: down and right
            for (var i = 0; i < xCount; i++)
            {
                for (var j = 0; j < yCount; j++)
                {

                    //intersection point
                    var p1 = new Point(Signature.MinX + i * gap, Signature.MinY + j * gap);

                    //next mesh point downwards
                    var pDown = new Point(Signature.MinX + i * gap, Signature.MinY + (j + 1) * gap);

                    //next point to the right
                    var pRight = new Point(Signature.MinX + (i + 1) * gap, Signature.MinY + j * gap);
                    
                    //mesh obeys the gravity
                    if (_drawDistorted)
                    {
                        p1 = Signature.GravityShift(p1);
                        pDown = Signature.GravityShift(pDown);
                        pRight = Signature.GravityShift(pRight);
                    }


                    //transforming real world coordinates to screen coordinates after (if) gravity has been applied
                    p1 = TransformPoint(p1);
                    pDown = TransformPoint(pDown);
                    pRight = TransformPoint(pRight);
                    
                    var yLine = new Line
                    {
                        Stroke = Brushes.LightGreen,
                        StrokeThickness = .5,
                        X1 = p1.X,
                        Y1 = p1.Y,
                        X2 = pDown.X,
                        Y2 = pDown.Y
                    };

                    var xLine = new Line
                    {
                        Stroke = Brushes.LightGreen,
                        StrokeThickness = .5,
                        X1 = p1.X,
                        Y1 = p1.Y,
                        X2 = pRight.X,
                        Y2 = pRight.Y
                    };

                    Canvas.Children.Add(xLine);
                    Canvas.Children.Add(yLine);
                }
            }
        }

        
        //The ratio between screen and real world
        double _coef;

        private void CalcCoef()
        {
            _coef = Math.Max(Signature.Width / CanvasWidth, Signature.Height / CanvasHeight);
        }


        /// <summary>
        /// Transforms real world x into screen x
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private double TransformX(double x)
        {
            return (x - Signature.MinX) / _coef;
        }

        /// <summary>
        /// Transforms real world y into screen y (and flips it over)
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        private double TransformY(double y)
        {
            var result = (y - Signature.MinY) / _coef;
            return CanvasHeight - result;
        }

        
        private Point TransformPoint(Point p)
        {
            return new Point { X = TransformX(p.X), Y = TransformY(p.Y) };
        }

        private bool _drawDistorted;
        

        /// <summary>
        /// Draws the signature
        /// </summary>
        /// <param name="drawDistorted">Determines whether to draw original points or distorted points</param>
        public void Draw(bool drawDistorted)
        {
            
            _drawDistorted = drawDistorted;
            
            if(drawDistorted) Signature.Distort();
            
            Canvas.Children.Clear();

            CalcCoef();

            DrawGravityCenters();
            
            DrawMesh();
            
            var pointsToDraw = drawDistorted ? Signature.DistortedPoints : Signature.Points;

            var scaledPoints = new List<Point>(); //we don't want to mess with real data so we copy it to a new list while scaling it

            for (var i = 0; i < pointsToDraw.Count; i++)
            {
                var p = TransformPoint(new Point(pointsToDraw[i].X, pointsToDraw[i].Y));                
                scaledPoints.Add(p);
            }
            
            var lastPoint = scaledPoints[0];
            
            foreach (var point in scaledPoints)
            {
                //the points
                DrawCicrle(point.X, point.Y, 5, Brushes.DodgerBlue);

                //the lines connecting them
                var newLine = new Line();
                newLine.Stroke = Brushes.Black;
                newLine.StrokeThickness = 2;
                newLine.X1 = lastPoint.X;
                newLine.Y1 = lastPoint.Y;
                newLine.X2 = point.X;
                newLine.Y2 = point.Y;
                lastPoint = point;
                Canvas.Children.Add(newLine);
            }

        }
        
        

    }
}
