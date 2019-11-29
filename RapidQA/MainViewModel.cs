using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace RapidQA
{
    class MainViewModel
    {
        public ObservableCollection<Asset> Covers { get; set; } = new ObservableCollection<Asset>();
        public ObservableCollection<Asset> Frames { get; set; } = new ObservableCollection<Asset>();
        public ObservableCollection<Asset> Legs { get; set; } = new ObservableCollection<Asset>();
        public ObservableCollection<Asset> Baskets { get; set; } = new ObservableCollection<Asset>();
        public ObservableCollection<Asset> ClothesRails { get; set; } = new ObservableCollection<Asset>();
        public ObservableCollection<Asset> Shelves { get; set; } = new ObservableCollection<Asset>();

        public void LoadFiles(string rootDirectory)
        {
            var fileAssets = new List<Asset>();
            var files = Directory.GetFiles(rootDirectory, "*.png");

            foreach (var file in files)
            {
                try
                {
                    fileAssets.Add(new Asset(file));
                }
                catch (Exception ex)
                {
                    throw new Exception("Error loading image asset: " + file.ToString(), ex);
                }
            }

            foreach (Asset asset in fileAssets)
            {
                switch (asset.Family)
                {
                    case "Frames":
                        Frames.Add(asset);
                        break;

                    case "Legs":
                        Legs.Add(asset);
                        break;

                    case "Covers":
                        Covers.Add(asset);
                        break;

                    case "Baskets":
                        Baskets.Add(asset);
                        break;

                    case "ClothesRails":
                        ClothesRails.Add(asset);
                        break;

                    case "Shelves":
                        Shelves.Add(asset);
                        break;
                }

            }
        }
    }
}
