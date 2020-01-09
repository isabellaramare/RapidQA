using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RapidQA
{
    public class Asset
    {
        // Filename instead of filepath?
        public string Filepath { get; set; }
        public string Name { get; set; }


        public Asset()
        {

        }

        public Asset(string filepath)
        {
            Init(filepath);
        }

        private void Init(string filepath)
        {
            Filepath = filepath;
            Name = Path.GetFileNameWithoutExtension(Filepath);
        }

        public override string ToString()
        {
            return Name;
        }

    }
}
