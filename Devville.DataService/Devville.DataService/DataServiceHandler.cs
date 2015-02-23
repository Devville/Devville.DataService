// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataServiceHandler.cs" company="Devville">
//   Copyright © 2015 All Right Reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Devville.DataService
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Principal;
    using System.Web;

    using Devville.DataService.Contracts;
    using Devville.DataService.Contracts.ServiceResponses;

    /// <summary>
    /// The data service handler.
    /// Source code and more: https://github.com/Devville/Devville.DataService
    /// </summary>
    public class DataServiceHandler : IHttpHandler
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="DataServiceHandler" /> class.
        ///     Initializes the <see cref="DataServiceHandler" /> class.
        /// </summary>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>12/28/2014</created>
        static DataServiceHandler()
        {
            Type serviceOperationInterface = typeof(IServiceOperation);

            List<Type> serviceOperationTypes =
                LoadBinAssemblies()
                    .SelectMany(
                        assembly =>
                        assembly.GetTypes()
                            .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition && !t.IsInterface)
                            .Where(serviceOperationInterface.IsAssignableFrom))
                    .ToList();
            Dictionary<Type, IServiceOperation> serviceOperations = serviceOperationTypes.ToDictionary(
                t => t,
                t => (IServiceOperation)Activator.CreateInstance(t));

            ServiceOperations = serviceOperations;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the list of the available service operations.
        /// </summary>
        /// <value>
        ///     The service operations.
        /// </value>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>12/28/2014</created>
        public static Dictionary<Type, IServiceOperation> ServiceOperations { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler" /> instance.
        /// </summary>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>12/28/2014</created>
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Loads the bin assemblies.
        /// </summary>
        /// <returns>List of assemblies in the bin</returns>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>1/12/2015</created>
        public static IEnumerable<Assembly> LoadBinAssemblies()
        {
            var assemblies = new List<Assembly>();

            WindowsImpersonationContext impersonationContext = WindowsIdentity.Impersonate(IntPtr.Zero);

            string binPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
            string[] dlls = Directory.GetFiles(binPath, "*.dll", SearchOption.AllDirectories);

            assemblies.AddRange(dlls.Select(Assembly.LoadFile));
            impersonationContext.Undo();

            return assemblies;
        }

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the
        ///     <see cref="T:System.Web.IHttpHandler"/> interface.
        /// </summary>
        /// <param name="context">
        /// An <see cref="T:System.Web.HttpContext"/> object that provides references to the intrinsic
        ///     server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.
        /// </param>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>12/28/2014</created>
        /// <exception cref="System.IndexOutOfRangeException">
        /// Operation can't be found.
        /// </exception>
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                string operationName = context.Request.QueryString["op"];
                if (string.IsNullOrWhiteSpace(operationName))
                {
                    var operationsList =
                        ServiceOperations.Select(
                            o =>
                            new
                                {
                                    o.Value.Name,
                                    o.Value.Description,
                                    Class = o.Key.FullName,
                                    Assembly = o.Key.Assembly.FullName,
                                    OperationUrl = string.Format("{0}?op={1}", context.Request.Url, o.Value.Name)
                                });

                    new JsonResponse(new { Operations = operationsList }).Render(context, true);

                    return;
                }

                List<KeyValuePair<Type, IServiceOperation>> operations =
                    ServiceOperations.Where(
                        o => o.Value.Name.Equals(operationName, StringComparison.InvariantCultureIgnoreCase)).ToList();

                if (operations.Count == 0)
                {
                    throw new IndexOutOfRangeException(string.Format("Operation {0} can't be found!", operationName));
                }

                // Always execute the first one if there are any duplicates.
                var service = operations.First().Value;
                var serviceResponse = service.Execute(context);
                serviceResponse.Render(context);
            }
            catch (Exception exception)
            {
                var responseStatus = new JsonResponseStatus(JsonOperationStatus.Failed, exception.ToString());
                var exceptionResponse = new JsonResponse(null, responseStatus);
                exceptionResponse.Render(context);
            }
        }

        #endregion
    }
}