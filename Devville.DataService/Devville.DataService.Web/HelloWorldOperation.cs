// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HelloWorldOperation.cs" company="">
//   
// </copyright>
// <summary>
//   The hello world operation.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Devville.DataService.Web
{
    using System;
    using System.Collections.Generic;
    using System.Web;

    using Devville.DataService.Contracts;
    using Devville.DataService.ServiceResponses;

    /// <summary>
    /// The hello world operation.
    /// </summary>
    public class HelloWorldOperation : IServiceOperation
    {
        #region Public Properties

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description
        {
            get
            {
                return "This is a hello world operation";
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name
        {
            get
            {
                return "ShowHelloWorld";
            }
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        public Dictionary<string, string> Parameters
        {
            get
            {
                var parameters = new Dictionary<string, string>();
                parameters["Name"] = "string: This a sample parameter.";
                return parameters;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Executes the current service operation.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        /// The Service Response
        /// </returns>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        public IServiceResponse Execute(HttpContext context)
        {
            string name = context.Request["Name"] ?? "World";
            var response =
                new JsonResponse(
                    new
                        {
                            Message = "Hello " + name, 
                            Data = new List<string> { "Hello", name }, 
                            TimeStamp = DateTime.Now.ToString("O")
                        });

            return response;
        }

        #endregion
    }
}