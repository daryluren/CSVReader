using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvReader
{

    public class CsvReader<T> : CsvReader<T, List<T>>
        where T : class, new()
    {
        public CsvReader(Stream Stream, bool IncludesHeader, char Delimeter)
            : base(Stream, IncludesHeader, Delimeter, new CsvToClassMapper())
        { }
        public CsvReader(Stream stream)
            : this(stream, true, ',')
        { }
        public CsvReader(Stream Stream, bool IncludesHeader)
            : this(Stream, IncludesHeader, ',')
        { }

        private class CsvToClassMapper : CsvMapper
        {
            private List<T> page;

            public override List<T> CurrentPage => page ?? (page = new List<T>());

            public override void AddRowToPage(T current) => CurrentPage.Add(current);

            public override int CurrentPageRowCount() => CurrentPage.Count;

            public override T MakeRow(string[] values)
            {
                T result = new T();

                var ColumnNamesList = new List<string>(ColumnNames);
                var ColumnsRemaining = new List<string>(ColumnNames);

                foreach (var prop in typeof(T).GetProperties().Where(p => p.GetCustomAttributes(typeof(CsvMappingColumnAttribute), true).Any()))
                {
                    var att = prop.GetCustomAttributes(typeof(CsvMappingColumnAttribute), true).First() as CsvMappingColumnAttribute;
                    var index = ColumnNamesList.IndexOf(att.ColumnName);
                    var val = Parse(prop.PropertyType, values[index]);
                    prop.SetValue(result, val);
                    ColumnsRemaining.Remove(att.ColumnName);
                }

                if (ColumnsRemaining.Any())
                    foreach (var prop in typeof(T).GetProperties().Where(p => p.GetCustomAttributes(typeof(CsvMappingColumnsRemainingAttribute), true).Any()))
                    {
                        if (prop.PropertyType != typeof(Dictionary<string, string>))
                            continue;

                        var dict = new Dictionary<string, string>();
                        prop.SetValue(result, dict);

                        foreach (var col in ColumnsRemaining)
                            dict[col] = values[ColumnNamesList.IndexOf(col)];

                        break;
                    }

                (result as ICsvMappable)?.AfterMapping();
                return result;
            }

            //https://codereview.stackexchange.com/questions/102289/setting-the-value-of-properties-via-reflection
            public static object Parse(Type type, string str)
            {
                bool IsNullableType()
                {
                    return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
                }

                bool IsNullable = false;
                try
                {
                    if (type == typeof(string))
                        return str;

                    if (IsNullableType())
                    {
                        type = Nullable.GetUnderlyingType(type);
                        IsNullable = true;
                    }

                    var parse = type.GetMethod("Parse", new[] { typeof(string) });
                    if (parse == null)
                    {
                        if (!IsNullable)
                            throw new NotSupportedException();
                        return null;
                    }
                    return parse.Invoke(null, new object[] { str });
                }
                //or don't catch
                catch (Exception)
                {
                    return null;
                }

            }

            public override void NextPage() { page = null; }
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class CsvMappingColumnAttribute : System.Attribute
    {
        public string ColumnName { get; private set; }

        public CsvMappingColumnAttribute(string ColumnName)
        {
            this.ColumnName = ColumnName;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class CsvMappingColumnsRemainingAttribute : System.Attribute
    {
    }

    public interface ICsvMappable
    {
        void AfterMapping();
    }

}
