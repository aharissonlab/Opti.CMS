using EPiServer.Data;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Opti.CMS.Models.Pages;
using Opti.CMS.Models.ViewModels;

namespace Opti.CMS.Business;

[ServiceConfiguration]
public class PageViewContextFactory(
    IContentLoader contentLoader,
    UrlResolver urlResolver,
    IDatabaseMode databaseMode,
    IOptionsMonitor<CookieAuthenticationOptions> optionMonitor)
{
    readonly IContentLoader _contentLoader = contentLoader;
    readonly UrlResolver _urlResolver = urlResolver;
    readonly IDatabaseMode _databaseMode = databaseMode;
    readonly CookieAuthenticationOptions _cookieAuthenticationOptions = optionMonitor.Get(IdentityConstants.ApplicationScheme);

    public virtual LayoutModel CreateLayoutModel(ContentReference currentContentLink, HttpContext httpContext)
    {
        var startPageContentLink = SiteDefinition.Current.StartPage;

        // Use the content link with version information when editing the startpage,
        // otherwise the published version will be used when rendering the props below.
        if (currentContentLink.CompareToIgnoreWorkID(startPageContentLink))
        {
            startPageContentLink = currentContentLink;
        }

        var startPage = _contentLoader.Get<StartPage>(startPageContentLink);

        return new LayoutModel
        {
            Logotype = startPage.SiteLogotype,
            LogotypeLinkUrl = new HtmlString(_urlResolver.GetUrl(SiteDefinition.Current.StartPage)),
            ProductPages = startPage.ProductPageLinks,
            CompanyInformationPages = startPage.CompanyInformationPageLinks,
            NewsPages = startPage.NewsPageLinks,
            CustomerZonePages = startPage.CustomerZonePageLinks,
            LoggedIn = httpContext.User.Identity.IsAuthenticated,
            LoginUrl = new HtmlString(GetLoginUrl(currentContentLink)),
            SearchActionUrl = new HtmlString(UrlResolver.Current.GetUrl(startPage.SearchPageLink)),
            IsInReadonlyMode = _databaseMode.DatabaseMode == DatabaseMode.ReadOnly
        };
    }

    string GetLoginUrl(ContentReference returnToContentLink) => $"{_cookieAuthenticationOptions?.LoginPath.Value ?? Globals.LoginPath}?ReturnUrl={_urlResolver.GetUrl(returnToContentLink)}";

    public virtual IContent GetSection(ContentReference contentLink)
    {
        var currentContent = _contentLoader.Get<IContent>(contentLink);

        static bool isSectionRoot(ContentReference contentReference) =>
           ContentReference.IsNullOrEmpty(contentReference) ||
           contentReference.Equals(SiteDefinition.Current.StartPage) ||
           contentReference.Equals(SiteDefinition.Current.RootPage);

        return isSectionRoot(currentContent.ParentLink)
            ? currentContent
            : _contentLoader.GetAncestors(contentLink)
            .OfType<PageData>()
            .SkipWhile(x => !isSectionRoot(x.ParentLink))
            .FirstOrDefault();
    }
}