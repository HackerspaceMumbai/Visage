using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;
using Visage.Models;

namespace Visage.Data
{
    public class VisageDatabase
    {
        readonly SQLiteAsyncConnection database;
        
        public VisageDatabase(string dbPath)
        {
			database = new SQLiteAsyncConnection(dbPath);
			database.CreateTableAsync<Profile>().Wait();
        }

        public Task<Profile> GetProfile()
		{
            return database.Table<Profile>().FirstOrDefaultAsync();
		}

		public Task<int> SaveItemAsync(Profile item)
		{
			if (item.Id != 0)
			{
				return database.UpdateAsync(item);
			}
			else
			{
				return database.InsertAsync(item);
			}
		}

        public Task<int> DeleteItemAsync(Profile item)
		{
			return database.DeleteAsync(item);
		}

        public async Task<bool> ProfileExists()
		{
            var count = await database.Table<Profile>().CountAsync();

            if (count > 0)
                return true;

            return false;
		}
    }
}
