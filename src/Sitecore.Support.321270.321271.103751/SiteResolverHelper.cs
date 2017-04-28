using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.Sites;
using Sitecore.Web;
using System;
using System.Collections.Generic;

namespace Sitecore.Support
{
    internal class SiteResolverHelper
    {
        public static bool GetPreviewResolveSite()
        {
            return Settings.GetBoolSetting("Preview.ResolveSite", false);
        }     

        public static SiteInfo GetTargetSite(Item item)
        {
            UrlOptions urlOptions = new UrlOptions
            {
                SiteResolving = true
            };
            LinkBuilder builder = new LinkBuilder(urlOptions);
            return builder.ResolveTargetSiteHelper(item);
        }

        private class LinkBuilder : LinkProvider.LinkBuilder
        {
            private readonly UrlOptions _options;

            public LinkBuilder(UrlOptions urlOptions) : base(urlOptions)
            {
                this._options = urlOptions;
            }

            protected new static SiteInfo FindMatchingSite(Dictionary<LinkProvider.LinkBuilder.SiteKey, SiteInfo> resolvingTable, LinkProvider.LinkBuilder.SiteKey key)
            {
                Assert.ArgumentNotNull(resolvingTable, "resolvingTable");
                Assert.ArgumentNotNull(key, "key");
                if (key.Language.Length != 0)
                {
                    while (!resolvingTable.ContainsKey(key))
                    {
                        int length = key.Path.LastIndexOf("/", StringComparison.InvariantCulture);
                        if (length <= 1)
                        {
                            return null;
                        }
                        key = LinkProvider.LinkBuilder.BuildKey(key.Path.Substring(0, length), key.Language);
                    }
                    return resolvingTable[key];
                }
                return FindMatchingSiteByPath(resolvingTable, key.Path);
            }

            protected new static SiteInfo FindMatchingSiteByPath(Dictionary<LinkProvider.LinkBuilder.SiteKey, SiteInfo> resolvingTable, string path)
            {
                Assert.ArgumentNotNull(resolvingTable, "resolvingTable");
                Assert.ArgumentNotNull(path, "path");
                Label_0016:
                foreach (KeyValuePair<LinkProvider.LinkBuilder.SiteKey, SiteInfo> pair in resolvingTable)
                {
                    SiteInfo info = pair.Value;
                    if (pair.Key.Path.Equals(path, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return info;
                    }
                }
                int length = path.LastIndexOf("/", StringComparison.InvariantCulture);
                if (length > 1)
                {
                    path = path.Substring(0, length);
                    goto Label_0016;
                }
                return null;
            }

            public virtual SiteInfo ResolveTargetSiteHelper(Item item)
            {
                SiteContext site = Context.Site;
                SiteContext context2 = this._options.Site ?? site;
                SiteInfo siteInfo = context2?.SiteInfo;
                if (this._options.SiteResolving && (item.Database.Name != "core"))
                {
                    if ((this._options.Site != null) && ((site == null) || (this._options.Site.Name != site.Name)))
                    {
                        return siteInfo;
                    }
                    Dictionary<LinkProvider.LinkBuilder.SiteKey, SiteInfo> siteResolvingTable = base.GetSiteResolvingTable();
                    string path = item.Paths.FullPath.ToLowerInvariant();
                    SiteInfo info2 = FindMatchingSite(siteResolvingTable, LinkProvider.LinkBuilder.BuildKey(path, item.Language.ToString())) ?? FindMatchingSiteByPath(siteResolvingTable, path);
                    if (info2 != null)
                    {
                        return info2;
                    }
                }
                return siteInfo;
            }
        }
    }
}