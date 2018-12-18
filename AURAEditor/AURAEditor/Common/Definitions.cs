using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AuraEditor.Common.AuraEditorColorHelper;

namespace AuraEditor.Common
{
    static class Definitions
    {
        static Definitions()
        {
            DefaultColorList = new List<List<ColorPoint>>();

            List<ColorPoint> DefaultColorPoints1 = new List<ColorPoint>();
            List<ColorPoint> DefaultColorPoints2 = new List<ColorPoint>();
            List<ColorPoint> DefaultColorPoints3 = new List<ColorPoint>();
            List<ColorPoint> DefaultColorPoints4 = new List<ColorPoint>();
            List<ColorPoint> DefaultColorPoints5 = new List<ColorPoint>();
            List<ColorPoint> DefaultColorPoints6 = new List<ColorPoint>();

            DefaultColorPoints1 = new List<ColorPoint>()
            {
                new ColorPoint() { Color =HexToColor("#FFFFFFFF"), Offset = 0.0 },
                new ColorPoint() { Color =HexToColor("#FF4E4E4E"), Offset = 1.0 },
            };

            DefaultColorPoints2 = new List<ColorPoint>()
            {
                new ColorPoint() { Color =HexToColor("#FFFEBE3F"), Offset = 0.0 },
                new ColorPoint() { Color =HexToColor("#FFFE3F7D"), Offset = 0.5 },
                new ColorPoint() { Color =HexToColor("#FFF91D1D"), Offset = 1.0 },
            };

            DefaultColorPoints3 = new List<ColorPoint>()
            {
                new ColorPoint() { Color =HexToColor("#FFD1FE3F"), Offset = 0.0 },
                new ColorPoint() { Color =HexToColor("#FF00DCFF"), Offset = 0.33 },
                new ColorPoint() { Color =HexToColor("#FF00DCFF"), Offset = 0.66 },
                new ColorPoint() { Color =HexToColor("#FFD1FE3F"), Offset = 1.0 },
            };

            DefaultColorPoints4 = new List<ColorPoint>()
            {
                new ColorPoint() { Color =HexToColor("#FFF1FF00"), Offset = 0.0 },
                new ColorPoint() { Color =HexToColor("#FFFFB500"), Offset = 0.25 },
                new ColorPoint() { Color =HexToColor("#FFF1FF00"), Offset = 0.5 },
                new ColorPoint() { Color =HexToColor("#FFFFB500"), Offset = 0.75 },
                new ColorPoint() { Color =HexToColor("#FFF1FF00"), Offset = 1.0 },
            };

            DefaultColorPoints5 = new List<ColorPoint>()
            {
                new ColorPoint() { Color =HexToColor("#FFFF0091"), Offset = 0.0 },
                new ColorPoint() { Color =HexToColor("#FF8C00FF"), Offset = 0.2 },
                new ColorPoint() { Color =HexToColor("#FF4B00D9"), Offset = 0.4 },
                new ColorPoint() { Color =HexToColor("#FF4B00D9"), Offset = 0.6 },
                new ColorPoint() { Color =HexToColor("#FF8C00FF"), Offset = 0.8 },
                new ColorPoint() { Color =HexToColor("#FFFF0091"), Offset = 1.0 },
            };

            DefaultColorPoints6 = new List<ColorPoint>()
            {
                new ColorPoint() { Color =HexToColor("#FFFF000D"), Offset = 0.0 },
                new ColorPoint() { Color =HexToColor("#FFF500FF"), Offset = 0.16 },
                new ColorPoint() { Color =HexToColor("#FF0006FF"), Offset = 0.32 },
                new ColorPoint() { Color =HexToColor("#FF00FAFF"), Offset = 0.48 },
                new ColorPoint() { Color =HexToColor("#FF01FF00"), Offset = 0.64 },
                new ColorPoint() { Color =HexToColor("#FFFFF600"), Offset = 0.8 },
                new ColorPoint() { Color =HexToColor("#FFFFF600"), Offset = 1.0 },
            };

            DefaultColorList.Add(DefaultColorPoints1);
            DefaultColorList.Add(DefaultColorPoints2);
            DefaultColorList.Add(DefaultColorPoints3);
            DefaultColorList.Add(DefaultColorPoints4);
            DefaultColorList.Add(DefaultColorPoints5);
            DefaultColorList.Add(DefaultColorPoints6);
        }

        static public int GetSecondsPerTimeUnitByLevel(int level)
        {
            int seconds;

            if (level == 1) seconds = 1;
            else if (level == 2) seconds = 2;
            else if (level == 3) seconds = 5;
            else if (level == 4) seconds = 15;
            else if (level == 5) seconds = 30;
            else seconds = 60;

            return seconds;
        }
        public const int GridPixels = 24;
        public const double PixelsPerTimeUnit = 200;
        public const double MaxEditTime = 180; // second
        public const string UserScriptsDefaultFolderPath = "C:\\ProgramData\\ASUS\\AURA Creator\\UserScripts\\";
        public const string UserFilesDefaultFolderPath = "C:\\ProgramData\\ASUS\\AURA Creator\\UserFiles\\";
        public const string LocalUserScriptsFolderName = "UserScripts";
        public const string LocalUserFilesFolderName = "UserFiles";
        public const float SpaceZoomDefaultPercent = 50;

        static public List<List<ColorPoint>> DefaultColorList;
    }
}                     
                                                    