using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSVFileReader;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVFileReader.Tests
{
    [TestClass()]
    public class CsvFileTests
    {
        [TestMethod()]
        public void LoadFileTest()
        {
            CsvFile testFile = new CsvFile();
            testFile.LoadFile(System.IO.Directory.GetCurrentDirectory() +@"\cities.csv");
            Assert.AreEqual(128, testFile.RowsCount);
        }

        [TestMethod()]
        public void LoadFileTest2()
        {
            CsvFile testFile = new CsvFile();
            testFile.LoadFile(System.IO.Directory.GetCurrentDirectory() + @"\addresses.csv");
            Assert.AreEqual(testFile[2]["first name"], "John \"\"Da Man\"\"");
        }

        [TestMethod()]
        public void LoadFileTest3()
        {
            CsvFile testFile = new CsvFile();
            testFile.LoadFile(System.IO.Directory.GetCurrentDirectory() + @"\homes.csv");
            Assert.AreEqual(testFile[12]["\"Beds\""], "Invalid Column");
        }

        [TestMethod()]
        public void CsvSplitLineTest()
        {
            string unsplitLine = "  words\"words  , \"text,text\", sample   sample     ";
            string[] expectedOutput = new string[3]
            {
                "words\"words",
                "text,text",
                "sample   sample"
            };
            CsvFile csvFile = new CsvFile();
            string[] actualOutput = csvFile.CsvSplitLine(unsplitLine);
            CollectionAssert.AreEqual(expectedOutput, actualOutput);
        }
    }
}