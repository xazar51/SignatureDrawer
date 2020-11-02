using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SignatureDrawer
{
    public static class Utils
    {
        /// <summary>
        /// Returns a copy (a snapshot) of an object
        /// </summary>
        public static T DeepCopy<T>(this T @this)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, @this);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }


        public static double Sqr(this double x)
        {
            return x * x;
        }

        public static int Sqr(this int x)
        {
            return x * x;
        }

        public static string ExePath()
        {
            return Assembly.GetExecutingAssembly().Location;
        }

        public static bool Confirmed(this string msgText)
        {
            return MessageBox.Show(msgText, Path.GetFileNameWithoutExtension(ExePath()),
                       MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

    }


}
