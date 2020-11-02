using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignatureDrawer.Classes
{
    /// <summary>
    /// Contains loaded signatures
    /// </summary>
    public static class SignatureRepo
    {
        
        public static readonly string SignaturePath = Path.Combine(Path.GetFullPath(@"..\..\"), @"Resources\Task2");

        public static readonly List<DistortableSignature> Signatures = new List<DistortableSignature>();

        /// <summary>
        /// List of users to bind to the combo box
        /// </summary>
        public static List<string> Users = new List<string>();


        /// <summary>
        /// Loads all the signatures from SignaturePath folder
        /// </summary>
        public static void Load()
        {
            var signFileList = new List<SignatureFile>();
            foreach (var filepath in Directory.GetFiles(SignaturePath))
            {
                signFileList.Add(new SignatureFile(filepath));
            }
            
            foreach (var signFile in signFileList)
            {
                var lines = File.ReadAllLines(signFile.File).ToList();

                var newSignature = new DistortableSignature(signFile.File, lines[0], signFile.SignerID, signFile.SignatureID);
                if (!Users.Contains(signFile.SignerID))
                {
                    Users.Add(signFile.SignerID);
                }

                lines.RemoveAt(0);
                foreach (var line in lines)
                {
                    string[] lineSplit = line.Split(' ');
                    newSignature.AddPoint(int.Parse(lineSplit[0]),
                        int.Parse(lineSplit[1]),
                        int.Parse(lineSplit[2]),
                        int.Parse(lineSplit[3]),
                        int.Parse(lineSplit[4]),
                        int.Parse(lineSplit[5]),
                        int.Parse(lineSplit[5])
                    );
                }
                Signatures.Add(newSignature);
            }

            Users.Sort((s1, s2) => (int.Parse(s1).CompareTo(int.Parse(s2))));
            
        }

        /// <summary>
        /// Returns all signatures for a UserId so we could bind them to a listbox
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<string> UserSignatureIds(string userId)
        {
            var usersSignatures = SignatureRepo.Signatures.FindAll(s => s.UserID == userId);
            usersSignatures.Sort((s1, s2) => (int.Parse(s1.SignatureID).CompareTo(int.Parse(s2.SignatureID))));
            return usersSignatures.Select(s => s.SignatureID).ToList();
        }


    }
}
