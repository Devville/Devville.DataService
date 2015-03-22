// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetListItemsPaged2.cs" company="Devville">
//   Copyright © 2015 All Right Reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Devville.DataService.SharePointOperations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Web;

    using Devville.DataService.Contracts;
    using Devville.DataService.ServiceResponses;
    using Devville.Helpers;

    using Microsoft.SharePoint;

    /// <summary>
    ///     Get SPListItem paged using SPListItemPosition.
    /// </summary>
    /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
    /// <created>2/23/2015</created>
    public class GetListItemsPaged2 : IServiceOperation
    {
        #region Public Properties

        /// <summary>
        ///     Gets the description.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>2/23/2015</created>
        public string Description
        {
            get
            {
                return
                    "Gets SharePoint ListItems Paged in pages (does not get the total items count), it's useful if you want to virtual scroll or get page by page. Parameters are: 'SiteUrl', 'ListUrl' and 'ViewName'. You can 'ConvertToUmAlQura' to true if you want to have create a new date columns to UmAlQura."
                    + Constants.OperationDescription;
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
        /// <created>2/23/2015</created>
        public string Name
        {
            get
            {
                return "GetSPListItemsPaged2";
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
                parameters["SiteUrl"] = "string: The site collection URL";
                parameters["ListUrl"] = "string: The list URL";
                parameters["ViewName"] = "string: The view name";
                parameters["PageSize"] = "string: To override the view page size";
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
        /// <created>2/23/2015</created>
        /// <exception cref="System.MissingFieldException">
        /// Can't find ListUrl parameter
        /// </exception>
        /// <exception cref="System.IndexOutOfRangeException">
        /// Can't find list associated with the following URL:  + siteUrl
        /// </exception>
        public IServiceResponse Execute(HttpContext context)
        {
            string siteUrl = context.Request["SiteUrl"];
            string listUrl = context.Request["ListUrl"];
            string viewName = context.Request["ViewName"];
            string pagingInfo = context.Request["PagingInfo"];
            string pageSize = context.Request["PageSize"];

            if (string.IsNullOrWhiteSpace(listUrl))
            {
                throw new MissingFieldException("Can't find ListUrl parameter");
            }

            siteUrl = string.IsNullOrWhiteSpace(siteUrl) ? context.Request.Url.ToString() : siteUrl;

            using (var site = new SPSite(siteUrl))
            using (SPWeb web = site.OpenWeb())
            {
                SPList list = web.GetList(listUrl);
                if (list == null)
                {
                    throw new IndexOutOfRangeException("Can't find list associated with the following URL: " + siteUrl);
                }

                SPView view = !string.IsNullOrWhiteSpace(viewName) ? list.Views[viewName] : list.DefaultView;

                var query = new SPQuery(view) { RowLimit = pageSize.To(view.RowLimit) };

                if (!string.IsNullOrEmpty(pagingInfo))
                {
                    query.ListItemCollectionPosition = new SPListItemCollectionPosition(pagingInfo);
                }

                SPListItemCollection items = list.GetItems(query);

                string nextPageInfo = items.ListItemCollectionPosition == null
                                          ? null
                                          : items.ListItemCollectionPosition.PagingInfo;

                // contains the PagingInfo needed to go to the next page
                string prevPageInfo = string.Format("PagePrev=True&amp;Paged=TRUE&amp;p_ID={0}", items[0].ID);

                string nextPageUrl = string.IsNullOrWhiteSpace(nextPageInfo)
                                         ? null
                                         : GetUrlWithPagingInfo(context, nextPageInfo);
                string prevPageUrl = GetUrlWithPagingInfo(context, prevPageInfo);

                var response = new JsonResponse(new { Items = items.GetDataTable() });
                response.Extras["NextPageUrl"] = nextPageUrl == null ? null : new Uri(nextPageUrl);
                response.Extras["PrevPageUrl"] = new Uri(prevPageUrl);
                response.Extras["PageSize"] = query.RowLimit;

                return response;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the URL with paging information.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="pagingInfo">
        /// The paging information.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>2/23/2015</created>
        private static string GetUrlWithPagingInfo(HttpContext context, string pagingInfo)
        {
            NameValueCollection queryStrings = HttpUtility.ParseQueryString(context.Request.QueryString.ToString());
            queryStrings["PagingInfo"] = pagingInfo;
            return string.Format(
                "{0}://{1}{2}?{3}", 
                context.Request.Url.Scheme, 
                context.Request.Url.Authority, 
                context.Request.Url.AbsolutePath, 
                queryStrings);
        }

        #endregion
    }
}