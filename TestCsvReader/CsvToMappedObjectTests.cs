using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Data;
using System;
using System.Collections.Generic;
using CsvReader;

namespace TestCsvReader
{
    [TestClass]
    public class TestCsvToObjectMapper
    {

        private class Record
        {
            [CsvMappingColumn("id")]
            public int Id { get; set; }

            [CsvMappingColumn("word")]
            public string Word { get; set; }

            [CsvMappingColumn("date")]
            public DateTime? Due { get; set; }

            [CsvMappingColumn("decimal")]
            public decimal Score { get; set; }

            [CsvMappingColumnsRemaining()]
            public Dictionary<string, string> Remainders { get; set; }

        }


        [TestMethod]
        public void TestMapping()
        {
            using (var fs = new FileStream("CSVs//test1.csv", FileMode.Open)) 
            using (var csvr = new CsvReader<Record>(fs))
            {
                int total = 0;

                foreach (var rec in csvr.Rows())
                {
                    total += rec.Id;

                    Assert.IsTrue(rec.Due == null || rec.Due > DateTime.Parse("2017-01-01"));
                }

                Assert.IsTrue(total == 6);
            }
        }

        [TestMethod]
        public void TestMappingAndPage()
        {
            using (var fs = new FileStream("CSVs//test1.csv", FileMode.Open))
            using (var csvr = new CsvReader<Record>(fs))
            {
                int total = 0;

                foreach (var allRecs in csvr.ToPagedData(100))
                {
                    foreach (var rec in allRecs)
                    {
                        total += rec.Id;
                    }

                    var first = allRecs[0];
                    Assert.IsTrue(first.Due == null || first.Due > DateTime.Parse("2017-01-01"));
                }

                Assert.IsTrue(total == 6);
            }
        }

    }
}
