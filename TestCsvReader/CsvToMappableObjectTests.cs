using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Data;
using System;
using System.Collections.Generic;
using CsvReader;

namespace TestCsvReader
{
    [TestClass]
    public class TestCsvToMappableObject
    {

        private class Record : ICsvMappable
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

            public bool IsEven { get; private set; }

            public void AfterCsvMapping()
            {
                IsEven = Id % 2 == 0;
            }
        }


        [TestMethod]
        public void TestMapping()
        {
            using (var fs = new FileStream("CSVs//test1.csv", FileMode.Open))
            using (var csvr = new CsvReader<Record>(fs))
            {
                int total = 0;
                bool isEven = false;

                foreach (var rec in csvr.Rows())
                {
                    total += rec.Id;

                    Assert.IsTrue(rec.Due == null || rec.Due > DateTime.Parse("2017-01-01"));
                    Assert.IsTrue(rec.IsEven == isEven);
                    isEven = !isEven;
                }

                Assert.IsTrue(total == 6);
            }
        }

    }
}
