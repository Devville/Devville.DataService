// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetListItemsPartitioned.cs" company="Devville">
//   Copyright © 2015 All Right Reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Devville.DataService.SharePointOperations
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Web;

    using Devville.DataService.Contracts;
    using Devville.DataService.Contracts.ServiceResponses;
    using Devville.Helpers;

    /// <summary>
    ///     The get list items partitioned.
    /// </summary>
    public class GetListItemsPartitioned : IServiceOperation
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
                    "Gets SharePoint ListItems in grouped in partitions like two by two. Parameters are: 'SiteUrl', 'ListUrl' and 'ViewName'. You can 'ConvertToUmAlQura' to true if you want to have create a new date columns to UmAlQura."
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
        /// <created>12/29/2014</created>
        public string Name
        {
            get
            {
                return "GetSPListItemsPartitioned";
            }
        }

        /// <summary>
        ///     Gets the parameters.
        /// </summary>
        /// <value>
        ///     The parameters.
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
                parameters["ConvertToUmAlQura"] =
                    "bool: True or False to convert all DateTime columns to UmAlQura calendar.";
                parameters["PartitionSize"] =
                    "int: The partition size. The service will split/parition the data into that parititon count/size.";
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
        /// <created>12/29/2014</created>
        public IServiceResponse Execute(HttpContext context)
        {
            DataTable results = Common.GetListItemsByViewAsDataTable(context);
            int partitionSize = context.Request["PartitionSize"].To(2);
            var data = new List<object>();
            int i = -1;
            results.AsEnumerable()
                .Partition(partitionSize)
                .ToList()
                .ForEach(p => data.Add(new { Partition = ++i, Items = p.CopyToDataTable() }));

            var serviceResponse = new JsonResponse(new { Partitions = data });
            return serviceResponse;
        }

        #endregion
    }
}