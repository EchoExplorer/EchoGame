using UnityEngine;
using System;
using System.Collections;
using System.Text;
using Mono.Data.Sqlite;
using System.Data;
using System.Data.Common;

/// <summary>
/// Class that manages a connection to an SQL database.
/// </summary>
/// <remarks>
/// Supported operations include:
/// * Opening and closing a connection
/// * Executing SQL queries
/// * Generating SQL queries with various support methods
/// </remarks>
public class DbAccess: IDisposable
{
    private SqliteConnection dbConnection;

    /// <summary>
    /// <para>Constructs the instance with a specified connection string.</para>
    /// <para>The format of this string should match that of the constructor for Mono.Data.Sqlite.SqliteConnection.</para>
    /// </summary>
    /// <param name="connectionString">A string describing the database to connect to.</param>
    /// <exception cref="InvalidOperationException">Thrown when a connection cannot be made.</exception>
    public DbAccess(string connectionString)
    {
        try
        {
            dbConnection = new SqliteConnection(connectionString);
           // dbConnection.Open();
           // This open call seems to cause problems. Research into it?ty
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Database connection failure", e);
        }
    }

    /// <summary>
    /// Terminates the connection to the database and cleans up.
    /// </summary>
    public void Dispose()
    {
        if (dbConnection != null)
        {
            dbConnection.Close();
        }
        else
        {
            throw new ObjectDisposedException("Connection is already closed");
        }
        dbConnection = null;

        // Logging.Log("Disconnected from db.", Logging.LogLevel.NORMAL);
    }

    /// <summary>
    /// <para>Creates an ``SqliteCommand`` instance from the SQL query.</para>
    /// <para>This function cannot guard against SQL injection, so use with caution.</para>
    /// </summary>
    /// <param name="sqlQuery">The SQL query to build from</param>
    /// <returns>The ``SqliteCommand``instance created from the query</returns>
    public SqliteCommand CreateCommand(string sqlQuery)
    {
        SqliteCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = sqlQuery;
        Logging.Log("Created SQL query: " + sqlQuery, Logging.LogLevel.LOW_PRIORITY);
        return dbCommand;
    }

    /// <summary>
    /// Executes the command and disposes it for the user.
    /// </summary>
    /// <param name="command">The ``SqliteCommand`` to execute</param>
    public void ExecuteAndDispose(SqliteCommand command)
    {
        using (command)
        {
            command.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Reads the full contents of some table in the database.
    ///  The table name is not sanitized so use with caution.
    /// </summary>
    /// <param name="tableName">The name of the table in the database</param>
    /// <returns>An ``SqliteCommand`` that runs the query</returns>
    public SqliteCommand ReadFullTable(string tableName)
    {
        string query = "SELECT * FROM " + tableName;

        return CreateCommand(query);
    }

    /// <summary>
    /// Method to insert items into some table in the database.
    ///  Values are sanitized, but the table name is not sanitized so use with caution.
    /// </summary>
    /// <param name="tableName">The name of the table in the database</param>
    /// <param name="values">The items to insert</param>
    /// <returns>An ``SqliteCommand`` that runs the query</returns>
    /// <exception cref="ArgumentException">Thrown when the array of values is empty.</exception>
    public SqliteCommand InsertInto(string tableName, object[] values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException();
        }

        StringBuilder query = new StringBuilder().Append("INSERT INTO " + tableName + " VALUES (");
        query.Append("?, ", 0, values.Length - 1);
        query.Append("?)");
        SqliteCommand command = new SqliteCommand(query.ToString(), dbConnection);

        foreach (object value in values)
        {
            command.Parameters.Add(value);
        }

        return command;
    }

    /* Method to update an item in the database
     * TODO: Implement this function.
    public SqliteCommand UpdateInto(string tableName, string[] cols, string[] colsvalues, string selectkey, string selectvalue)
    {
        throw new NotImplementedException();
        string query = "UPDATE " + tableName + " SET " + cols[0] + " = " + colsvalues[0];

        for (int i = 1; i < colsvalues.Length; ++i)
        {
            query += ", " + cols[i] + " =" + colsvalues[i];
        }

        query += " WHERE " + selectkey + " = " + selectvalue + " ";

        return CreateCommand(query);
    }

    /* Method to delete an item from the database
     * TODO: Implement this function.
    public SqliteCommand Delete(string tableName, string[] cols, string[] colsvalues)
    {
        throw new NotImplementedException();
        string query = "DELETE FROM " + tableName + " WHERE " + cols[0] + " = " + colsvalues[0];

        for (int i = 1; i < colsvalues.Length; ++i)
        {
            query += " or " + cols[i] + " = " + colsvalues[i];
        }
        Debug.Log(query);
        return CreateCommand(query);
    }

    /* Insert field item into a specific row in a table
     * TODO: Implement this function.
    public SqliteCommand InsertIntoSpecific(string tableName, string[] cols, string[] values)
    {
        throw new NotImplementedException();
        if (cols.Length != values.Length)
        {
            throw new SqliteException("columns.Length != values.Length");
        }

        string query = "INSERT INTO " + tableName + "(" + cols[0];

        for (int i = 1; i < cols.Length; ++i)
        {
            query += ", " + cols[i];
        }

        query += ") VALUES (" + values[0];

        for (int i = 1; i < values.Length; ++i)
        {
            query += ", " + values[i];
        }

        query += ")";

        return CreateCommand(query);

    }

    /* Delete a table from the database
     * TODO: Implement this function.
    public SqliteCommand DeleteContents(string tableName)
    {
        throw new NotImplementedException();
        string query = "DELETE FROM " + tableName;

        return CreateCommand(query);
    }

    /* Create a new table in the database
     * TODO: Implement this function.
    public SqliteCommand CreateTable(string name, string[] col, string[] colType)
    {
        throw new NotImplementedException();
        if (col.Length != colType.Length)
        {
            throw new ArgumentException("columns.Length != colType.Length");
        }

        string query = "CREATE TABLE " + name + " (" + col[0] + " " + colType[0];

        for (int i = 1; i < col.Length; ++i)
        {
            query += ", " + col[i] + " " + colType[i];
        }

        query += ")";

        return CreateCommand(query);
    }

    /* Search an item in the database and print to console
     * TODO: Implement this function.
    public SqliteCommand searchItem(string tableName, string[] items, string[] col, string[] operation, string[] values)
    {
        throw new NotImplementedException();
        if (col.Length != operation.Length || operation.Length != values.Length)
        {
            throw new SqliteException("col.Length != operation.Length != values.Length");
        }

        string query = "SELECT " + items[0];

        for (int i = 1; i < items.Length; ++i)
        {
            query += ", " + items[i];
        }

        query += " FROM " + tableName + " WHERE " + col[0] + operation[0] + "'" + values[0] + "' ";

        for (int i = 1; i < col.Length; ++i)
        {
            query += " AND " + col[i] + operation[i] + "'" + values[0] + "' ";
        }

        return CreateCommand(query);
    } */

    // Also what in the world, the original code was copied elsewhere.
    // Found an exact match from http://www.itwendao.com/article/detail/381506.html. This is irresponsible.
}