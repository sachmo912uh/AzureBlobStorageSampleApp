using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Polly;
using SQLite;
using Xamarin.Essentials;

namespace AzureBlobStorageSampleApp
{
	public abstract class BaseDatabase
	{
		static readonly string DatabasePath = Path.Combine(FileSystem.AppDataDirectory, $"{nameof(AzureBlobStorageSampleApp)}.db3");

		static readonly Lazy<SQLiteAsyncConnection> _databaseConnectionHolder =
			new Lazy<SQLiteAsyncConnection>(() => new SQLiteAsyncConnection(DatabasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache));

		static SQLiteAsyncConnection DatabaseConnection => _databaseConnectionHolder.Value;

		protected static async ValueTask<SQLiteAsyncConnection> GetDatabaseConnection<T>()
		{
			if (!DatabaseConnection.TableMappings.Any(x => x.MappedType.Name == typeof(T).Name))
			{
				await DatabaseConnection.EnableWriteAheadLoggingAsync().ConfigureAwait(false);
				await DatabaseConnection.CreateTablesAsync(CreateFlags.None, typeof(T)).ConfigureAwait(false);
			}

			return DatabaseConnection;
		}

		protected static Task<T> AttemptAndRetry<T>(Func<Task<T>> action, int numRetries = 3)
		{
			return Policy.Handle<SQLiteException>().WaitAndRetryAsync(numRetries, pollyRetryAttempt).ExecuteAsync(action);

			TimeSpan pollyRetryAttempt(int attemptNumber) => TimeSpan.FromSeconds(Math.Pow(2, attemptNumber));
		}
	}
}
