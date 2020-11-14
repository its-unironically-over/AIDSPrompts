using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AIDungeonPrompts.Application.Abstractions.DbContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AIDungeonPrompts.Application.Queries.GetPrompt
{
	public class GetPromptQuery : IRequest<GetPromptViewModel>
	{
		public int Id { get; set; }
	}

	public class GetPromptQueryHandler : IRequestHandler<GetPromptQuery, GetPromptViewModel>
	{
		private readonly IAIDungeonPromptsDbContext _dbContext;

		public GetPromptQueryHandler(IAIDungeonPromptsDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<GetPromptViewModel> Handle(GetPromptQuery request, CancellationToken cancellationToken)
		{
			return await _dbContext.Prompts
				.Include(e => e.WorldInfos)
				.Include(e => e.PromptTags)
				.ThenInclude(e => e.Tag)
				.AsNoTracking().Select(prompt => new GetPromptViewModel
				{
					AuthorsNote = prompt.AuthorsNote,
					Id = prompt.Id,
					Memory = prompt.Memory,
					Nsfw = prompt.Nsfw,
					PromptContent = prompt.PromptContent,
					Quests = prompt.Quests,
					Title = prompt.Title,
					Description = prompt.Description,
					DateCreated = prompt.DateCreated,
					WorldInfos = prompt.WorldInfos.Select(worldInfo => new GetPromptWorldInfoViewModel
					{
						Entry = worldInfo.Entry,
						Keys = worldInfo.Keys
					}),
					PromptTags = prompt.PromptTags.Select(promptTag => new GetPromptPromptTagViewModel
					{
						Id = promptTag.Tag!.Id,
						Name = promptTag.Tag!.Name
					})
				}).FirstOrDefaultAsync(prompt => prompt.Id == request.Id);
		}
	}
}
