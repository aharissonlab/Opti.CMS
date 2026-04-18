using EPiServer.Filters;
using EPiServer.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Opti.CMS.Business;
using Opti.CMS.Models.Blocks;
using Opti.CMS.Models.ViewModels;

namespace Opti.CMS.Components;

public class PageListBlockViewComponent(ContentLocator contentLocator, IContentLoader contentLoader) : BlockComponent<PageListBlock>
{
    readonly ContentLocator _contentLocator = contentLocator;
    readonly IContentLoader _contentLoader = contentLoader;

    protected override IViewComponentResult InvokeComponent(PageListBlock currentContent)
    {
        var pages = FindPages(currentContent);

        pages = Sort(pages, currentContent.SortOrder);

        if (currentContent.Count > 0)
        {
            pages = pages.Take(currentContent.Count);
        }

        var model = new PageListModel(currentContent)
        {
            Pages = pages.Cast<PageData>()
        };

        ViewData.GetEditHints<PageListModel, PageListBlock>()
            .AddConnection(x => x.Heading, x => x.Heading);

        return View(model);
    }

    IEnumerable<PageData> FindPages(PageListBlock currentBlock)
    {
        IEnumerable<PageData> pages;
        var listRoot = currentBlock.Root;

        if (currentBlock.Recursive)
        {
            pages = currentBlock.PageTypeFilter is not null
                ? _contentLocator.FindPagesByPageType(listRoot, true, currentBlock.PageTypeFilter.ID)
                : _contentLocator.GetAll<PageData>(listRoot);
        }
        else
        {
            pages = currentBlock.PageTypeFilter is not null
                ? _contentLoader
                    .GetChildren<PageData>(listRoot)
                    .Where(p => p.ContentTypeID == currentBlock.PageTypeFilter.ID)
                : _contentLoader.GetChildren<PageData>(listRoot);
        }

        if (currentBlock.CategoryFilter is not null && !currentBlock.CategoryFilter.IsEmpty)
        {
            pages = pages.Where(x => x.Category.Intersect(currentBlock.CategoryFilter).Any());
        }

        return pages;
    }

    static IEnumerable<PageData> Sort(IEnumerable<PageData> pages, FilterSortOrder sortOrder)
    {
        var sortFilter = new FilterSort(sortOrder);
        sortFilter.Sort([.. pages.ToList()]);
        return pages;
    }
}