// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TaxonomyTerm.cs" company="Devville">
//   Copyright © 2015 All Right Reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Devville.DataService.SharePointOperations.Model
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Taxonomy Term
    /// </summary>
    /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
    /// <created>3/22/2015</created>
    public class TaxonomyTerm
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the identifier.
        /// </summary>
        /// <value>
        ///     The identifier.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>1/7/2015</created>
        public Guid Id { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>1/7/2015</created>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the terms.
        /// </summary>
        /// <value>
        ///     The terms.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>1/7/2015</created>
        public List<TaxonomyTerm> Terms { get; set; }

        #endregion
    }
}