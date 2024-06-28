using System;
using System.Globalization;
using System.IO;
using Crestron.SimplSharp;
using CsvHelper;
using CsvHelper.Configuration;

namespace mySimplSharpProLib
{
    public class CSVReader
    {
        private CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture);
        
        public CSVReader()
        {
            config.NewLine = Environment.NewLine;
            config.PrepareHeaderForMatch = args => args.Header.ToLower();
            config.HasHeaderRecord = true;
        }

        public void ReadCSV(string filepath)
        {
            using (var reader = new StreamReader(filepath))
            using (var csv = new CsvReader(reader, this.config))
            {
                var records = csv.GetRecords<MockType>();
                foreach (var record in records)
                {
                    CrestronConsole.PrintLine($"DEBUG CSV({record.Id}): {record.Email}");
                }
            }
        }
    }
}