// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetListItemsPaged.cs" company="Devville">
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
    using Devville.DataService.Contracts.ServiceResponses;
    using Devville.Helpers;

    /// <summary>
    ///     Get paged list items.
    /// </summary>
    /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
    /// <created>12/28/2014</created>
    public class GetListItemsPaged : IServiceOperation
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Executes the current service operation.
        /// </summary>
        /// <param name="context">
        ///     The context.
        /// </param>
        /// <returns>
        ///     The Service Response
        /// </returns>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>12/28/2014</created>
        public IServiceResponse Execute(HttpContext context)
        {
            var data = Common.GetListItemsByViewAsDataTable(context);
            var pageIndex = context.Request[PageIndexKey].To(0);
            var pageSize = context.Request[PageSizeKey].To(8);
            var totalCount = data.Rows.Count;

            // if the user provided pageIndex which higher of the currect one then set its value to the last page index.
            var lastPageIndex = (int)(Math.Ceiling((double)totalCount / pageSize) - 1);

            pageIndex = lastPageIndex >= pageIndex ? pageIndex : lastPageIndex;

            var pagedData = data.AsEnumerable().Skip(pageIndex * pageSize).Take(pageSize).ToList();
            var pagedResults = pagedData.Any() ? pagedData.CopyToDataTable() : new DataTable();

            var response = new JsonResponse(new { Items = pagedResults });
            response.Extras[PageIndexKey] = pageIndex;
            response.Extras[PageSizeKey] = pageSize;
            response.Extras[TotalCountKey] = data.Rows.Count;

            return response;
        }

        #endregion

        #region Constants

        /// <summary>
        ///     The page index key
        /// </summary>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>1/7/2015</created>
        public const string PageIndexKey = "PageIndex";

        /// <summary>
        ///     The page size key
        /// </summary>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>1/7/2015</created>
        public const string PageSizeKey = "PageSize";

        /// <summary>
        ///     The total count key
        /// </summary>
        /// <author>
        ///     Ahmed Magdy (ahmed.magdy@devville.net)
        /// </author>
        /// <created>1/7/2015</created>
        public const string TotalCountKey = "TotalCount";

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the operation name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>12/28/2014</created>
        public string Name
        {
            get
            {
                return "GetSPListItemsPaged";
            }
        }

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
                    "Gets SharePoint ListItems Paged with TotalItemCount, it's useful if you want to build pager with number. Parameters are: 'SiteUrl', 'ListUrl', 'ViewName', 'PageSize' and 'PageIndex'. You can 'ConvertToUmAlQura' to true if you want to have create a new date columns to UmAlQura." + Constants.OperationDescription;
            }
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        /// <author>Ahmed Magdy (amagdy@sure.com.sa)</author>
        /// <created>3/21/2015</created>
        public Dictionary<string, string> Parameters
        {
            get
            {
                var parameters = new Dictionary<string, string>();
                parameters["SiteUrl"] = "string: The site collection URL";
                parameters["ListUrl"] = "string: The list URL";
                parameters["ViewName"] = "string: The view name";
                parameters["ConvertToUmAlQura"] = "bool: True or False to convert all DateTime columns to UmAlQura calendar.";
                parameters[PageIndexKey] = "int: The page index.";
                parameters[PageSizeKey] = "int: The page size.";
                return parameters;
            }
        }

        #endregion
    }
}