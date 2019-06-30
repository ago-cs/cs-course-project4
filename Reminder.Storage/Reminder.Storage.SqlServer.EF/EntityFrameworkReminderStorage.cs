﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Reminder.Storage.Core;
using Reminder.Storage.SqlServer.EF.Context;

namespace Reminder.Storage.SqlServer.EF
{
	public class EntityFrameworkReminderStorage : IReminderStorage
	{
		public static readonly LoggerFactory MyConsoleLoggerFactory =
			new LoggerFactory(new[]
			{
				new ConsoleLoggerProvider(
					(category, level) =>
						category == DbLoggerCategory.Database.Command.Name
						&& level == LogLevel.Information,
					true)
			});


		private readonly DbContextOptionsBuilder<ReminderStorageContext> _builder;

		public EntityFrameworkReminderStorage(
			string connectionString,
			bool enableLogging = false)
		{
			_builder = new DbContextOptionsBuilder<ReminderStorageContext>()
				.UseSqlServer(connectionString);

			if (enableLogging)
			{
				_builder
					.UseLoggerFactory(MyConsoleLoggerFactory)
					.EnableSensitiveDataLogging(true);
			}
		}

		public int Count
		{
			get
			{
				using (var context = new ReminderStorageContext(_builder.Options))
				{
					return context.ReminderItems.Count();
				}
			}
		}

		public Guid Add(ReminderItemRestricted reminder)
		{
			var dto = new ReminderItemDto(reminder);
			using (var context = new ReminderStorageContext(_builder.Options))
			{
				context.ReminderItems.Add(dto);
				context.SaveChanges();
				return dto.Id;
			}
		}

		public ReminderItem Get(Guid id)
		{
			using (var context = new ReminderStorageContext(_builder.Options))
			{
				return context.ReminderItems
					.FirstOrDefault(ri => ri.Id == id)
					?.ToReminderItem();
			}
		}

		public List<ReminderItem> Get(int count = 0, int startPostion = 0)
		{
			using (var context = new ReminderStorageContext(_builder.Options))
			{
				if (count == 0 && startPostion == 0)
				{
					return context.ReminderItems
					.OrderBy(ri => ri.Id)
					.Select(ri => ri.ToReminderItem())
					.ToList();
				}

				if (count == 0)
				{
					return context.ReminderItems
					.OrderBy(ri => ri.Id)
					.Skip(startPostion)
					.Select(ri => ri.ToReminderItem())
					.ToList();
				}

				return context.ReminderItems
					.OrderBy(ri => ri.Id)
					.Skip(startPostion)
					.Take(count)
					.Select(ri => ri.ToReminderItem())
					.ToList();
			}
		}

		public List<ReminderItem> Get(ReminderItemStatus status, int count, int startPostion)
		{
			using (var context = new ReminderStorageContext(_builder.Options))
			{
				if (count == 0 && startPostion == 0)
				{
					return context.ReminderItems
					.Where(ri => ri.Status == status)
					.Select(ri => ri.ToReminderItem())
					.ToList();
				}

				if (count == 0)
				{
					return context.ReminderItems
					.Where(ri => ri.Status == status)
					.Skip(startPostion)
					.Select(ri => ri.ToReminderItem())
					.ToList();
				}

				return context.ReminderItems
					.Where(ri => ri.Status == status)
					.Skip(startPostion)
					.Take(count)
					.Select(ri => ri.ToReminderItem())
					.ToList();
			}
		}

		public List<ReminderItem> Get(ReminderItemStatus status)
		{
			using (var context = new ReminderStorageContext(_builder.Options))
			{
				return context.ReminderItems
					.OrderBy(ri => ri.Id)
					.Where(ri => ri.Status == status)
					.Select(ri => ri.ToReminderItem())
					.ToList();
			}
		}

		public bool Remove(Guid id)
		{
			using (var context = new ReminderStorageContext(_builder.Options))
			{
				var found = context.ReminderItems.Find(id) != null;
				if (found)
				{
					context.ReminderItems.Remove(new ReminderItemDto { Id = id });
					context.SaveChanges();
				}

				return found;
			}
		}

		public void UpdateStatus(IEnumerable<Guid> ids, ReminderItemStatus status)
		{
			using (var context = new ReminderStorageContext(_builder.Options))
			{
				var dtos = context.ReminderItems
				.Where(d => ids.Contains(d.Id))
				.ToList();

				foreach (var dto in dtos)
				{
					dto.Status = status;
				}

				context.SaveChanges();
			}
		}

		public void UpdateStatus(Guid id, ReminderItemStatus status)
		{
			using (var context = new ReminderStorageContext(_builder.Options))
			{
				var dto = context.ReminderItems.Find(id);
				dto.Status = status;
				context.SaveChanges();
			}
		}
	}
}
