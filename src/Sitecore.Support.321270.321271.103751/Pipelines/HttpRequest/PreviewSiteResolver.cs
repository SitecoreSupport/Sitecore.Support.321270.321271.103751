using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Layouts;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Sites;
using Sitecore.Support;
using Sitecore.Web;
using System;

namespace Sitecore.Support.Pipelines.HttpRequest
{
    public class PreviewSiteResolver: HttpRequestProcessor
    {
        protected virtual bool IsPreview(HttpRequestArgs args)
        {
            if (!Context.PageMode.IsExperienceEditor && !Context.PageMode.IsPreview)
            {
                return false;
            }
            return true;
        }

        public override void Process(HttpRequestArgs args)
        {
            if (this.IsPreview(args) && SiteResolverHelper.GetPreviewResolveSite())
            {
                Item item = Context.Item;
                if (item != null)
                {
                    SiteContext siteContext = null;
                    SiteInfo targetSite = SiteResolverHelper.GetTargetSite(item);
                    if (targetSite != null)
                    {
                        siteContext = SiteContextFactory.GetSiteContext(targetSite.Name);
                        if (siteContext != null)
                        {
                            PageContext page = Context.Page;
                            if (((page != null) && (page.FilePath != null)) && (page.FilePath.Length > 0))
                            {
                                siteContext.Page.FilePath = Context.Page.FilePath;
                            }
                            Context.Site = siteContext;
                        }
                    }
                }
            }
        }
    }
}