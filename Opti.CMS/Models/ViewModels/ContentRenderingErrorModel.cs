namespace Opti.CMS.Models.ViewModels;

public class ContentRenderingErrorModel
{
    public ContentRenderingErrorModel(IContentData contentData, Exception exception)
    {
        ContentName = contentData is IContent content ? content.Name : string.Empty;

        ContentTypeName = contentData.GetOriginalType().Name;

        Exception = exception;
    }

    public string ContentName { get; set; }

    public string ContentTypeName { get; set; }

    public Exception Exception { get; set; }
}