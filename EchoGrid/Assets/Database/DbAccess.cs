﻿using UnityEngine;
using System;
using System.Collections;
using Mono.Data.Sqlite;

public class DbAccess
{
	private SqliteConnection dbConnection;	
	private SqliteCommand dbCommand;
	private SqliteDataReader reader;
	
	public DbAccess (string connectionString)
	{	
		OpenDB (connectionString);	
	}
	
	public DbAccess ()
	{
		
	}
	
	public void OpenDB (string connectionString)
	{
		try
		{
			dbConnection = new SqliteConnection (connectionString);
			
			dbConnection.Open ();
			
			Debug.Log ("Connected to db");
		}
		catch(Exception e)
		{
			string temp1 = e.ToString();
			Debug.Log(temp1);
		}
	}
	
	public void CloseSqlConnection ()
	{
		if (dbCommand != null) {		
			dbCommand.Dispose ();
		}
		
		dbCommand = null;
		
		if (reader != null) {
			reader.Dispose ();
		}
		
		reader = null;
		
		if (dbConnection != null) {
			dbConnection.Close ();
		}
		
		dbConnection = null;
		
		Debug.Log ("Disconnected from db.");
	}
	
	/* Executes an SQL query 
	   @sqlQuery input must be properly formatted query
	 */
	public SqliteDataReader ExecuteQuery (string sqlQuery)	
	{
		dbCommand = dbConnection.CreateCommand ();	
		dbCommand.CommandText = sqlQuery;
		reader = dbCommand.ExecuteReader ();
		
		return reader;
	}
	
	public SqliteDataReader ReadFullTable (string tableName)
	{
		string query = "SELECT * FROM " + tableName;
		
		return ExecuteQuery (query);	
	}
	
	/* Method to insert an item into the database */
	public SqliteDataReader InsertInto (string tableName, string[] values)
	{
		string query = "INSERT INTO " + tableName + " VALUES (" + values[0];
		
		for (int i = 1; i < values.Length; ++i) {
			query += ", " + values[i];
		}
		
		query += ")";
		
		return ExecuteQuery (query);	
	}
	
	/* Method to update an item in the database */
	public SqliteDataReader UpdateInto (string tableName, string []cols,string []colsvalues,string selectkey,string selectvalue)
	{
		string query = "UPDATE "+tableName+" SET "+cols[0]+" = "+colsvalues[0];
		
		for (int i = 1; i < colsvalues.Length; ++i) {
			query += ", " +cols[i]+" ="+ colsvalues[i];
		}
		
		query += " WHERE "+selectkey+" = "+selectvalue+" ";
		
		return ExecuteQuery (query);
	}
	
	/* Method to delete an item from the database */
	public SqliteDataReader Delete(string tableName,string []cols,string []colsvalues)
	{
		string query = "DELETE FROM "+tableName + " WHERE " +cols[0] +" = " + colsvalues[0];
		
		for (int i = 1; i < colsvalues.Length; ++i) {
			query += " or " +cols[i]+" = "+ colsvalues[i];
		}
		Debug.Log(query);
		return ExecuteQuery (query);
	}
	
	/* Insert field item into a specific row in a table */
	public SqliteDataReader InsertIntoSpecific (string tableName, string[] cols, string[] values)
	{
		if (cols.Length != values.Length) {		
			throw new SqliteException ("columns.Length != values.Length");
		}
		
		string query = "INSERT INTO " + tableName + "(" + cols[0];
		
		for (int i = 1; i < cols.Length; ++i) {
			query += ", " + cols[i];
		}
		
		query += ") VALUES (" + values[0];
		
		for (int i = 1; i < values.Length; ++i) {
			query += ", " + values[i];
		}
		
		query += ")";
		
		return ExecuteQuery (query);
		
	}
	
	/* Delete a table from the database */
	public SqliteDataReader DeleteContents (string tableName)
	{
		string query = "DELETE FROM " + tableName;
		
		return ExecuteQuery (query);
	}
	
	/* Create a new table in the database */
	public SqliteDataReader CreateTable (string name, string[] col, string[] colType)
	{
		if (col.Length != colType.Length) {		
			throw new SqliteException ("columns.Length != colType.Length");
		}
		
		string query = "CREATE TABLE " + name + " (" + col[0] + " " + colType[0];
		
		for (int i = 1; i < col.Length; ++i) {
			query += ", " + col[i] + " " + colType[i];
		}
		
		query += ")";
		
		return ExecuteQuery (query);
	}
	
	/* Search an item in the database and print to console */
	public SqliteDataReader searchItem (string tableName, string[] items, string[] col, string[] operation, string[] values)
	{	
		if (col.Length != operation.Length || operation.Length != values.Length) {		
			throw new SqliteException ("col.Length != operation.Length != values.Length");
		}
		
		string query = "SELECT " + items[0];
		
		for (int i = 1; i < items.Length; ++i) {
			query += ", " + items[i];
		}
		
		query += " FROM " + tableName + " WHERE " + col[0] + operation[0] + "'" + values[0] + "' ";
		
		for (int i = 1; i < col.Length; ++i) {
			query += " AND " + col[i] + operation[i] + "'" + values[0] + "' ";
		}
		
		return ExecuteQuery (query);
	}
	
}