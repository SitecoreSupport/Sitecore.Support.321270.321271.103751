using Sitecore.Data;
using Sitecore.Data.Events;
using Sitecore.Diagnostics;
using Sitecore.Web.UI.Sheer;
using Sitecore.Links;
using Sitecore.Data.Items;
using Sitecore.Web;
using Sitecore.Sites;
using Sitecore.Configuration;
using Sitecore.StringExtensions;
using Sitecore.Text;
using Sitecore.Globalization;
using System.Web.UI;
using Sitecore.Resources;
using Sitecore.Pipelines.HasPresentation;

namespace Sitecore.Support.Shell.Applications.WebEdit
{
    public class WebEditRibbonForm: Sitecore.Shell.Applications.WebEdit.WebEditRibbonForm
    {
        private const string EditModeKey = "edit";
        private const string PreviewModeKey = "preview";
        private ID parentID;

        protected override void DeletedNotification(object sender, ItemDeletedEventArgs args)
        {
            base.DeletedNotification(sender, args);
            this.parentID = args.ParentID;
        }
        protected new void Redirect(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Item item = Database.GetDatabase("master").GetItem(this.parentID);
            SiteInfo siteInfo = GetSite(item);
            item = IsLayoutExist(item) ?? Database.GetDatabase("master").GetItem((siteInfo.RootPath + siteInfo.StartItem).Replace("//", "/"));
            UrlOptions option = new UrlOptions()
            {
                AlwaysIncludeServerUrl = true,
                Site = new Sites.SiteContext(siteInfo),
                LanguageEmbedding = LinkManager.LanguageEmbedding,
                LowercaseUrls = LinkManager.LowercaseUrls,
                ShortenUrls = LinkManager.ShortenUrls,
                UseDisplayName = LinkManager.UseDisplayName,
                LanguageLocation = LinkManager.LanguageLocation,
                AddAspxExtension = LinkManager.AddAspxExtension,
                SiteResolving = true,
                EncodeNames = LinkManager.Provider.EncodeNames,
            };
            string url = LinkManager.GetItemUrl(item, option);
            SheerResponse.Eval($"window.parent.location.href='{url}?sc_mode=edit&sc_site={siteInfo.Name}'");
        }
        private Item IsLayoutExist(Item item)
        {
            Item _item = item;
            LayoutItem _layout = item.Visualization.Layout;
            while (true)
            {
                if (_layout == null)
                {
                    _item = _item.Parent;
                    if (_item.ID == ID.Parse("00000000-0000-0000-0000-000000000000") || _item.ID == ID.Parse("11111111-1111-1111-1111-111111111111"))
                        return null;
                    else
                        _layout = _item.Visualization.Layout;
                }
                else
                    return _item;
            }
        }
        private SiteInfo GetSite(Item item)
        {
            SiteInfo site = null;
            foreach (SiteInfo info in SiteContextFactory.Sites)
            {
                if (((!info.RootPath.IsNullOrEmpty() && !info.StartItem.IsNullOrEmpty()) && (!info.Domain.Equals("sitecore") && !info.Domain.Equals(""))) && !info.VirtualFolder.Contains("/sitecore modules/web"))
                {
                    Item item2 = Database.GetDatabase("master").GetItem(info.RootPath + info.StartItem);
                    if (item2.ParentID.Equals(item.ParentID))
                        return site = info;
                    if (item.Paths.Path.Contains(item2.Parent.Paths.Path))
                        return site = info;
                }
            }
            return SiteContextFactory.GetSiteInfo(Settings.Preview.DefaultSite);
        }
        private string AppendModeToUrl(string url)
        {
            UrlString str = new UrlString(url);
            string str2 = null;
            if (Context.PageMode.IsPreview)
            {
                str2 = PreviewModeKey;
            }
            else if (Context.PageMode.IsExperienceEditor)
            {
                str2 = EditModeKey;
            }
            if (!string.IsNullOrEmpty(str2))
            {
                str.Parameters["sc_mode"] = str2;
            }
            return str.ToString();
        }
        private string GetItemUrl(Item item)
        {
            string str3;
            Assert.ArgumentNotNull(item, "item");
            SiteRequest request = Context.Request;
            Assert.IsNotNull(request, "Site request not found.");
            UrlOptions defaultOptions = UrlOptions.DefaultOptions;
            defaultOptions.ShortenUrls = false;
            defaultOptions.Language = item.Language;
            SiteContext site = null;
            if (SiteResolverHelper.GetPreviewResolveSite())
            {
                string siteName = this.SiteName;
                site = string.IsNullOrEmpty(siteName) ? null : SiteContextFactory.GetSiteContext(siteName);
            }
            else
            {
                string str2 = !string.IsNullOrEmpty(request.QueryString["sc_pagesite"]) ? request.QueryString["sc_pagesite"] : "website";
                site = Factory.GetSite(str2);
            }
            Language language = WebEditUtil.GetClientContentLanguage() ?? Context.Language;
            if (site == null)
            {
                using (new LanguageSwitcher(language))
                {
                    return this.AppendModeToUrl(Assert.ResultNotNull<string>(LinkManager.GetItemUrl(item, defaultOptions)));
                }
            }
            using (new SiteContextSwitcher(site))
            {
                using (new LanguageSwitcher(language))
                {
                    str3 = this.AppendModeToUrl(Assert.ResultNotNull<string>(LinkManager.GetItemUrl(item, defaultOptions)));
                }
            }
            return str3;
        }
        protected override void RenderTreecrumbGo(HtmlTextWriter output, Item item)
        {
            Assert.ArgumentNotNull(output, "output");
            Assert.ArgumentNotNull(item, "item");
            output.Write("<div class=\"scTreecrumbDivider\">{0}</div>", Images.GetSpacer(1, 1));
            bool flag = HasPresentationPipeline.Run(item);
            if (flag)
            {
                output.Write("<a href=\"{0}\" class=\"scTreecrumbGo\" target=\"_parent\">", this.GetItemUrl(item));
            }
            else
            {
                output.Write("<span class=\"scTreecrumbGo\">");
            }
            ImageBuilder builder = new ImageBuilder
            {
                Src = "ApplicationsV2/16x16/arrow_right_green.png",
                Class = "scTreecrumbGoIcon",
                Disabled = !flag
            };
            output.Write("{0} {1}{2}", builder, Translate.Text("Go"), flag ? "</a>" : "</span>");
        }
        protected virtual string SiteName { get; }
    }
}