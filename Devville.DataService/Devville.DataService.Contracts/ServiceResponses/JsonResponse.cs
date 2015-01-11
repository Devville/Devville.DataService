// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonResponse.cs" company="Devville">
//   Copyright © 2015 All Right Reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Devville.DataService.Contracts.ServiceResponses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

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
        /// <param name="templatePath">
        /// The template path.
        /// </param>
        /// <param name="templateId">
        /// The template identifier.
        /// </param>
        /// <param name="containerId">
        /// The container identifier.
        /// </param>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>1/12/2015</created>
        public JsonResponse(
            string templatePath,
            string templateId,
            string containerId,
            object model,
            JsonResponseStatus status)
        {
            this.TemplateId = templateId;
            this.TemplatePath = templatePath;
            this.ContainerId = containerId;
            this.Model = model;
            this.ServiceStatus = status;
            this.Extras = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonResponse"/> class.
        /// </summary>
        /// <param name="templatePath">
        /// The template path.
        /// </param>
        /// <param name="templateId">
        /// The template identifier.
        /// </param>
        /// <param name="containerId">
        /// The container identifier.
        /// </param>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>1/12/2015</created>
        public JsonResponse(string templatePath, string templateId, string containerId, object model)
            : this(templatePath, templateId, containerId, model, new JsonResponseStatus(JsonOperationStatus.Succeeded))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonResponse"/> class.
        /// </summary>
        /// <param name="templateId">
        /// The template identifier.
        /// </param>
        /// <param name="containerId">
        /// The container identifier.
        /// </param>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>1/12/2015</created>
        public JsonResponse(string templateId, string containerId, object model)
            : this(null, templateId, containerId, model, new JsonResponseStatus(JsonOperationStatus.Succeeded))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonResponse"/> class.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>1/12/2015</created>
        public JsonResponse(object model)
            : this(null, null, model)
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the container identifier.
        /// </summary>
        /// <value>
        ///     The container identifier.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>1/12/2015</created>
        public string ContainerId { get; set; }

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

        /// <summary>
        ///     Gets or sets the template identifier.
        /// </summary>
        /// <value>
        ///     The template identifier.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>1/12/2015</created>
        public string TemplateId { get; set; }

        /// <summary>
        ///     Gets or sets the template path.
        /// </summary>
        /// <value>
        ///     The template path.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>12/28/2014</created>
        public string TemplatePath { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>1/12/2015</created>
        public void Render(HttpContext context)
        {
            this.Render(context, false);
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="renderModelOnly">if set to <c>true</c> [render model only].</param>
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
                string templatePath = context.Request.QueryString["TemplatePath"];
                if (!string.IsNullOrWhiteSpace(templatePath))
                {
                    this.TemplatePath = templatePath;
                }

                string templateId = context.Request.QueryString["TemplateId"];
                if (!string.IsNullOrWhiteSpace(templateId))
                {
                    this.TemplateId = templateId;
                }

                string containerId = context.Request.QueryString["ContainerId"];
                if (!string.IsNullOrWhiteSpace(containerId))
                {
                    this.ContainerId = containerId;
                }

                IEnumerable<string> extraQueryStringNames =
                    context.Request.QueryString.AllKeys.Where(
                        q =>
                        !string.IsNullOrWhiteSpace(q)
                        && q.StartsWith(ExtrasPrefix, StringComparison.InvariantCultureIgnoreCase));
                foreach (string extraQueryStringName in extraQueryStringNames)
                {
                    string extraKey = extraQueryStringName.Replace(ExtrasPrefix, string.Empty);
                    if (this.Extras.ContainsKey(extraKey))
                    {
                        extraKey = extraKey + "_";
                    }

                    this.Extras.Add(extraKey, context.Request[extraQueryStringName]);
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