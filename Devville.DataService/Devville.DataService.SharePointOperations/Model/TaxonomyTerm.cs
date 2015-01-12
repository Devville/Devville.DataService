using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devville.DataService.SharePointOperations.Model
{
    public class TaxonomyTerm
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>1/7/2015</created>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>1/7/2015</created>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the terms.
        /// </summary>
        /// <value>
        /// The terms.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>1/7/2015</created>
        public List<TaxonomyTerm> Terms { get; set; }

        #endregion Public Properties
    }
}
