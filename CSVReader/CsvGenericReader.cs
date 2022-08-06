using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvReader
{

    public class CsvReader<TRow, TPage> : IDisposable
        where TRow : class
        where TPage : class
    {
        // need two enumerators to deal with the special case header row
        private IEnumerator<string[]> LineEnumerator { get; set; }
        private IEnumerator<TRow> DataRowEnumerator { get; set; }
        private CsvMapper Mapper { get; set; }

        public CsvReader(Stream stream, bool isHeaderIncluded, char delimeter, CsvMapper mapper)
        {
            this.Mapper = mapper;
            LineEnumerator = RowReader(stream, delimeter).GetEnumerator();
            if (isHeaderIncluded)
            {
                LineEnumerator.MoveNext();
                mapper.ColumnNames = LineEnumerator.Current;
            }
        }

        public TPage ToPagedData() => ToPagedData(int.MaxValue)?.First();

        public IEnumerable<TPage> ToPagedData(int pageSize)
        {
            DataRowEnumerator = Rows().GetEnumerator();

            while (DataRowEnumerator.MoveNext())
            {
                Mapper.AddRowToPage(DataRowEnumerator.Current);
                if (Mapper.CurrentPageRowCount() >= pageSize)
                {
                    yield return Mapper.CurrentPage;
                    Mapper.NextPage();
                }
            }

            if (Mapper.CurrentPageRowCount() > 0)
            {
                yield return Mapper.CurrentPage;
            }
        }

        public IEnumerable<TRow> Rows()
        {
            while (LineEnumerator.MoveNext())
            {
                yield return Mapper.MakeRow(LineEnumerator.Current);
            }
        }

        /// <summary>
        /// Yields rows from the csv as a string[].
        /// Responsible for parsing the csv and all the comma/newline/quote-handling glory
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        private IEnumerable<string[]> RowReader(Stream stream, char delimiter)
        {
            using var reader = new StreamReader(stream);
            var cells = new List<StringBuilder>();
            var currentCell = new StringBuilder();
            cells.Add(currentCell);

            while (true)
            {
                int i = reader.Read();
                if (i == -1)
                {
                    if (cells.Count > 1 || !string.IsNullOrEmpty(cells[0].ToString()))
                    {
                        yield return (from cell in cells select cell.ToString()).ToArray();
                    }
                    break;
                }
                else
                {
                    var c = (char)i;
                    if (c == '\r')
                    {
                        // ignore
                    }
                    else if (c == '\n')
                    {
                        yield return (from cell in cells select cell.ToString()).ToArray();
                        cells = new List<StringBuilder>();
                        currentCell = new StringBuilder();
                        cells.Add(currentCell);
                    }
                    else if (c == delimiter)
                    {
                        currentCell = new StringBuilder();
                        cells.Add(currentCell);
                    }
                    else if (c == '"' && string.IsNullOrEmpty(currentCell.ToString()))
                    {
                        // start quoted string
                        while (true)
                        {
                            var j = (char)reader.Read();

                            if (j == '"')
                            {
                                var peek = reader.Peek();
                                if (peek != -1 && (char)peek == '"')
                                {
                                    // it's a quoted quote like this: "first part""second part"
                                    reader.Read();
                                    currentCell.Append('"');
                                }
                                else
                                {
                                    // it's the end of a quoted string
                                    break;
                                }
                            }
                            else
                            {
                                // contents of the quoted string
                                currentCell.Append(j);
                            }
                        }
                    }
                    else
                    {
                        currentCell.Append(c);
                    }
                }
            }
        }

        public abstract class CsvMapper
        {
            public string[] ColumnNames { get; set; }
            public abstract TPage CurrentPage { get; }

            public abstract TRow MakeRow(string[] values);
            public abstract void AddRowToPage(TRow current);
            public abstract int CurrentPageRowCount();
            public abstract void NextPage();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    LineEnumerator?.Dispose();
                    DataRowEnumerator?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CsvReader() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
