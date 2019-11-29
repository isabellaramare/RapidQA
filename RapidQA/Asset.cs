using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RapidQA
{
    internal class Asset
    {
        // Filename instead of filepath?
        public string Filepath { get; set; }
        public string Size { get; set; }
        public string Width { get; set; }
        public string Depth { get; set; }
        public string Height { get; set; }
        public string Name { get; set; }
        public string Family { get; set; }
        public string PartType { get; set; }
        public string Details { get; set; }
        // Depth
        public int ZLayer { get; set; }
        // width
        public int YPosition { get; set; }
        // height
        public int XPosition { get; set; }

        public Asset(string filepath)
        {
            Init(filepath);
        }

        private void Init(string filepath)
        {
            Filepath = filepath;
            var splt = Path.GetFileNameWithoutExtension(Filepath).Split('_');

            foreach (var word in splt)
            {
                Name += word + " ";
            }

            // insert config file to help sort the files


            //PartType = splt[2];        

            //if (PartType == "txtl" || PartType == "mesh" || PartType == "wire")
            //{
            //    Family = "Baskets";
            //    Size = $"{splt[4]}x{splt[5]}x{splt[6]}";
            //    Width = splt[4];
            //    Depth = splt[5];
            //    Height = splt[6];

            //}

            //else if (PartType == "castors")
            //{
            //    Family = "Legs";
            //    Size = $"{splt[5]}x{splt[6]}x{splt[7]}";
            //    Width = splt[5];
            //    Depth = splt[6];
            //    Height = splt[7];

            //}
            //else if (PartType == "paws")
            //{
            //    Family = "Legs";
            //    Size = $"{splt[3]}x{splt[4]}x{splt[5]}";
            //    Width = splt[3];
            //    Depth = splt[4];
            //    Height = splt[5];

            //}

            //else if (PartType == "frame")
            //{
            //    Family = "Frames";
            //    Size = $"{splt[3]}x{splt[4]}x{splt[5]}";
            //    Width = splt[3];
            //    Depth = splt[4];
            //    Height = splt[5];      
            //}

            //else if (PartType == "cover")
            //{
            //    Family = "Covers";
            //    Size = $"{splt[3]}x{splt[4]}x{splt[5]}";
            //    Width = splt[3];
            //    Depth = splt[4];
            //    Height = splt[5];  
            //}

            //else if (PartType == "adj")
            //{
            //    Family = "ClothesRails";
            //    Size = $"{splt[5]}x{splt[6]}x{splt[7]}";
            //    Width = splt[5];
            //    Depth = splt[6];
            //    Height = splt[7];      
            //}

            //else if (PartType == "top")
            //{
            //    Family = "Shelves";
            //    Size = $"{splt[4]}x{splt[5]}x{splt[6]}";
            //    Width = splt[4];
            //    Depth = splt[5];
            //    Height = splt[6];        
            //}

            //else if (PartType == "shelf")
            //{                    
            //    Family = "Frames";
            //    Size = $"{splt[4]}x{splt[5]}x{splt[6]}";
            //    Width = splt[4];
            //    Depth = splt[5];
            //    Height = splt[6];          
            //}

            //else if (PartType == "adj")
            //{
            //    Family = "Clothes Rails";
            //    Size = $"{splt[5]}x{splt[6]}x{splt[7]}";
            //    Width = splt[5];
            //    Depth = splt[6];
            //    Height = splt[7];         
            //}

            //else
            //{
            //    Family = "No Family";
            //    Size = "0x0x0";
            //    Width = "0";
            //    Depth = "0";
            //    Height = "0";          
            //}

            //SetDetails(splt);
            //CreateName(splt);
        }

        private void SetDetails(string[] splt)
        {
            // Add State (eg. Open, closed)
            var spltReversed = splt.Reverse();
            foreach (string word in spltReversed)
            {
                if (word != "0001" && word != "white")
                {

                    if (word == Height)
                    {
                        break;
                    }

                    Details += " " + word;
                }
            }
        }

        private void CreateName(string[] splt)
        {
            // Checks number of words before size declaration (eg. 50_51_104) to get the whole name if it has several words
            int nameLenght = 0;
            for (int i = 2; i < splt.Length; i++)
            {
                // Checks if the split string contains numbers
                bool isLetters = string.IsNullOrEmpty(splt[i]) || splt[0][0] == '$' || !splt[i].Any(c => char.IsDigit(c));
                if (isLetters)
                {
                    nameLenght += 1;
                }
                else if (!isLetters)
                {
                    break;
                }
            }

            Name = splt[2];
            for (int i = 1; i < nameLenght; i++)
            {
                Name += " " + splt[2 + i];
            }

            // Add size to name
            Name += " " + Size;
            Name += Details;
        }

        public override string ToString()
        {
            return Name;
        }

    }
}
