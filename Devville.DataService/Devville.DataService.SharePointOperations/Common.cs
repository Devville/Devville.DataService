// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Common.cs" company="Devville">
//   Copyright © 2015 All Right Reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Devville.DataService.SharePointOperations
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Web;

    using Devville.Helpers;

    using Microsoft.SharePoint;

    /// <summary>
    ///     The common.
    /// </summary>
    public static class Common
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get list items by view.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="SPListItemCollection"/>.
        /// </returns>
        /// <exception cref="MissingFieldException">
        /// Can't find ListUrl parameter
        /// </exception>
        /// <exception cref="IndexOutOfRangeException">
        /// Can't find list associated with the following URL:  + siteUrl
        /// </exception>
        public static SPListItemCollection GetListItemsByView(HttpContext context)
        {
            string siteUrl = context.Request["SiteUrl"];
            string listUrl = context.Request["ListUrl"];
            string viewName = context.Request["ViewName"];

            if (string.IsNullOrWhiteSpace(listUrl))
            {
                throw new MissingFieldException("Can't find ListUrl parameter");
            }

            siteUrl = string.IsNullOrWhiteSpace(siteUrl) ? GetSiteUrl(context) : siteUrl;

            using (var site = new SPSite(siteUrl))
            using (SPWeb web = site.OpenWeb())
            {
                SPList list = web.GetList(listUrl);
                if (list == null)
                {
                    throw new IndexOutOfRangeException("Can't find list associated with the following URL: " + siteUrl);
                }

                SPListItemCollection results;

                if (string.IsNullOrWhiteSpace(viewName))
                {
                    results = list.GetItems();
                }
                else
                {
                    SPView view = list.Views[viewName];
                    results = list.GetItems(view);
                }

                return results;
            }
        }

        /// <summary>
        /// Gets the list items by view.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>12/28/2014</created>
        /// <exception cref="System.MissingFieldException">
        /// Can't find ListUrl parameter
        /// </exception>
        /// <exception cref="System.IndexOutOfRangeException">
        /// Can't find list associated with the following URL:  + siteUrl
        /// </exception>
        public static DataTable GetListItemsByViewAsDataTable(HttpContext context)
        {
            var convertToUmAlQura = context.Request["ConvertToUmAlQura"].To<bool>();
            SPListItemCollection items = GetListItemsByView(context);
            DataTable results = items.GetDataTable();

            if (convertToUmAlQura)
            {
                var dateTimeColumns =
                    results.Columns.Cast<DataColumn>()
                        .Where(dc => dc.DataType == typeof(DateTime))
                        .Select(dc => new { Name = dc.ColumnName, UmAlQuraName = dc.ColumnName + "UmAlQura" })
                        .ToList();
                if (dateTimeColumns.Any())
                {
                    string dateFormat = context.Request["DateFormat"].To("dd  MMMM  yyyy");
                    foreach (var column in dateTimeColumns)
                    {
                        results.Columns.Add(column.UmAlQuraName, typeof(string));
                    }

                    foreach (DataRow row in results.Rows)
                    {
                        foreach (var dateTimeColumn in dateTimeColumns)
                        {
                            var date = (DateTime)row[dateTimeColumn.Name];
                            var umalQuraCulture = new CultureInfo("ar-SA")
                                                      {
                                                          DateTimeFormat =
                                                              {
                                                                  Calendar =
                                                                      new UmAlQuraCalendar()
                                                              }
                                                      };
                            row[dateTimeColumn.UmAlQuraName] = date.ToString(dateFormat, umalQuraCulture);
                        }

                        row.AcceptChanges();
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Gets the site URL.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The site url from the current url without any query string
        /// </returns>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>3/17/2015</created>
        public static string GetSiteUrl(HttpContext context)
        {
            Uri url = context.Request.Url;
            return string.Format("{0}://{1}{2}", url.Scheme, url.Authority, url.AbsolutePath);
        }

        #endregion
    }
}