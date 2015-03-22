// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetTaxonomyTerms.cs" company="Devville">
//   Copyright © 2015 All Right Reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Devville.DataService.SharePointOperations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using Devville.DataService.Contracts;
    using Devville.DataService.ServiceResponses;
    using Devville.DataService.SharePointOperations.Model;

    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Taxonomy;

    /// <summary>
    ///     The get SharePoint taxonomy terms.
    /// </summary>
    public class GetTaxonomyTerms : IServiceOperation
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
                return "Get the SharePoint terms (taxonomy) in a tree format." + Constants.OperationDescription;
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
        /// <created>1/7/2015</created>
        public string Name
        {
            get
            {
                return "GetSPTaxonomyTerms";
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
                parameters["TermStoreName"] = "string: The term store name; otherwise will get the default term store";
                parameters["GroupName"] = "string: The group name.";
                parameters["TermSetName"] = "string: The Term Set Name";
                parameters["SiteUrl"] = "string: The site collection URL";
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
        /// <created>1/12/2015</created>
        /// <exception cref="System.Exception">
        /// Can't find the Term Store!
        /// </exception>
        public IServiceResponse Execute(HttpContext context)
        {
            string termStoreName = context.Request["TermStoreName"];
            string groupName = context.Request["GroupName"];
            string termSetName = context.Request["TermSetName"];
            string siteUrl = context.Request["SiteUrl"];

            if (string.IsNullOrWhiteSpace(siteUrl))
            {
                siteUrl = SPContext.Current.Site.Url;
            }

            using (var site = new SPSite(siteUrl))
            {
                var taxonomySession = new TaxonomySession(site);

                TermStore termStore = string.IsNullOrWhiteSpace(termStoreName)
                                          ? taxonomySession.DefaultSiteCollectionTermStore
                                          : taxonomySession.TermStores.SingleOrDefault(
                                              ts =>
                                              ts.Name.Equals(termStoreName, StringComparison.InvariantCultureIgnoreCase));

                if (termStore == null)
                {
                    throw new Exception("Can't find the Term Store!");
                }

                Group group = termStore.Groups[groupName];

                TermSet termSet = group.TermSets[termSetName];

                List<TaxonomyTerm> taxonomyTerms = this.BuildTermsTree(termSet.Terms);

                var model = new { Terms = taxonomyTerms };

                var response = new JsonResponse(model);
                return response;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the terms tree.
        /// </summary>
        /// <param name="terms">
        /// The terms.
        /// </param>
        /// <returns>
        /// The terms tree.
        /// </returns>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>1/12/2015</created>
        private List<TaxonomyTerm> BuildTermsTree(TermCollection terms)
        {
            var taxonomyTerms = new List<TaxonomyTerm>();
            foreach (Term term in terms)
            {
                var taxonomyTerm = new TaxonomyTerm
                                       {
                                           Id = term.Id, 
                                           Name = term.Name, 
                                           Terms = this.BuildTermsTree(term.Terms)
                                       };

                taxonomyTerms.Add(taxonomyTerm);
            }

            return taxonomyTerms;
        }

        #endregion
    }
}