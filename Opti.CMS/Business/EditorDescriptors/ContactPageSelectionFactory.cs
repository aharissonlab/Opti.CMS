using EPiServer.ServiceLocation;
using EPiServer.Shell.ObjectEditing;

namespace Opti.CMS.Business.EditorDescriptors;

/// <summary>
/// Provides a list of options corresponding to ContactPage pages on the site
/// </summary>
/// <seealso cref="ContactPageSelector"/>
[ServiceConfiguration]
public class ContactPageSelectionFactory(ContentLocator contentLocator) : ISelectionFactory
{
    readonly ContentLocator _contentLocator = contentLocator;

    public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
    {
        var contactPages = _contentLocator.GetContactPages();

        return new List<SelectItem>(contactPages.Select(c => new SelectItem { Value = c.PageLink, Text = c.Name }));
    }
}