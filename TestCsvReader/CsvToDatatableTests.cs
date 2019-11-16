using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Data;
using System;
using System.Collections.Generic;
using CsvReader;

namespace TestCsvReader
{
    [TestClass]
    public class TestCsvToDatatable
    {
        [TestMethod]
        public void TestDataTable()
        {
            using (var mem = new FileStream("CSVs//test1.csv", FileMode.Open))
            using (var csvr = new CsvReader.CsvReader(mem))
            {
                int rowCount = 0;

                foreach (var dr in csvr.Rows())
                {
                    rowCount++;
                    if (dr[0].ToString() == "1")
                        Assert.IsTrue(dr[1].ToString() == "one");
                    if (dr[0].ToString() == "2")
                    {
                        Assert.IsTrue(dr[1].ToString() == "two");
                        Assert.IsTrue(dr["x"].ToString() == "comma,here");
                        Assert.IsTrue(dr["y"].ToString() == "new line\r\nhere");
                        Assert.IsTrue(dr["z"].ToString() == "double\"quote");
                    }
                    if (dr[0].ToString() == "3")
                        Assert.IsTrue(dr[1].ToString() == "three");

                    Assert.IsTrue(dr.ItemArray.Length == 7);
                }

                Assert.IsTrue(rowCount == 3);
            }
        }

        /// <summary>
        /// shouldn't care about an empty line at the end
        /// </summary>
        [TestMethod]
        public void TestDataTable_EmptyLine()
        {
            using (var mem = new FileStream("CSVs//test2.csv", FileMode.Open))
            using (var csvr = new CsvReader.CsvReader(mem))
            {
                int rowCount = 0;

                foreach (var dr in csvr.Rows())
                {
                    rowCount++;
                    Assert.IsTrue(dr.ItemArray.Length == 7);
                }

                Assert.IsTrue(rowCount == 3);
            }
        }

        /// <summary>
        /// tests it pages the data into tables
        /// </summary>
        [TestMethod]
        public void TestDataTable_Paging()
        {
            using (var mem = new FileStream("CSVs//test3.csv", FileMode.Open))
            using (var csvr = new CsvReader.CsvReader(mem))
            {
                int total = 0;

                foreach (var page in csvr.ToPagedData(4))
                {
                    Assert.IsTrue(page.Rows.Count <= 4);

                    foreach (DataRow dr in page.Rows)
                    {
                        var num = int.Parse(dr["id"].ToString());
                        total += num;
                    }
                }

                Assert.IsTrue(total == 55);
            }
        }

    }

}
