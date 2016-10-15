using System.IO;

namespace Slider.WorkDone.Data.ElasticScale
{
	public class MultiverseConfiguration
	{
		private string shardMapManagerServerName;

		public string ShardMapManagerServerName
		{
			get { return shardMapManagerServerName ?? UniverseServerName; }
			set { shardMapManagerServerName = value; }
		}

		public string ShardMapManagerDatabaseName => "ShardMapManager";

		public string InitializeShardDbScriptPath => "Slider.WorkDone.Data.Initialize.sql";
		/// <summary>
		/// Gets the name for the per Tenant Shard Map that contains metadata.
		/// </summary>
		public string TenantsShardMapName => "TenantsShardMap";

		/// <summary>
		/// Gets the server name from the App.config file for shards to be created on.
		/// </summary>
		public string UniverseServerName { get; set; }

		public string UniverseUserId { get; set; }
		public string UniversePassword { get; set; }

		/// <summary>
		/// Gets the edition to use for Shards and Shard Map Manager Database if the server is an Azure SQL DB server. 
		/// If the server is a regular SQL Server then this is ignored.
		/// </summary>
		public string DefaultDatabaseEdition => "Basic";
	}
}