// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonResponse.cs" company="Devville">
//   Copyright © 2015 All Right Reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Devville.DataService.ServiceResponses
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using Devville.DataService.Contracts;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    ///     JSON Response
    /// </summary>
    /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
    /// <created>1/4/2015</created>
    public class JsonResponse : IServiceResponse
    {
        #region Constants

        /// <summary>
        ///     The extras prefix
        /// </summary>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>1/7/2015</created>
        private const string ExtrasPrefix = "_";

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonResponse"/> class.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <author>
        ///     Ahmed Magdy (ahmed.magdy@devville.net)
        /// </author>
        /// <created>1/12/2015</created>
        public JsonResponse(object model, JsonResponseStatus status)
        {
            this.Model = model;
            this.ServiceStatus = status;
            this.Extras = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonResponse"/> class.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <author>
        ///     Ahmed Magdy (ahmed.magdy@devville.net)
        /// </author>
        /// <created>1/12/2015</created>
        public JsonResponse(object model)
            : this(model, new JsonResponseStatus(JsonOperationStatus.Succeeded))
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the type of the content.
        /// </summary>
        /// <value>
        ///     The type of the content.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>1/7/2015</created>
        [JsonIgnore]
        public string ContentType
        {
            get
            {
                return "application/json";
            }
        }

        /// <summary>
        ///     Gets or sets the extras.
        /// </summary>
        /// <value>
        ///     The extras.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>1/7/2015</created>
        public Dictionary<string, object> Extras { get; set; }

        /// <summary>
        ///     Gets or sets the items.
        /// </summary>
        /// <value>
        ///     The items.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>3/30/2013</created>
        public object Model { get; set; }

        /// <summary>
        ///     Gets or sets the service status.
        /// </summary>
        /// <value>
        ///     The service status.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>12/10/2014</created>
        public JsonResponseStatus ServiceStatus { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>1/12/2015</created>
        public void Render(HttpContext context)
        {
            this.Render(context, false);
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="renderModelOnly">
        /// if set to <c>true</c> [render model only].
        /// </param>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>1/12/2015</created>
        public void Render(HttpContext context, bool renderModelOnly)
        {
            var serializerSettings = new JsonSerializerSettings
                                         {
                                             ContractResolver =
                                                 new CamelCasePropertyNamesContractResolver(), 
                                             ReferenceLoopHandling =
                                                 ReferenceLoopHandling.Serialize
                                         };

            string response;
            if (renderModelOnly)
            {
                response = JsonConvert.SerializeObject(this.Model, serializerSettings);
            }
            else
            {
                IEnumerable<string> queryStringKeys =
                    context.Request.QueryString.AllKeys.Where(q => !string.IsNullOrWhiteSpace(q));
                foreach (string queryStringKey in queryStringKeys)
                {
                    string extraKey = queryStringKey.Replace(ExtrasPrefix, string.Empty);
                    if (this.Extras.ContainsKey(extraKey))
                    {
                        extraKey = extraKey + "_";
                    }

                    this.Extras.Add(extraKey, context.Request[queryStringKey]);
                }

                response = JsonConvert.SerializeObject(this, serializerSettings);
            }

            context.Response.Clear();
            context.Response.ContentType = this.ContentType;
            context.Response.Write(response);
        }

        #endregion
    }
}