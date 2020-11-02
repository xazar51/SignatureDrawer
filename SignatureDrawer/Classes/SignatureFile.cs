using System;
using System.IO;
using System.Linq;

namespace SignatureDrawer
{
    public struct SignatureFile
    {
        public string File { get; set; }
        public string SignerID { get; set; }
        public string SignatureID { get; set; }

        public SignatureFile(string file)
        {
            File = file;
            string name = file.Split('/').Last();//handle if file is in zip directory
            var parts = Path.GetFileNameWithoutExtension(name).Replace("U", "").Split('S');
            if (parts.Length != 2)
            {
                throw new InvalidOperationException("Invalid file format. All signature files should be in 'U__S__.txt' format");
            }
            SignerID = parts[0].PadLeft(2, '0');
            SignatureID = parts[1].PadLeft(2, '0');
        }
    }
}