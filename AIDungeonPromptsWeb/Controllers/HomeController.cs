using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AIDungeonPrompts.Application.Queries.RandomPrompt;
using AIDungeonPrompts.Application.Queries.SearchPrompts;
using AIDungeonPrompts.Web.ColorScheme;
using AIDungeonPrompts.Web.Constants;
using AIDungeonPrompts.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AIDungeonPrompts.Web.Controllers
{
	public class HomeController : Controller
	{
		private readonly IMediator _mediator;

		public HomeController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost("/color-scheme"), ValidateAntiForgeryToken]
		public IActionResult ColorScheme(ColorSchemePreference? preference, string? returnUrl)
		{
			if (preference != null)
			{
				var cookieOptions = new CookieOptions
				{
					HttpOnly = true,
					IsEssential = true,
					MaxAge = new TimeSpan(365, 0, 0, 0),
					Secure = true,
				};
				Response.Cookies.Append(CookieValueConstants.DarkModePreference, ((int)preference).ToString(), cookieOptions);
			}
			if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}
			return RedirectToAction("Index");
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		public async Task<IActionResult> Index(SearchRequestParameters request)
		{
			var tags = new List<string>();
			if (!string.IsNullOrWhiteSpace(request.Tags))
			{
				tags = request.Tags.Split(',').Select(t => t.Trim()).ToList();
			}
			var nsfwIndex = tags.FindIndex(t => string.Equals("nsfw", t, System.StringComparison.OrdinalIgnoreCase));
			if (nsfwIndex > -1)
			{
				request.NsfwSetting = SearchNsfw.NsfwOnly;
				tags.RemoveAt(nsfwIndex);
			}

			var result = await _mediator.Send(new SearchPromptsQuery
			{
				Page = request.Page ?? 1,
				Reverse = request.Reverse,
				Search = request.Query ?? string.Empty,
				Tags = tags,
				Nsfw = request.NsfwSetting,
				TagJoin = request.TagJoin,
				TagsFuzzy = !request.MatchExact
			});

			return View(new SearchViewModel
			{
				Page = request.Page,
				Query = request.Query,
				Reverse = request.Reverse,
				Tags = request.Tags,
				NsfwSetting = request.NsfwSetting,
				SearchResult = result,
				MatchExact = request.MatchExact,
				TagJoin = request.TagJoin
			});
		}

		public async Task<IActionResult> Random()
		{
			var result = await _mediator.Send(new RandomPromptQuery());
			return RedirectToAction("View", "Prompts", new { result.Id });
		}

		[HttpGet("/whats-new")]
		public IActionResult WhatsNew()
		{
			return View();
		}
	}
}
