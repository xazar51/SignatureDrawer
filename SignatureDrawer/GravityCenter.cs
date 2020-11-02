using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SignatureDrawer
{
    public class GravityCenter
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Mass { get; set; }

        
        public Point Shift(double x, double y)
        {

            try
            {
                var rSquared = Math.Sqrt((x - X).Sqr() + (y - Y).Sqr());
                var r = Math.Sqrt(rSquared);
                var force = Mass / rSquared;  // F = m * M / r^2;      m=1

                var rshift = force; 

                if (rshift > r) rshift = r; //we don't want our "planets" to go beyound our "sun"

                var cos = Math.Abs(x - X) / r;
                var sin = Math.Abs(y - Y) / r;

                var xshift = rshift * cos;
                var yshift = rshift * sin;

                if (x > X) xshift = -xshift;
                if (y > Y) yshift = -yshift;

                return new Point(xshift, yshift);

            }
            catch (Exception e)
            {
                return new Point(0, 0);
            }

            

        }

    }
}
