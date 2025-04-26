using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace OracleLib;

public class OracleLib
{
    private readonly string _connectionString;
    private readonly OracleConnection _connection;

    public OracleLib(string connectionString)
    {
        ArgumentException.ThrowIfNullOrEmpty(connectionString, nameof(connectionString));
        
        _connectionString = connectionString;
        _connection = new OracleConnection(_connectionString);
    }

    public void OpenConnection()
    {
        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }
    }

    public void CloseConnection()
    {
        if (_connection.State != ConnectionState.Closed)
        {
            _connection.Close();
        }
    }

    public DataTable ExecuteQuery(string query, params OracleParameter[] parameters)
    {
        try
        {
            OpenConnection();
            using (OracleCommand command = new OracleCommand(query, _connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                DataTable dataTable = new DataTable();
                using (OracleDataAdapter adapter = new OracleDataAdapter(command))
                {
                    adapter.Fill(dataTable);
                }
                return dataTable;
            }
        }
        finally
        {
            CloseConnection();
        }
    }

    public int ExecuteNonQuery(string query, params OracleParameter[] parameters)
    {
        try
        {
            OpenConnection();
            using (OracleCommand command = new OracleCommand(query, _connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }
                return command.ExecuteNonQuery();
            }
        }
        finally
        {
            CloseConnection();
        }
    }

    public object ExecuteScalar(string query, params OracleParameter[] parameters)
    {
        try
        {
            OpenConnection();
            using (OracleCommand command = new OracleCommand(query, _connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }
                return command.ExecuteScalar();
            }
        }
        finally
        {
            CloseConnection();
        }
    }

    // 트랜잭션 처리를 위한 메서드
    public void ExecuteTransaction(Action<OracleTransaction> transactionAction)
    {
        try
        {
            OpenConnection();
            using (OracleTransaction transaction = _connection.BeginTransaction())
            {
                try
                {
                    transactionAction(transaction);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        finally
        {
            CloseConnection();
        }
    }
}
