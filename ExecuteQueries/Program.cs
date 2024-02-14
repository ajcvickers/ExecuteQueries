using System.Diagnostics;
using CommandLine;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.Extensions.Logging;
using Npgsql;

var options = Parser.Default.ParseArguments<Options>(args).Value;

if (options.Init)
{
	using (var context = new SomeDbContext())
	{
		await context.Database.EnsureDeletedAsync();
		await context.Database.EnsureCreatedAsync();

		for (int i = 0; i < 1000; i++)
		{
			context.Add(new Foo()
			{
				A = 1,
				B = 2,
				C = 3,
				D = 8,
				E = 1212,
				F = 1,
				H = 12,
				I = 11,
			});
		}

		await context.SaveChangesAsync();
	}
}
else
{
	if (!options.Worker)
	{
		//Console.WriteLine($"Async = {options.Async}; Queries = {(options.Queries == 0 ? 1000 : options.Queries)}");
		
		for (var i = 0; i < options.Processes; i++)
		{
			//Console.WriteLine("Process.Start(\"ExecuteQueries.exe\", args.Concat([\"-w\"]).ToArray());");
			Process.Start("ExecuteQueries.exe", args.Concat(["-w"]).ToArray());
		}
	}
	else
	{
		var threadCount = options.Threads / options.Processes;
		Console.WriteLine("Starting...");
		var stopwatch = Stopwatch.StartNew();

		var tasks = new List<Task>();
		for (int a = 0; a < threadCount; a++)
		{
			tasks.Add(ExecuteQueryAdo());
			//tasks.Add(JustDoSomeStuff());
		}

		await Task.WhenAll(tasks);

		var done = stopwatch.ElapsedMilliseconds;

		Console.WriteLine($"Done in {done} ms.");
	}
}

// void ExecuteQuery()
// {
// 	// Console.WriteLine("  Query...");
// 	// var stopwatch = Stopwatch.StartNew();
// 	for (var i = 0; i < 1000; i++)
// 	{
// 		var random = new Random();
// 		using (var context = new SomeDbContext())
// 		{
// 			IQueryable<Foo> query = context.Foos;
//
// 			//query = query.Where(e => e.A > 10);
// 			// for (var j = 0; j < 16; j++)
// 			// {
// 			// 	var whereIndex = random.Next(0, 15);
// 			// 	if (whereIndex < 11)
// 			// 	{
// 			// 		var next = random.Next();
// 			// 		query = query.Where(e => EF.Property<int>(e, ((char)('A' + whereIndex)).ToString()) == next);
// 			// 	}
// 			// }
//
// 			var results = query.ToList();
// 		}
// 	}
// 	// var done = stopwatch.ElapsedMilliseconds;
// 	//
// 	// Console.WriteLine($"  Done in {done} ms.");
// }

// async Task ExecuteQueryAsync()
// {
// 	// Console.WriteLine("  Query...");
// 	// var stopwatch = Stopwatch.StartNew();
// 	for (var i = 0; i < 1000; i++)
// 	{
// 		var random = new Random();
// 		using (var context = new SomeDbContext())
// 		{
// 			IQueryable<Foo> query = context.Foos;
// 			// for (var j = 0; j < 16; j++)
// 			// {
// 			// 	var whereIndex = random.Next(0, 15);
// 			// 	if (whereIndex < 11)
// 			// 	{
// 			// 		var next = random.Next();
// 			// 		query = query.Where(e => EF.Property<int>(e, ((char)('A' + whereIndex)).ToString()) == next);
// 			// 	}
// 			// }
//
// 			var results = await query.ToListAsync();
// 		}
// 	}
// 	// var done = stopwatch.ElapsedMilliseconds;
// 	//
// 	// Console.WriteLine($"  Done in {done} ms.");
// }

async Task ExecuteQueryAdo()
{
	var count = options.Queries == 0 ? 1000 : options.Queries;
	for (var i = 0; i < count; i++)
	{
		//using var connection = new NpgsqlConnection("Server=localhost;Database=One;User ID=postgres;Password=clippy77i@");
		// using var connection = new SqlConnection("Data Source=localhost;Database=One;Integrated Security=True;Trust Server Certificate=True");
		using var connection = new SqliteConnection("Data Source=db.dat");
		if (options.Async)
		{
			await connection.OpenAsync();
		}
		else
		{
			connection.Open();
		}
		
		using var command = connection.CreateCommand();
		command.CommandText =
			"""
			SELECT f."Id", f."A", f."B", f."C", f."D", f."E", f."F", f."G", f."H", f."I", f."J", f."K"
			FROM "Foos" AS f
			""";

		var results = new List<Foo>();
		if (options.Async)
		{
			await using var reader = await command.ExecuteReaderAsync();
			while (await reader.ReadAsync())
			{
				var foo = new Foo
				{
					Id = reader.GetInt32(0),
					A = reader.GetInt32(1),
					B = reader.GetInt32(2),
					C = reader.GetInt32(3),
					D = reader.GetInt32(4),
					E = reader.GetInt32(5),
					F = reader.GetInt32(6),
					G = reader.GetInt32(7),
					H = reader.GetInt32(8),
					I = reader.GetInt32(9),
					J = reader.GetInt32(10),
					K = reader.GetInt32(11),
				};
				
				results.Add(foo);
			}

			await connection.CloseAsync();
		}
		else
		{
			using var reader = command.ExecuteReader();
			while (reader.Read())
			{
				var foo = new Foo
				{
					Id = reader.GetInt32(0),
					A = reader.GetInt32(1),
					B = reader.GetInt32(2),
					C = reader.GetInt32(3),
					D = reader.GetInt32(4),
					E = reader.GetInt32(5),
					F = reader.GetInt32(6),
					G = reader.GetInt32(7),
					H = reader.GetInt32(8),
					I = reader.GetInt32(9),
					J = reader.GetInt32(10),
					K = reader.GetInt32(11),
				};
				
				results.Add(foo);
			}

			connection.Close();
		}
	}
}

// Task JustDoSomeStuff()
// {
// 	var count = options.Queries == 0 ? 1000 : options.Queries;
// 	for (var i = 0; i < count; i++)
// 	{
// 		var bytes = new byte[10000];
// 		using var stream = File.Open("Microsoft.CodeAnalysis.CSharp.dll", FileMode.Open, FileAccess.Read, FileShare.Read);
// 		while (stream.Read(bytes, 0, bytes.Length) != 0)
// 		{
// 		}
// 		
// 		// byte[] bytes = options.Async
// 		// 	? await File.ReadAllBytesAsync("Microsoft.CodeAnalysis.CSharp.dll")
// 		// 	: File.ReadAllBytes("Microsoft.CodeAnalysis.CSharp.dll");
//
// 		// for (int j = 1; j < bytes.Length; j++)
// 		// {
// 		// 	bytes[j] = (byte)((3 * Math.PI) + bytes[j - 1]);
// 		// }
// 	}
// 	
// 	return Task.CompletedTask;
// }

public class SomeDbContext : DbContext
{
	public DbSet<Foo> Foos => Set<Foo>();
	
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		=> optionsBuilder
			//.UseSqlServer(@"Data Source=localhost;Database=One;Integrated Security=True;Trust Server Certificate=True")
			.UseSqlite(@"Data Source=db.dat")
			//.UseNpgsql("Server=localhost;Database=One;User ID=postgres;Password=clippy77i@")
			//.LogTo(Console.WriteLine, LogLevel.Information)
			.EnableSensitiveDataLogging();
	
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
	}
}

public class Foo
{
	public int Id { get; set; }
	public int A { get; set; }
	public int B { get; set; }
	public int C { get; set; }
	public int D { get; set; }
	public int E { get; set; }
	public int F { get; set; }
	public int G { get; set; }
	public int H { get; set; }
	public int I { get; set; }
	public int J { get; set; }
	public int K { get; set; }
	
}

public class Options
{
	[Option('i', "init", Required = false, HelpText = "Initialize and seed the database.")]
	public bool Init { get; set; }

	[Option('a', "async", Required = false, HelpText = "Use async queries.")]
	public bool Async { get; set; }

	[Option('t', "threads", Required = false, HelpText = "Total number of threads.")]
	public int Threads { get; set; }

	[Option('p', "processes", Required = false, HelpText = "Number of processes.")]
	public int Processes { get; set; }

	[Option('w', "worker", Required = false, HelpText = "Launch a worker process.")]
	public bool Worker { get; set; }

	[Option('q', "queries", Required = false, HelpText = "Number of queries to execute on each thread.")]
	public int Queries { get; set; }
}
