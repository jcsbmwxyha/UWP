using AuraEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AuraEditor.Common.AuraEditorColorHelper;
using static AuraEditor.Common.EffectHelper;

namespace AuraEditor.Common
{
    static class Definitions
    {
        static Definitions()
        {
            DefaultColorPointListCollection = new List<List<ColorPointModel>>();

            List<ColorPointModel> DefaultColorPoints1 = new List<ColorPointModel>();
            List<ColorPointModel> DefaultColorPoints2 = new List<ColorPointModel>();
            List<ColorPointModel> DefaultColorPoints3 = new List<ColorPointModel>();
            List<ColorPointModel> DefaultColorPoints4 = new List<ColorPointModel>();
            List<ColorPointModel> DefaultColorPoints5 = new List<ColorPointModel>();
            List<ColorPointModel> DefaultColorPoints6 = new List<ColorPointModel>();

            DefaultColorPoints1 = new List<ColorPointModel>()
            {
                new ColorPointModel() { Color =HexToColor("#FFFFFFFF"), Offset = 0.0 },
                new ColorPointModel() { Color =HexToColor("#FF4E4E4E"), Offset = 1.0 },
            };

            DefaultColorPoints2 = new List<ColorPointModel>()
            {
                new ColorPointModel() { Color =HexToColor("#FFFEBE3F"), Offset = 0.0 },
                new ColorPointModel() { Color =HexToColor("#FFFE3F7D"), Offset = 0.5 },
                new ColorPointModel() { Color =HexToColor("#FFF91D1D"), Offset = 1.0 },
            };

            DefaultColorPoints3 = new List<ColorPointModel>()
            {
                new ColorPointModel() { Color =HexToColor("#FFD1FE3F"), Offset = 0.0 },
                new ColorPointModel() { Color =HexToColor("#FF00DCFF"), Offset = 0.33 },
                new ColorPointModel() { Color =HexToColor("#FF00DCFF"), Offset = 0.66 },
                new ColorPointModel() { Color =HexToColor("#FFD1FE3F"), Offset = 1.0 },
            };

            DefaultColorPoints4 = new List<ColorPointModel>()
            {
                new ColorPointModel() { Color =HexToColor("#FFF1FF00"), Offset = 0.0 },
                new ColorPointModel() { Color =HexToColor("#FFFFB500"), Offset = 0.25 },
                new ColorPointModel() { Color =HexToColor("#FFF1FF00"), Offset = 0.5 },
                new ColorPointModel() { Color =HexToColor("#FFFFB500"), Offset = 0.75 },
                new ColorPointModel() { Color =HexToColor("#FFF1FF00"), Offset = 1.0 },
            };

            DefaultColorPoints5 = new List<ColorPointModel>()
            {
                new ColorPointModel() { Color =HexToColor("#FFFF0091"), Offset = 0.0 },
                new ColorPointModel() { Color =HexToColor("#FF8C00FF"), Offset = 0.2 },
                new ColorPointModel() { Color =HexToColor("#FF4B00D9"), Offset = 0.4 },
                new ColorPointModel() { Color =HexToColor("#FF4B00D9"), Offset = 0.6 },
                new ColorPointModel() { Color =HexToColor("#FF8C00FF"), Offset = 0.8 },
                new ColorPointModel() { Color =HexToColor("#FFFF0091"), Offset = 1.0 },
            };

            DefaultColorPoints6 = new List<ColorPointModel>()
            {
                new ColorPointModel() { Color =HexToColor("#FFFF000D"), Offset = 0.0 },
                new ColorPointModel() { Color =HexToColor("#FFF500FF"), Offset = 0.16 },
                new ColorPointModel() { Color =HexToColor("#FF0006FF"), Offset = 0.32 },
                new ColorPointModel() { Color =HexToColor("#FF00FAFF"), Offset = 0.48 },
                new ColorPointModel() { Color =HexToColor("#FF01FF00"), Offset = 0.64 },
                new ColorPointModel() { Color =HexToColor("#FFFFF600"), Offset = 0.8 },
                new ColorPointModel() { Color =HexToColor("#FFFF000D"), Offset = 1.0 },
            };

            DefaultColorPointListCollection.Add(DefaultColorPoints1);
            DefaultColorPointListCollection.Add(DefaultColorPoints2);
            DefaultColorPointListCollection.Add(DefaultColorPoints3);
            DefaultColorPointListCollection.Add(DefaultColorPoints4);
            DefaultColorPointListCollection.Add(DefaultColorPoints5);
            DefaultColorPointListCollection.Add(DefaultColorPoints6);

            foreach (var list in DefaultColorPointListCollection)
                SetColorPointBorders(list);
        }

        static public int GetMSecondsPerTimeUnitByLevel(int level)
        {
            int seconds;

            if (level == 1) seconds = 200;
            else if (level == 2) seconds = 1000;
            else if (level == 3) seconds = 2000;
            else if (level == 4) seconds = 5000;
            else if (level == 5) seconds = 15000;
            else seconds = 30000;

            return seconds;
        }
        public const int GridPixels = 24;
        public const double PixelsBetweenLongLines = 200;
        public const double MaxEditTime = 360; // second
        public const string LocalUserScriptsFolderName = "UserScripts";
        public const string LocalUserFilesFolderName = "UserFiles";
        public const float SpaceZoomDefaultPercent = 50;

        static public List<List<ColorPointModel>> DefaultColorPointListCollection;
    }
}                                     