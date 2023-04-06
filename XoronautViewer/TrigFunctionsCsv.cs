using DataTicker3D;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XoronautViewer
{
    internal class TrigFunctionsCsv : ITimeTickerDataProvider
    {
        public String FileName { get; set; }

        public TrigFunctionsCsv(String pathAndFileName)
        {
            FileName = pathAndFileName;
        }

        public SortedDictionary<double, double> getData()
        {
            if (validateFileName() == false)
                return null;

            var returnDict = new SortedDictionary<Double, Double>();

            String line; String[] parsedLine; char[] delimChar = { ',' };
            using (System.IO.StreamReader file = new System.IO.StreamReader(FileName))
            {
                while ((line = file.ReadLine()) != null)
                {
                    parsedLine = line.Split(delimChar);
                    try
                    {
                        var x = Convert.ToDouble(parsedLine[0]);
                        var y1 = Convert.ToDouble(parsedLine[1]);
                        var y2 = Convert.ToDouble(parsedLine[2]);
                        var newDT = DateTime.FromOADate(x);

                    }
                    catch (FormatException fe)
                    { }
                }

            }

            return returnDict;
        }

        public List<SortedDictionary<double, double>> getDataList()
        {
            if (validateFileName() == false)
                return null;

            var sineDict = new SortedDictionary<double, Double>();
            var cosineDict = new SortedDictionary<double, Double>();

            String line; String[] parsedLine; char[] delimChar = { ',' };
            using (System.IO.StreamReader file = new System.IO.StreamReader(FileName))
            {
                while ((line = file.ReadLine()) != null)
                {
                    parsedLine = line.Split(delimChar);
                    try
                    {
                        var x = Convert.ToDouble(parsedLine[0]);
                        var y1 = Convert.ToDouble(parsedLine[1]);
                        var y2 = Convert.ToDouble(parsedLine[2]);

                        sineDict[x] = y1;
                        cosineDict[x] = y2;
                    }
                    catch (FormatException fe)
                    { }
                }

            }

            var retval = new List<SortedDictionary<double, double>>();
            retval.Add(sineDict);
            retval.Add(cosineDict);
            return retval;
        }

        private bool validateFileName()
        {
            if (FileName == null) return false;
            if (FileName.Length == 0) return false;
            if (File.Exists(FileName) == false) return false;
            return true;
        }
    }
}
