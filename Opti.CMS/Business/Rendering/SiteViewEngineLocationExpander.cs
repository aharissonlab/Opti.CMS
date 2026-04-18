using Microsoft.AspNetCore.Mvc.Razor;

namespace Opti.CMS.Business.Rendering;

public class SiteViewEngineLocationExpander : IViewLocationExpander
{
    static readonly string[] AdditionalPartialViewFormats =
    [
        TemplateCoordinator.BlockFolder + "{0}.cshtml",
        TemplateCoordinator.PagePartialsFolder + "{0}.cshtml"
    ];

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        foreach (string location in viewLocations)
        {
            yield return location;
        }

        for (int i = 0; i < AdditionalPartialViewFormats.Length; i++)
        {
            yield return AdditionalPartialViewFormats[i];
        }
    }

    public void PopulateValues(ViewLocationExpanderContext context) { }
}