using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;
using Reminder.Storage.SqlServer.EF.Context;
using System.Reflection;

namespace Reminder.Storage.SqlServer.EF.DbInit
{
	public class DesignTimeReminderStorageDbContextFactory : IDesignTimeDbContextFactory<ReminderStorageContext>
	{
		public ReminderStorageContext CreateDbContext(string[] args)
		{
			string connectionString = ConnectionStringFactory.GetDbConnectionString();
			var migrationAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;

			var builder = new DbContextOptionsBuilder<ReminderStorageContext>();
			builder.UseSqlServer(connectionString, ob => ob.MigrationsAssembly(migrationAssembly));

			return new ReminderStorageContext(builder.Options);
		}
	}
}
