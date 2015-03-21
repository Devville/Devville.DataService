// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonResponseStatus.cs" company="Devville">
//   Copyright © 2015 All Right Reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Devville.DataService.Contracts.ServiceResponses
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    ///     Response Status
    /// </summary>
    /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
    /// <created>1/12/2015</created>
    public class JsonResponseStatus
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonResponseStatus"/> class.
        /// </summary>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>12/10/2014</created>
        public JsonResponseStatus(JsonOperationStatus status, string message)
        {
            this.Status = status;
            this.Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonResponseStatus"/> class.
        /// </summary>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>12/10/2014</created>
        public JsonResponseStatus(JsonOperationStatus status)
            : this(status, string.Empty)
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the message.
        /// </summary>
        /// <value>
        ///     The message.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>12/10/2014</created>
        public string Message { get; private set; }

        /// <summary>
        ///     Gets the status.
        /// </summary>
        /// <value>
        ///     The status.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>12/10/2014</created>
        [JsonConverter(typeof(StringEnumConverter))]
        public JsonOperationStatus Status { get; private set; }

        #endregion
    }
}