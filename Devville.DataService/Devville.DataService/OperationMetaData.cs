// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OperationMetaData.cs" company="Devville">
//   Copyright © 2015 All Right Reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Devville.DataService
{
    using System;
    using System.Collections.Generic;
    using System.Web;

    using Newtonsoft.Json;

    /// <summary>
    ///     Operation Meta Data
    /// </summary>
    /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
    /// <created>3/18/2015</created>
    [Serializable]
    internal class OperationMetaData
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperationMetaData" /> class.
        /// </summary>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>3/18/2015</created>
        public OperationMetaData()
        {
            this.Id = Guid.NewGuid();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the assembly path.
        /// </summary>
        /// <value>
        ///     The assembly path.
        /// </value>
        /// <author>Ahmed Magdy (amagdy@sure.com.sa)</author>
        /// <created>3/19/2015</created>
        [JsonIgnore]
        [JsonProperty(Order = 4)]
        public string AssemblyPath { get; set; }

        /// <summary>
        ///     Gets or sets the description.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>3/18/2015</created>
        [JsonProperty(Order = 3)]
        public string Description { get; set; }

        /// <summary>
        ///     Gets the id.
        /// </summary>
        [JsonIgnore]
        public Guid Id { get; private set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>3/18/2015</created>
        [JsonProperty(Order = 1)]
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the type of the operation.
        /// </summary>
        /// <value>
        ///     The type of the operation.
        /// </value>
        /// <author>Ahmed Magdy (amagdy@sure.com.sa)</author>
        /// <created>3/19/2015</created>
        public string OperationType { get; set; }

        /// <summary>
        ///     Gets the operation URL.
        /// </summary>
        /// <value>
        ///     The operation URL.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>3/18/2015</created>
        [JsonProperty(Order = 2)]
        public string OperationUrl
        {
            get
            {
                return string.Format("{0}?op={1}", HttpContext.Current.Request.Url, this.Name);
            }
        }

        /// <summary>
        ///     Gets or sets the parameters.
        /// </summary>
        /// <value>
        ///     The parameters.
        /// </value>
        /// <author>Ahmed Magdy (amagdy@sure.com.sa)</author>
        /// <created>3/21/2015</created>
        [JsonProperty(Order = 5)]
        public Dictionary<string, string> Parameters { get; set; }

        #endregion
    }
}