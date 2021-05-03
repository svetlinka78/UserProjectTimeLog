using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UserProject
{
    public class SpResults
    {
        private readonly DbDataReader _reader;

        public SpResults(DbDataReader reader)
        {
            _reader = reader;
        }

        public IList<T> ReaderToList<T>() where T : new()
        {
            return MapToList<T>(_reader);
        }
  
        public bool NextResult()
        {
            return _reader.NextResult();
        }
        public Task<bool> NextResultAsync(CancellationToken token)
        {
            return _reader.NextResultAsync(token);
        }

        public Task<bool> NextResultAsync()
        {
            return _reader.NextResultAsync();
        }


        //private fields
        private static IList<T> MapToList<T>(DbDataReader reader) where T : new()
        {
            
            var lst = new List<T>();
            var props = typeof(T).GetRuntimeProperties().ToList();

            //map name of the given column to schema column
            var cols = reader.GetColumnSchema()
                .Where(s => props.Any(p => string.Equals(p.Name, s.ColumnName, StringComparison.CurrentCultureIgnoreCase)))
                .ToDictionary(key => key.ColumnName.ToUpper());

            if (!reader.HasRows)
                return lst;

            while(reader.Read())
            {
                var obj = new T();
                foreach (var prop in props)
                {
                    var propName = prop.Name.ToUpper();
                    if (!cols.ContainsKey(propName))
                        continue;


                    var col = cols[propName];   

                    //how the column are ordered -position
                    if (col?.ColumnOrdinal == null)
                        continue;
                    var val = reader.GetValue(col.ColumnOrdinal.Value);

                    //for db null in sql server
                    prop.SetValue(obj, val == DBNull.Value ? null : val);
                }

                lst.Add(obj);
            }
            return lst;
        }

        private static T? MapToValue<T>(DbDataReader reader) where T : struct
        {
            if (!reader.HasRows)
                return new T?();

            if (reader.Read())
            {
                return reader.IsDBNull(0) ? new T?() : reader.GetFieldValue<T>(0);
            }

            return new T?();
        }
    }

}

