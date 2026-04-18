using EPiServer.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;
using static Opti.CMS.Globals;

namespace Opti.CMS.Business.Rendering;

public class AlloyContentAreaItemRenderer(IContentAreaLoader contentAreaLoader)
{
    readonly IContentAreaLoader _contentAreaLoader = contentAreaLoader;

    /// <summary>
    /// Gets a CSS class used for styling based on a tag name (ie a Bootstrap class name)
    /// </summary>
    /// <param name="tagName">Any tag name available, see <see cref="ContentAreaTags"/></param>
    static string GetCssClassForTag(string tagName) => string.IsNullOrEmpty(tagName)
            ? string.Empty
            : tagName.ToLowerInvariant() switch
            {
                ContentAreaTags.FullWidth => "col-12",
                ContentAreaTags.WideWidth => "col-12 col-md-8",
                ContentAreaTags.HalfWidth => "col-12 col-sm-6",
                ContentAreaTags.NarrowWidth => "col-12 col-sm-6 col-md-4",
                _ => string.Empty,
            };

    static string GetTypeSpecificCssClasses(ContentAreaItem contentAreaItem)
    {
        var content = contentAreaItem.LoadContent();
        string cssClass = content == null ? string.Empty : content.GetOriginalType().Name.ToLowerInvariant();

        if (content is ICustomCssInContentArea customClassContent &&
            !string.IsNullOrWhiteSpace(customClassContent.ContentAreaCssClass))
        {
            cssClass += $" {customClassContent.ContentAreaCssClass}";
        }

        return cssClass;
    }

    public void RenderContentAreaItemCss(ContentAreaItem contentAreaItem, TagHelperContext context, TagHelperOutput output)
    {
        var displayOption = _contentAreaLoader.LoadDisplayOption(contentAreaItem);
        var cssClasses = new StringBuilder();

        if (displayOption != null)
        {
            cssClasses.Append(displayOption.Tag);
            cssClasses.Append((string)$" {GetCssClassForTag(displayOption.Tag)}");
        }
        cssClasses.Append((string)$" {GetTypeSpecificCssClasses(contentAreaItem)}");

        foreach (string cssClass in cssClasses.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            output.AddClass(cssClass, System.Text.Encodings.Web.HtmlEncoder.Default);
        }
    }
}