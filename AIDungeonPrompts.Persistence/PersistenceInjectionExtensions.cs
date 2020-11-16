using AIDungeonPrompts.Application.Abstractions.DbContexts;
using AIDungeonPrompts.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AIDungeonPrompts.Persistence
{
	public static class PersistenceInjectionExtensions
	{
		public static IServiceCollection AddPersistenceLayer(this IServiceCollection services,
			string databaseConnection)
		{
			services
				.AddDbContext<AIDungeonPromptsDbContext>(options => options.UseNpgsql(databaseConnection, builder =>
				{
					// See: https://docs.microsoft.com/en-us/ef/core/querying/single-split-queries
					builder.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
				}))
				.AddScoped<IAIDungeonPromptsDbContext>(provider => provider.GetService<AIDungeonPromptsDbContext>()!);

			return services;
		}
	}
}
