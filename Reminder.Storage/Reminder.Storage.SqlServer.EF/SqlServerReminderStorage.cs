using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Reminder.Storage.Core;
using Reminder.Storage.SqlServer.EF.Context;

namespace Reminder.Storage.SqlServer.EF
{
	public class SqlServerReminderStorage: IReminderStorage
	{
		private readonly DbContextOptionsBuilder<ReminderStorageContext> _builder;

		public int Count
		{
			get
			{
				using var context = new ReminderStorageContext(_builder.Options);
				return context.ReminderItems.Count();
			}
		}

		public SqlServerReminderStorage(string connectionString)
		{
			//connectionString;
			_builder = new DbContextOptionsBuilder<ReminderStorageContext>()
				.UseSqlServer(connectionString);
		}

		public SqlServerReminderStorage(IConfiguration config)
			: this(config.GetConnectionString("DefaultConnection"))
		{
		}

		public Guid Add(ReminderItemRestricted reminder)
		{
			var dto = new ReminderItemDto(reminder);
			using var context = new ReminderStorageContext(_builder.Options);
			context.ReminderItems.Add(dto);
			context.SaveChanges();
			return dto.Id;
		}

		public bool Remove(Guid id)
		{
			using var context = new ReminderStorageContext(_builder.Options);
			var dto = context.ReminderItems.FirstOrDefault(r => r.Id == id);
			if (dto == null)
			{
				return false;
			}

			context.ReminderItems.Remove(dto);
			context.SaveChanges();
			return true;
		}

		public void Clear()
		{
			using var context = new ReminderStorageContext(_builder.Options);
			context.ReminderItems.RemoveRange(context.ReminderItems);
		}

		public ReminderItem Get(Guid id)
		{
			using var context = new ReminderStorageContext(_builder.Options);
			return context.ReminderItems
				.FirstOrDefault(r => r.Id == id)
				?.ToReminderItem();
		}

		public List<ReminderItem> Get(int count = 0, int startPosition = 0)
		{
			using var context = new ReminderStorageContext(_builder.Options);

			if (count == 0 && startPosition == 0)
			{
				return context.ReminderItems
					.Select(r => r.ToReminderItem())
					.ToList();
			}

			if (count == 0)
			{
				return context.ReminderItems
					.OrderBy(r => r.Id)
					.Skip(startPosition)
					.Select(r => r.ToReminderItem())
					.ToList();
			}

			return context.ReminderItems
				.OrderBy(r => r.Id)
				.Skip(startPosition)
				.Take(count)
				.Select(r => r.ToReminderItem())
				.ToList();
		}

		public List<ReminderItem> Get(ReminderItemStatus status, int count = 0, int startPosition = 0)
		{
			using var context = new ReminderStorageContext(_builder.Options);

			if (count == 0 && startPosition == 0)
			{
				return context.ReminderItems
					.Where(r => r.Status == status)
					.Select(r => r.ToReminderItem())
					.ToList();
			}

			if (count == 0)
			{
				return context.ReminderItems
					.Where(r => r.Status == status)
					.OrderBy(r => r.Id)
					.Skip(startPosition)
					.Select(r => r.ToReminderItem())
					.ToList();
			}

			return context.ReminderItems
				.Where(r => r.Status == status)
				.OrderBy(r => r.Id)
				.Skip(startPosition)
				.Take(count)
				.Select(r => r.ToReminderItem())
				.ToList();
		}

		public void UpdateStatus(IEnumerable<Guid> ids, ReminderItemStatus status)
		{
			using var context = new ReminderStorageContext(_builder.Options);

			var dtos = context.ReminderItems
				.Where(r => ids.Contains(r.Id))
				.ToList();

			foreach (var dto in dtos)
			{
				dto.Status = status;
			}

			context.SaveChanges();
		}

		public void UpdateStatus(Guid id, ReminderItemStatus status)
		{
			using var context = new ReminderStorageContext(_builder.Options);
			var dto = context.ReminderItems.Find(id);
			dto.Status = status;
			context.SaveChanges();
		}
	}
}
