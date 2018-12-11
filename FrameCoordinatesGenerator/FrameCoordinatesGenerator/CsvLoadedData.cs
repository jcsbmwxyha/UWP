using CsvParse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace FrameCoordinatesGenerator
{

    public class CsvLoadedData
    {
        public List<CsvRow> DataRows;

        private StorageFile csvFile;
        private int column_exist = -1;
        private int column_leftTopX = -1;
        private int column_leftTopY = -1;
        private int column_rightBottomX = -1;
        private int column_rightBottomY = -1;
        private int column_z = -1;
        private int column_png = -1;

        public int LedDataRowStartIndex = -1;
        public int LedDataColumnEndIndex = -1;
        private List<int> ledOrderedIndex;

        public CsvLoadedData(StorageFile inputFile)
        {
            csvFile = inputFile;
            DataRows = new List<CsvRow>();
            ledOrderedIndex = new List<int>();
        }

        public async Task StartParsingAsync()
        {
            DataRows = new List<CsvRow>();

            using (CsvFileReader csvReader = new CsvFileReader(await csvFile.OpenStreamForReadAsync()))
            {
                CsvRow row = new CsvRow();
                int rowNumber = 0;

                while (csvReader.ReadRow(row))
                {
                    rowNumber++;

                    if (row[0].ToLower().Contains("parameter"))
                    {
                        for (int i = 0; i < row.Count; i++)
                        {
                            if (row[i].ToLower() == "exist") { column_exist = i; }
                            else if (row[i].ToLower() == "lefttop_x") { column_leftTopX = i; }
                            else if (row[i].ToLower() == "lefttop_y") { column_leftTopY = i; }
                            else if (row[i].ToLower() == "rightbottom_x") { column_rightBottomX = i; }
                            else if (row[i].ToLower() == "rightbottom_y") { column_rightBottomY = i; }
                            else if (row[i].ToLower() == "z_index" || row[i].ToLower() == "zindex") { column_z = i; }
                            else if (row[i].ToLower() == "png") { column_png = i; }
                        }

                        LedDataRowStartIndex = rowNumber - 1;
                        LedDataColumnEndIndex = row.Count;
                    }
                    else
                    {
                        string row0 = row[0].ToLower();

                        if (row0.Contains("led"))
                        {
                            row0 = row0.Replace("led", "").Replace(" ", "");

                            if (row[column_exist] == "1")
                                ledOrderedIndex.Add(Int32.Parse(row0));
                        }
                    }

                    DataRows.Add(row);
                    row = new CsvRow();
                }
            }
        }

        public int[] GetIndexOrderArray()
        {
            return ledOrderedIndex.ToArray();
        }
    }
}
