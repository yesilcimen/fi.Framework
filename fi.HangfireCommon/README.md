fi.HangfireCommon

fi.HangfireCommon is a helper library that simplifies automatic recurring job registration for Hangfire.
It scans loaded assemblies at application startup, discovers classes derived from AutoHangfireBase, and automatically registers them as Hangfire recurring jobs.

🚀 Features

🔍 Scans all loaded application assemblies

🧩 Automatically discovers AutoHangfireBase implementations

⏰ Registers jobs using RecurringJob.AddOrUpdate

♻️ Supports scoped and non-scoped dependencies

⚙️ One-line Hangfire configuration

🗑️ Extension methods for add/update/remove jobs

📦 Installation

Add the project reference:

dotnet add reference fi.HangfireCommon


Required NuGet packages:

dotnet add package Hangfire
dotnet add package Hangfire.SqlServer

⚙️ Service Configuration

Configure Hangfire in Program.cs or Startup.cs:

builder.Services.HangfireCommon(
    sqlConnectionString,
    new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }
);

▶️ Application Startup

Register all recurring jobs automatically during application startup:

app.UseHangfireCommon();


This will:

Discover all classes derived from AutoHangfireBase

Register them as recurring jobs using Name and TimeCron

Automatically update existing jobs if they already exist

🧱 Defining a Job

To create a job, simply inherit from AutoHangfireBase:

public class SampleJob : AutoHangfireBase
{
    public override string Name => "SampleJob";
    public override string TimeCron => Cron.Daily;

    public override Task RunJob()
    {
        // Job logic
        return Task.CompletedTask;
    }
}


⚠️ If Name or TimeCron is empty or null, the application will throw an exception during startup.

♻️ Scoped Dependency Support

If your job requires scoped services, implement one of the following interfaces:

IScopedDependency

IScopedSelfDependency

Example:

public class ScopedJob : AutoHangfireBase, IScopedDependency
{
    public override string Name => "ScopedJob";
    public override string TimeCron => Cron.Hourly;

    public override Task RunJob()
    {
        return Task.CompletedTask;
    }
}


The library will automatically resolve the job using a scoped service provider.

🔄 Manual Job Management

You can also manage jobs manually using extension methods:

Add or Update Job
hangfire.JobAddOrUpdate();

Remove Job
hangfire.JobRemove();

🧠 How It Works

Scans all .dll files in the application directory

Excludes Microsoft.* and System.* assemblies

Loads assemblies dynamically via reflection

Finds all concrete and inherited AutoHangfireBase implementations

Registers them as Hangfire recurring jobs

❗ Requirements

.NET 6+

Hangfire

SQL Server (via Hangfire.SqlServer)

📄 License

MIT License
