using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;

namespace Slider.WorkDone.Data.ElasticScale
{
	public class SmmFacility
	{
		private readonly MultiverseConfiguration conf;
		private readonly DbFacility dataBase;

		public SmmFacility(MultiverseConfiguration conf)
			: this(conf, new DbFacility(conf.UniverseUserId, conf.UniversePassword)) {}

		public SmmFacility(MultiverseConfiguration conf, DbFacility dbFacility)
		{
			if (conf == null)
			{
				throw new ArgumentNullException(nameof(conf));
			}
			this.conf = conf;
			dataBase = dbFacility;
		}

		public ShardMapManager GetShardMapManagerIfExists()
		{
			var shardMapManagerConnectionString = GetSmmConnectionString();
			// Get shard map manager database connection string
			// Try to get a reference to the Shard Map Manager in the Shard Map Manager database. If it doesn't already exist, then create it.
			ShardMapManager shardMapManager;
			var shardMapManagerExists = ShardMapManagerFactory.TryGetSqlShardMapManager(
				shardMapManagerConnectionString,
				ShardMapManagerLoadPolicy.Lazy,
				out shardMapManager);

			return !shardMapManagerExists ? null : shardMapManager;
		}

		/// <summary>
		///   Creates a shard map manager in the database specified by the given connection string.
		/// </summary>
		public ShardMapManager CreateOrGetShardMapManager()
		{
			var shardMapManager = GetShardMapManagerIfExists();

			if (shardMapManager != null)
			{
				return shardMapManager;
			}
			// The Shard Map Manager does not exist, so create it
			var shardMapManagerConnectionString = GetSmmConnectionString();
			shardMapManager = ShardMapManagerFactory.CreateSqlShardMapManager(shardMapManagerConnectionString);

			return shardMapManager;
		}

		public async Task CreateShardMapDb(Action<string> stepOverCallBack)
		{
			// Create shard map manager database
			await dataBase.CreateDatabase(conf.ShardMapManagerServerName, conf.ShardMapManagerDatabaseName, "Basic", false, stepOverCallBack);
		}

		/// <summary>
		///   Creates a new Range Shard Map with the specified name, or gets the Range Shard Map if it already exists.
		/// </summary>
		public RangeShardMap<T> CreateOrGetRangeShardMap<T>(ShardMapManager shardMapManager, string shardMapName)
		{
			RangeShardMap<T> shardMap;
			var shardMapExists = shardMapManager.TryGetRangeShardMap(shardMapName, out shardMap);
			if (!shardMapExists)
			{
				shardMap = shardMapManager.CreateRangeShardMap<T>(shardMapName);
			}
			return shardMap;
		}

		/// <summary>
		///   Creates a new List Shard Map with the specified name, or gets the List Shard Map if it already exists.
		/// </summary>
		public ListShardMap<T> CreateOrGetListShardMap<T>(ShardMapManager shardMapManager, string shardMapName)
		{
			ListShardMap<T> shardMap;
			var shardMapExists = shardMapManager.TryGetListShardMap(shardMapName, out shardMap);
			if (!shardMapExists)
			{
				shardMap = shardMapManager.CreateListShardMap<T>(shardMapName);
			}
			return shardMap;
		}

		/// <summary>
		///   Adds Shards to the Shard Map, or returns them if they have already been added.
		/// </summary>
		public Shard CreateOrGetShard(ShardMap shardMap, ShardLocation shardLocation)
		{
			Shard shard;
			var shardExists = shardMap.TryGetShard(shardLocation, out shard);
			if (!shardExists)
			{
				shard = shardMap.CreateShard(shardLocation);
			}
			return shard;
		}

		public async Task<PointMapping<Guid>> CreateShard(ListShardMap<Guid> shardMap, Guid point, string dbName, Func<Stream> shardInitializerStript, Action<string> stepOverCallBack, string dbEdition = "Basic")
		{
			var callBack = stepOverCallBack ?? (x => { });
			if (string.IsNullOrWhiteSpace(dbName))
			{
				callBack("No se pudo determinar el nombre de la location del shard (dbname).");
			}
			await dataBase.Verify(conf.UniverseServerName, dbName, shardInitializerStript, callBack, dbEdition);

			callBack("Verificando inicialización del shard.");
			var shardLocation = new ShardLocation(conf.UniverseServerName, dbName);
			var shard = CreateOrGetShard(shardMap, shardLocation);
			return shardMap.CreatePointMapping(point, shard);
		}

		public async Task MoveShard(ListShardMap<Guid> shardMap, Guid point, string newDbName, Func<Stream> shardInitializerStript, Action<string> stepOverCallBack, string dbEdition = "Basic")
		{
			var callBack = stepOverCallBack ?? (x => { });
			if (string.IsNullOrWhiteSpace(newDbName))
			{
				callBack("No se pudo determinar el nombre de la location del shard (dbname).");
			}
			await dataBase.Verify(conf.UniverseServerName, newDbName, shardInitializerStript, callBack, dbEdition);
			PointMapping<Guid> mapping;
			if (!shardMap.TryGetMappingForKey(point, out mapping))
			{
				return;
			}

			var shardLocation = new ShardLocation(conf.UniverseServerName, newDbName);
			var shard = CreateOrGetShard(shardMap, shardLocation);
			shardMap.UpdateMapping(mapping, new PointMappingUpdate {Shard = shard});
		}

		private string GetSmmConnectionString()
		{
			return dataBase.GetConnectionString(conf.ShardMapManagerServerName, conf.ShardMapManagerDatabaseName);
		}
	}
}