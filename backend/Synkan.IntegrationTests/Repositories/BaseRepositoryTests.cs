using Microsoft.EntityFrameworkCore;
using Synkan.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Synkan.IntegrationTests.Repositories;

public class BaseRepositoryTests
{
    protected AppDbContext Context;
    
    private PostgreSqlContainer postgresContainer;
    private DbContextOptions<AppDbContext> dbContextOptions;
    
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        postgresContainer = new PostgreSqlBuilder("postgres:18-alpine")
            .WithDatabase("synkan_tests")
            .WithUsername("postgres")
            .WithPassword("1234")
            .Build();

        await postgresContainer.StartAsync();

        dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgresContainer.GetConnectionString())
            .Options;

        await using var migrationContext = CreateDbContext();
        await migrationContext.Database.MigrateAsync();
    }
    
    [SetUp]
    public virtual async Task SetUp()
    {
        await ResetDatabaseAsync();
        Context = CreateDbContext();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (Context is not null) 
            await Context.DisposeAsync();
        if (postgresContainer is not null) 
            await postgresContainer.DisposeAsync();
    }
    
    [TearDown]
    public async Task TearDown()
    {
        if (Context is not null) 
            await Context.DisposeAsync();
    }
    
    protected AppDbContext CreateDbContext()
    {
        return new AppDbContext(dbContextOptions);
    }
    
    private async Task ResetDatabaseAsync()
    {
        await using var resetContext = CreateDbContext();
        
        await resetContext.Users.ExecuteDeleteAsync();
        await resetContext.Boards.ExecuteDeleteAsync();
        await resetContext.Columns.ExecuteDeleteAsync();
        await resetContext.Checklists.ExecuteDeleteAsync();
        await resetContext.ChecklistItems.ExecuteDeleteAsync();
        await resetContext.Labels.ExecuteDeleteAsync();
        await resetContext.ChatMessages.ExecuteDeleteAsync();
        await resetContext.BoardMembers.ExecuteDeleteAsync();
        await resetContext.BoardAiSettings.ExecuteDeleteAsync();
    }
}