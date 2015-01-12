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
        /// <created>12/28/2014</created>
        public IServiceResponse Execute(HttpContext context)
        {
            DataTable data = Common.GetListItemsByViewAsDataTable(context);
            int pageIndex = context.Request[PageIndexKey].To(0);
            int pageSize = context.Request[PageSizeKey].To(8);
            int totalCount = data.Rows.Count;

            // if the user provided pageIndex which higher of the currect one then set its value to the last page index.
            var lastPageIndex = (int)(Math.Ceiling((double)totalCount / pageSize) - 1);

            pageIndex = lastPageIndex >= pageIndex ? pageIndex : lastPageIndex;

            List<DataRow> pagedData = data.AsEnumerable().Skip(pageIndex * pageSize).Take(pageSize).ToList();
            DataTable pagedResults = pagedData.Any() ? pagedData.CopyToDataTable() : new DataTable();

            var response = new JsonResponse(new { Items = pagedResults });
            response.Extras[PageIndexKey] = pageIndex;
            response.Extras[PageSizeKey] = pageSize;
            response.Extras[TotalCountKey] = data.Rows.Count;

            return response;
        }

        #endregion
    }
}