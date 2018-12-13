using CsvParse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace FrameCoordinatesGenerator
{
    public class InputCsvData
    {
        static public InputCsvData Self;
        public List<CsvRow> DataRows;

        private StorageFile csvFile;
        private List<int> unsortedLedIndexes;

        public int AppendRowStartIndex = -1;
        public int AppendColumnStartIndex = -1;

        public int Column_Exist = -1;
        public int Column_LeftTopX = -1;
        public int Column_LeftTopY = -1;
        public int Column_RightBottomX = -1;
        public int Column_RightBottomY = -1;
        public int Column_Zindex = -1;
        public int Column_PNG = -1;
        
        public InputCsvData(StorageFile inputFile)
        {
            Self = this;
            csvFile = inputFile;
            DataRows = new List<CsvRow>();
            unsortedLedIndexes = new List<int>();
        }

        public async Task StartParsingAsync()
        {
            DataReset();

            using (CsvFileReader csvReader = new CsvFileReader(await csvFile.OpenStreamForReadAsync()))
            {
                CsvRow row = new CsvRow();
                int rowNumber = 0;

                while (csvReader.ReadRow(row))
                {
                    if (row[0].ToLower().Contains("parameter"))
                    {
                        for (int i = 1; i < row.Count; i++)
                        {
                            string s = row[i].ToLower();

                            if (s == "exist") { Column_Exist = i; }
                            else if (s.Contains("lefttop") && s.Contains("x")) { Column_LeftTopX = i; }
                            else if (s.Contains("lefttop") && s.Contains("y")) { Column_LeftTopY = i; }
                            else if (s.Contains("rightbottom") && s.Contains("x")) { Column_RightBottomX = i; }
                            else if (s.Contains("rightbottom") && s.Contains("y")) { Column_RightBottomY = i; }
                            else if (s.Contains("index") && s.Contains("z")) { Column_Zindex = i; }
                            else if (s == "png") { Column_PNG = i; }
                        }

                        AppendRowStartIndex = rowNumber;
                        AppendColumnStartIndex = row.Count;
                    }
                    else
                    {
                        string row_0 = row[0].ToLower();

                        if (row_0.Contains("led"))
                        {
                            row_0 = row_0.Replace("led", "").Replace(" ", "");

                            if (row[Column_Exist] == "1")
                                unsortedLedIndexes.Add(Int32.Parse(row_0));
                        }
                    }

                    DataRows.Add(row);
                    row = new CsvRow();
                    rowNumber++;
                }
            }
            
            Column_LeftTopX = Column_LeftTopX == -1 ? AppendColumnStartIndex++ : Column_LeftTopX;
            Column_LeftTopY = Column_LeftTopY == -1 ? AppendColumnStartIndex++ : Column_LeftTopY;
            Column_RightBottomX = Column_RightBottomX == -1 ? AppendColumnStartIndex++ : Column_RightBottomX;
            Column_RightBottomY = Column_RightBottomY == -1 ? AppendColumnStartIndex++ : Column_RightBottomY;
            Column_PNG = Column_PNG == -1 ? AppendColumnStartIndex++ : Column_PNG;
            Column_Zindex = Column_Zindex == -1 ? AppendColumnStartIndex++ : Column_Zindex;

            // Align all rows data to rightmost column
            for (int i = 0; i < DataRows.Count; i++)
            {
                for (int j = DataRows[i].Count; j < AppendColumnStartIndex; j++)
                {
                    DataRows[i].Add("");
                }
            }
            
            DataRows[AppendRowStartIndex][Column_LeftTopX] = "LeftTop_x";
            DataRows[AppendRowStartIndex][Column_LeftTopY] = "LeftTop_y";
            DataRows[AppendRowStartIndex][Column_RightBottomX] = "RightBottom_x";
            DataRows[AppendRowStartIndex][Column_RightBottomY] = "RightBottom_y";
            DataRows[AppendRowStartIndex][Column_PNG] = "PNG";
            DataRows[AppendRowStartIndex][Column_Zindex] = "Z_index";
        }

        private void DataReset()
        {
            DataRows = new List<CsvRow>();
            unsortedLedIndexes = new List<int>();

            AppendRowStartIndex = -1;
            AppendColumnStartIndex = -1;

            Column_Exist = -1;
            Column_LeftTopX = -1;
            Column_LeftTopY = -1;
            Column_RightBottomX = -1;
            Column_RightBottomY = -1;
            Column_Zindex = -1;
            Column_PNG = -1;
        }

        public List<int> GetIndexOrder()
        {
            return unsortedLedIndexes;
        }

        public List<CsvRow> GetCopiedDataRows()
        {
            List<CsvRow> newList = new List<CsvRow>();

            DataRows.ForEach((item) =>
            {
                newList.Add(item.Copy());
            });

            return newList;
        }
    }
}
