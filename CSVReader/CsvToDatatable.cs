using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvReader
{
    public class CsvReader : CsvReader<DataRow, DataTable>
    {
        public CsvReader(Stream stream, bool isHeaderIncluded, char delimeter)
            : base(stream, isHeaderIncluded, delimeter, new CsvToDataRowMapper())
        { }
        public CsvReader(Stream Stream)
            : this(Stream, true, ',')
        { }
        public CsvReader(Stream stream, bool isHeaderIncluded)
            : this(stream, isHeaderIncluded, ',')
        { }

        private class CsvToDataRowMapper : CsvMapper
        {
            private DataTable resultTable;

            public override DataTable CurrentPage => resultTable ?? (resultTable = MakeTable());

            public override DataRow MakeRow(string[] values)
            {
                if (CurrentPage.Columns.Count == 0)
                {
                    int i = 0;
                    foreach (var cell in values)
                    {
                        resultTable.Columns.Add("Column" + (i++).ToString());
                    }
                }

                var r = resultTable.NewRow();
                r.ItemArray = values;
                return r;
            }

            public override void AddRowToPage(DataRow current) => resultTable.Rows.Add(current);

            public override int CurrentPageRowCount() => resultTable.Rows.Count;

            public override void NextPage() => resultTable = resultTable.Clone();

            private DataTable MakeTable()
            {
                var result = new DataTable();
                if (ColumnNames != null)
                {
                    foreach (var name in ColumnNames)
                    {
                        result.Columns.Add(name);
                    }
                }
                return result;
            }

        }
    }

}
