// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetSiteData.cs" company="Devville">
//   Copyright © 2015 All Right Reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Devville.DataService.SharePointOperations
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Web;

    using Devville.DataService.Contracts;
    using Devville.DataService.ServiceResponses;
    using Devville.Helpers;

    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Publishing;

    using NLog;

    /// <summary>
    ///     The get site data.
    /// </summary>
    public class GetSiteData : IServiceOperation
    {
        #region Static Fields

        /// <summary>
        ///     The logger
        /// </summary>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>3/17/2015</created>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the description.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>3/17/2015</created>
        /// <created>2/23/2015</created>
        public string Description
        {
            get
            {
                return "Gets data cross the SharePoint site colleciton.";
            }
        }

        /// <summary>
        ///     Gets the operation name to be used in the data service.
        ///     Make SURE that you use a unique name for your operation.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>3/17/2015</created>
        public string Name
        {
            get
            {
                return "SPGetSiteData";
            }
        }

        /// <summary>
        ///     Gets the parameters.
        /// </summary>
        /// <value>
        ///     The parameters.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>3/21/2015</created>
        public Dictionary<string, string> Parameters
        {
            get
            {
                var parameters = new Dictionary<string, string>();
                parameters["SiteUrl"] = "string: The site collection URL; otherwise use the current service URL.";
                parameters["listsServerTemplate"] = "int: lists server template";
                parameters["Query"] = "string: CAML query";
                parameters["ViewFields"] =
                    "string: internal field names joined by semilcolon. Example: Title;ID;CreatedBy";
                parameters["Recursive"] = "bool: True or False if you want to get data Recursively.";
                parameters["UseCache"] =
                    "bool: True or False if you want to cache the data (this will be applied to all site collection data).";
                return parameters;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Executes the current service operation.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The Service Response
        /// </returns>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>3/17/2015</created>
        public IServiceResponse Execute(HttpContext context)
        {
            Logger.Trace("Start Operation: " + this.Name);
            string siteUrl = context.Request["SiteUrl"] ?? Common.GetSiteUrl(context);

            // Custom list template id: 100
            int listsServerTemplate = context.Request["listsServerTemplate"].To(100);
            string queryCaml = context.Request["Query"];
            string viewFields = context.Request["ViewFields"];
            bool recursive = context.Request["Recursive"].To(true);
            var useCache = context.Request["UseCache"].To<bool>();

            Logger.Debug("SiteUrl: {0}", siteUrl);
            Logger.Debug("Lists Server Template: {0}", listsServerTemplate);
            Logger.Debug("Query: {0}", queryCaml);
            Logger.Debug("ViewFields: {0}", viewFields);
            Logger.Debug("Recursive: {0}", recursive);
            Logger.Debug("UseCache: {0}", useCache);

            using (var site = new SPSite(siteUrl))
            using (SPWeb web = site.OpenWeb())
            {
                DataTable data = this.GetData(web, listsServerTemplate, queryCaml, viewFields, recursive, useCache);

                var serviceResponse = new JsonResponse(new { Items = data });
                return serviceResponse;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="web">
        /// The web.
        /// </param>
        /// <param name="listsServerTemplate">
        /// The lists Server Template.
        /// </param>
        /// <param name="queryCaml">
        /// The query CAML.
        /// </param>
        /// <param name="viewFields">
        /// The view fields.
        /// </param>
        /// <param name="websScopeRecursive">
        /// if set to <c>true</c> [webs scope recursive].
        /// </param>
        /// <param name="useCache">
        /// if set to <c>true</c> [use cache].
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>3/17/2015</created>
        private DataTable GetData(
            SPWeb web, 
            int listsServerTemplate, 
            string queryCaml, 
            string viewFields, 
            bool websScopeRecursive, 
            bool useCache)
        {
            if (web == null)
            {
                throw new ArgumentNullException("web");
            }

            if (queryCaml == null)
            {
                throw new ArgumentNullException("queryCaml");
            }

            if (viewFields == null)
            {
                throw new ArgumentNullException("viewFields");
            }

            string fields = viewFields.Split(';')
                .Aggregate(string.Empty, (current, field) => current + string.Format("<FieldRef Name='{0}' />", field));
            Logger.Debug("Formatted Fields: " + fields);

            var crossListQueryInfo = new CrossListQueryInfo();

            crossListQueryInfo.Lists = string.Format("<Lists ServerTemplate='{0}' />", listsServerTemplate);
            crossListQueryInfo.Query = queryCaml;
            crossListQueryInfo.ViewFields = fields;
            crossListQueryInfo.Webs = websScopeRecursive ? "<Webs Scope='Recursive' />" : "<Webs />";
            crossListQueryInfo.UseCache = useCache;

            var crossListQueryCache = new CrossListQueryCache(crossListQueryInfo);
            DataTable data = useCache ? crossListQueryCache.GetSiteData(web.Site) : crossListQueryCache.GetSiteData(web);
            if (data != null)
            {
                data.Columns.Add("ListUrl", typeof(string));
                data.Columns.Add("WebUrl", typeof(string));
                foreach (DataRow row in data.Rows)
                {
                    var webId = row["WebId"].To<Guid>();
                    using (SPWeb itemWeb = web.Site.OpenWeb(webId))
                    {
                        row["WebUrl"] = itemWeb.Url;
                        var listId = row["listId"].To<Guid>();
                        SPList itemList = itemWeb.Lists[listId];
                        row["ListUrl"] = web.Site.MakeFullUrl(itemList.RootFolder.ServerRelativeUrl);
                        row.AcceptChanges();
                    }
                }
            }

            return data;
        }

        #endregion
    }
}