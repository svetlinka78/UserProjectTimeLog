using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;

namespace UserProject
{
    public static class Extensions
    {
      

        public static DbCommand GetStoredProc(this DbContext context, string spName,
           bool defSchema = true, short commandTimeOut = 30)
        {
            var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandTimeout = commandTimeOut;

            if (defSchema)
            {
                var schemaName = context.Model["DefaultSchema"];
                if (schemaName != null)
                {
                    spName = $"{schemaName}.{spName}";
                }
            }

            command.CommandText = spName;
            command.CommandType = CommandType.StoredProcedure;

            return command;
        }


        public static DbCommand SqlParams(this DbCommand command, string paramName, object paramVal,
            Action<DbParameter> setParam = null)
        {
         
            var param = command.CreateParameter();
            param.ParameterName = paramName;
            param.Value = paramVal ?? DBNull.Value;
            setParam?.Invoke(param);
            command.Parameters.Add(param);
            return command;
        }
        public static DbCommand SqlParams(this DbCommand command, IDbDataParameter parameter)
        {
            command.Parameters.Add(parameter);
            return command;
        }

        public static DbCommand SqlParams(this DbCommand command, IDbDataParameter[] parameters)
        {
            command.Parameters.AddRange(parameters);

            return command;
        }
        public static DbCommand SqlParams(this DbCommand command, string paramName, Action<DbParameter> setParam = null)
        {
            var param = command.CreateParameter();
            param.ParameterName = paramName;
            setParam?.Invoke(param);
            command.Parameters.Add(param);
            return command;

        }


        public static void ExecuteSp(this DbCommand command, Action<SpResults> handle,
           CommandBehavior commandBehaviour = CommandBehavior.Default,
           bool isConnected = true)
        {
            if (handle == null)
            {
                throw new ArgumentNullException(nameof(handle));
            }

            using (command)
            {
                if (isConnected && command.Connection.State == ConnectionState.Closed)
                    command.Connection.Open();
                try
                {
                    using (var reader = command.ExecuteReader(commandBehaviour))
                    {
                        var spResults = new SpResults(reader);
                        handle(spResults);
                    }
                }
                finally
                {
                    if (isConnected)
                    {
                        command.Connection.Close();
                    }
                }
            }
        }


        public static async Task ExecuteSpAsync(this DbCommand command, Action<SpResults> handle,
            CommandBehavior commandBehaviour = CommandBehavior.Default,
            CancellationToken token = default, bool isConnected = true)
        {
            if (handle == null)
            {
                throw new ArgumentNullException(nameof(handle));
            }

            using (command)
            {
                if (isConnected && command.Connection.State == ConnectionState.Closed)
                    await command.Connection.OpenAsync(token).ConfigureAwait(false);
                try
                {
                    using (var reader = await command.ExecuteReaderAsync(commandBehaviour, token)
                        .ConfigureAwait(false))
                    {
                        var spResults = new SpResults(reader);
                        handle(spResults);
                    }
                }
                finally
                {
                    if (isConnected)
                    {
                        command.Connection.Close();
                    }
                }
            }
        }

        public static async Task ExecuteSpAsync(this DbCommand command,
           CommandBehavior commandBehaviour = CommandBehavior.Default,
           CancellationToken token = default, bool isConnected = true,
           params Action<SpResults>[] handlers) //array
        {
            if (handlers == null)
            {
                throw new ArgumentNullException(nameof(handlers));
            }

            using (command)
            {
                if (isConnected && command.Connection.State == ConnectionState.Closed)
                    await command.Connection.OpenAsync(token).ConfigureAwait(false);
                try
                {
                    using (var reader = await command.ExecuteReaderAsync(commandBehaviour,token)
                        .ConfigureAwait(false))
                    {
                        var spResults = new SpResults(reader);

                        foreach (var handler in handlers)
                            handler(spResults);
                    }
                }
                finally
                {
                    if (isConnected)
                    {
                        command.Connection.Close();
                    }
                }
            }
        }

        public static int ExecuteSpNonQuery(this DbCommand command, bool isConnected = true)
        {
            var affectedRows = -1;

            using (command)
            {
                if (command.Connection.State == ConnectionState.Closed)
                {
                    command.Connection.Open();
                }

                try
                {
                    affectedRows = command.ExecuteNonQuery();
                }
                finally
                {
                    if (isConnected)
                    {
                        command.Connection.Close();
                    }
                }
            }

            return affectedRows;
        }

        public static async Task<int> ExecuteSpNonQueryAsync(this DbCommand command, CancellationToken token = default,
            bool manageConnection = true)
        {
            var affectedRows = -1;

            using (command)
            {
                if (command.Connection.State == ConnectionState.Closed)
                {
                    await command.Connection.OpenAsync(token).ConfigureAwait(false);
                }

                try
                {
                    affectedRows = await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
                }
                finally
                {
                    if (manageConnection)
                    {
                        command.Connection.Close();
                    }
                }
            }

            return affectedRows;
        }


        public static ExpandoObject Merge<TLeft, TRight>(this TLeft left, TRight right)
        {
            var expObj = new ExpandoObject();
            IDictionary<string, object> dict = expObj;
            foreach (var p in typeof(TLeft).GetProperties())
                dict[p.Name] = p.GetValue(left);
            foreach (var p in typeof(TRight).GetProperties())
                dict[p.Name] = p.GetValue(right);
            return expObj;
        }

        public static List<dynamic> MergeList<TLeft, TRight>(this IList<TLeft> left, IList<TRight> right)
        {
            var lst = new List<dynamic>();
            lst.Add(left);
            lst.Add(right);
            return lst;
        }
    }
}


