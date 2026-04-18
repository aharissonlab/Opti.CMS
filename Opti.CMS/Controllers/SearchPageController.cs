using EPiServer.AddOns.Helpers;
using EPiServer.Find;
using EPiServer.Find.Cms;
using Microsoft.AspNetCore.Mvc;
using Opti.CMS.Models.Pages;
using Opti.CMS.Models.ViewModels;
using static Opti.CMS.Models.ViewModels.SearchContentModel;

namespace Opti.CMS.Controllers;

public class SearchPageController(IClient client) : PageControllerBase<SearchPage>
{
    public readonly IClient client = client;
    public async Task<IActionResult> Index(SearchPage currentPage, string searchedQuery)
    {
        var model = new SearchContentModel(currentPage);

        if (!string.IsNullOrWhiteSpace(searchedQuery))
        {
            model.SearchedQuery = searchedQuery;


            ITypeSearch<SitePageData> query = client
                .Search<SitePageData>() 
                .For(searchedQuery)                                        
                .FilterForVisitor()
                .FilterOnCurrentSite(); 

            var results = await query.GetContentResultAsync();

            model.NumberOfHits = results.TotalMatching;

            model.Hits = [.. results
                    .Select(x => new SearchHit
                    {
                        Title = x.MetaTitle ?? x.Name,
                        Url = x.PageLink.GetPublicUrl(),
                        Excerpt = x.MetaDescription
                    })];
        }

        return View(model);

    }
}