using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Slider.WorkDone.Data.ElasticScale
{
	public class DbFacility
	{
		private readonly string multiversePassword;
		private readonly string multiverseUserId;

		public DbFacility(string multiverseUserId, string multiversePassword)
		{
			if (multiverseUserId == null)
			{
				throw new ArgumentNullException(nameof(multiverseUserId));
			}
			if (multiversePassword == null)
			{
				throw new ArgumentNullException(nameof(multiversePassword));
			}
			this.multiverseUserId = multiverseUserId;
			this.multiversePassword = multiversePassword;
		}

		public async Task<bool> Exists(string server, string dbName)
		{
			if (string.IsNullOrWhiteSpace(server))
			{
				throw new ArgumentNullException(nameof(server));
			}
			if (string.IsNullOrWhiteSpace(dbName))
			{
				throw new ArgumentNullException(nameof(dbName));
			}
			using (var conn = new SqlConnection(GetConnectionString(server, "master")))
			{
				conn.Open();
				return await Exists(conn, dbName);
			}
		}

		private static async Task<bool> Exists(SqlConnection conn, string dbName)
		{
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = "select count(*) from sys.databases where name = @dbname";
				cmd.Parameters.AddWithValue("@dbname", dbName);
				cmd.CommandTimeout = 60;
				var count = await cmd.ExecuteScalarAsync();
				if (count != null)
				{
					return Convert.ToInt32(count) > 0;
				}
				return false;
			}
		}

		public async Task<bool> IsOnline(string server, string dbName)
		{
			if (string.IsNullOrWhiteSpace(server))
			{
				throw new ArgumentNullException(nameof(server));
			}
			if (string.IsNullOrWhiteSpace(dbName))
			{
				throw new ArgumentNullException(nameof(dbName));
			}
			using (var conn = new SqlConnection(GetConnectionString(server, "master")))
			{
				conn.Open();
				return await IsOnline(conn, dbName);
			}
		}

		private static async Task<bool> IsOnline(SqlConnection conn, string dbName)
		{
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = "select count(*) from sys.databases where name = @dbname and state = 0"; // online
				cmd.Parameters.AddWithValue("@dbname", dbName);
				cmd.CommandTimeout = 60;
				var count = await cmd.ExecuteScalarAsync();
				if (count != null)
				{
					return Convert.ToInt32(count) > 0;
				}
				return false;
			}
		}

		public async Task<bool> CreateDatabase(string server, string db, string dbEdition = "Basic", bool waitIsOnLine = false, Action<string> stepOverCallBack = null)
		{
			if (string.IsNullOrWhiteSpace(server))
			{
				throw new ArgumentNullException(nameof(server));
			}
			if (string.IsNullOrWhiteSpace(db))
			{
				throw new ArgumentNullException(nameof(db));
			}
			var callBack = stepOverCallBack ?? (x => { });
			callBack($"Creando DB '{db}'...");
			try
			{
				using (var conn = new SqlConnection(GetConnectionString(server, "master")))
				{
					conn.Open();
					var engineEdition = await GetServerEngineEdition(conn);
					if (engineEdition != 5)
					{
						// NO Azure SQL DB
						callBack($"El DB '{db}' no está en Azure.");
						return false;
					}
					if (await Exists(conn, db))
					{
						callBack($"El db '{db}' ya existe.");
						return true;
					}
					using (var cmd = conn.CreateCommand())
					{
						cmd.CommandText = $"CREATE DATABASE [{db}] (SERVICE_OBJECTIVE = '{dbEdition}')";
						cmd.CommandTimeout = 60;
						cmd.ExecuteNonQuery();
					}
					if (waitIsOnLine)
					{
						while (!await IsOnline(conn, db))
						{
							callBack($"Esperando que el db '{db}' esté online...");
							await Task.Delay(5000);
						}
						callBack($"El db '{db}' está listo para usar.");
					}
				}
			}
			catch (Exception e)
			{
				callBack($"Hubo un problema tratando de crear el db '{db}'.\n> {e.Message}");
				return false;
			}
			return true;
		}

		public async Task Verify(string server, string db, Func<Stream> shardInitializerStript, Action<string> stepOverCallBack = null, string dbEdition = "Basic")
		{
			if (await Exists(server, db))
			{
				return;
			}
			if (shardInitializerStript == null)
			{
				throw new ArgumentNullException(nameof(shardInitializerStript));
			}
			await CreateDatabase(server, db, dbEdition, true, stepOverCallBack);
			var connectionstring = GetConnectionString(server, db);
			await ExecuteSqlScript(connectionstring, shardInitializerStript());
		}

		private static async Task<int> GetServerEngineEdition(SqlConnection conn)
		{
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = "SELECT SERVERPROPERTY('EngineEdition')";
				cmd.CommandTimeout = 60;
				var edition = await cmd.ExecuteScalarAsync();
				if (edition != null)
				{
					return Convert.ToInt32(edition);
				}
				return -1;
			}
		}

		/// <summary>
		///   Returns a connection string that can be used to connect to the specified server and database.
		/// </summary>
		public string GetConnectionString(string serverName, string database)
		{
			var connStr = new SqlConnectionStringBuilder(GetCredentialsConnectionString())
			{
				DataSource = serverName,
				InitialCatalog = database
			};
			return connStr.ToString();
		}

		/// <summary>
		///   Returns a connection string to use for Data-Dependent Routing and Multi-Shard Query,
		///   which does not contain DataSource or InitialCatalog.
		/// </summary>
		public string GetCredentialsConnectionString()
		{
			var connStr = new SqlConnectionStringBuilder
			{
				UserID = multiverseUserId ?? string.Empty,
				Password = multiversePassword ?? string.Empty,
				IntegratedSecurity = false,
				ApplicationName = "Slider_WorkdoneV1.0",
				ConnectTimeout = 60
			};
			return connStr.ToString();
		}

		public static async Task ExecuteSqlScript(string connectionString, Stream script)
		{
			using (var conn = new SqlConnection(connectionString))
			{
				await conn.OpenAsync();
				using (var tx = conn.BeginTransaction())
				{
					var commands = GetCommandsFromScript(script);
					foreach (var command in commands)
					{
						using (var cmd = conn.CreateCommand())
						{
							cmd.Transaction = tx;
							cmd.CommandText = command;
							cmd.CommandTimeout = 60;
							await cmd.ExecuteNonQueryAsync();
						}
					}
					tx.Commit();
				}
			}
		}

		public static Task ExecuteSqlScript(string connectionString, string schemaFilePath)
		{
			return ExecuteSqlScript(connectionString, File.OpenRead(schemaFilePath));
		}

		private static IEnumerable<string> GetCommandsFromScript(Stream scriptStream)
		{
			var commands = new List<string>();
			using (TextReader tr = new StreamReader(scriptStream))
			{
				var sb = new StringBuilder();
				string line;
				while ((line = tr.ReadLine()) != null)
				{
					if (line.Trim() == "GO")
					{
						commands.Add(sb.ToString());
						sb.Clear();
					}
					else
					{
						sb.AppendLine(line);
					}
				}
			}
			return commands;
		}
	}
}