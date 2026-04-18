using EPiServer.ServiceLocation;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Opti.CMS.Business.Rendering;

namespace Opti.CMS.Views;

public abstract class AlloyPageBase<TModel>(AlloyContentAreaItemRenderer alloyContentAreaItemRenderer) : RazorPage<TModel> where TModel : class
{
    readonly AlloyContentAreaItemRenderer _alloyContentAreaItemRenderer = alloyContentAreaItemRenderer;

    public abstract override Task ExecuteAsync();

    public AlloyPageBase() : this(ServiceLocator.Current.GetInstance<AlloyContentAreaItemRenderer>())
    {
    }

    protected void OnItemRendered(ContentAreaItem contentAreaItem, TagHelperContext context, TagHelperOutput output) => _alloyContentAreaItemRenderer.RenderContentAreaItemCss(contentAreaItem, context, output);
}