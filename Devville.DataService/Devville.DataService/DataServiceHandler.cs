// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataServiceHandler.cs" company="Devville">
//   Copyright © 2015 All Right Reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Devville.DataService
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Principal;
    using System.Web;

    using Devville.DataService.Contracts;
    using Devville.DataService.Contracts.ServiceResponses;

    using NLog;

    /// <summary>
    ///     The data service handler.
    ///     Source code and more: https://github.com/Devville/Devville.DataService
    /// </summary>
    public class DataServiceHandler : IHttpHandler
    {
        #region Static Fields

        /// <summary>
        ///     The logger
        /// </summary>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>3/12/2015</created>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="DataServiceHandler" /> class.
        ///     Initializes the <see cref="DataServiceHandler" /> class.
        /// </summary>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>12/28/2014</created>
        static DataServiceHandler()
        {
            try
            {
                Type serviceOperationInterface = typeof(IServiceOperation);

                var loadedAssemblied = new List<Assembly>();

                WindowsImpersonationContext impersonationContext = WindowsIdentity.Impersonate(IntPtr.Zero);

                string binPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
                string[] dlls = Directory.GetFiles(binPath, "*.dll", SearchOption.AllDirectories);

                foreach (string dll in dlls)
                {
                    Logger.Trace("Assembly: {0}", dll);
                    Assembly assmbly = Assembly.LoadFile(dll);
                    if (assmbly == null)
                    {
                        continue;
                    }

                    Logger.Trace("Assembly {0} loaded", assmbly.FullName);
                    loadedAssemblied.Add(assmbly);
                }

                impersonationContext.Undo();

                Logger.Trace("Total number of loaded assembies are: {0}", loadedAssemblied.Count);

                ServiceOperations = new Dictionary<Type, IServiceOperation>();

                foreach (Assembly loadedAssembly in loadedAssemblied)
                {
                    Type[] publicTypes = loadedAssembly.GetExportedTypes();
                    Logger.Trace(
                        "Assembly '{0}' has {1} public types", 
                        loadedAssembly.GetName().Name, 
                        publicTypes.Length);
                    foreach (Type publicType in publicTypes)
                    {
                        if (!publicType.IsClass || publicType.IsAbstract || publicType.IsGenericTypeDefinition
                            || publicType.IsInterface)
                        {
                            continue;
                        }

                        if (!serviceOperationInterface.IsAssignableFrom(publicType))
                        {
                            continue;
                        }

                        var instance = (IServiceOperation)Activator.CreateInstance(publicType);
                        ServiceOperations.Add(publicType, instance);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Fatal("Can't load assemblies", exception);
                var loadException = exception as ReflectionTypeLoadException;
                if (loadException == null)
                {
                    throw;
                }

                foreach (Exception loaderException in loadException.LoaderExceptions)
                {
                    Logger.Fatal("Loader exception: " + loaderException.Message, loaderException);
                }

                throw loadException.LoaderExceptions.First();
            }
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
                IServiceOperation service = operations.First().Value;
                IServiceResponse serviceResponse = service.Execute(context);
                serviceResponse.Render(context);
            }
            catch (Exception exception)
            {
                exception.Data["TimeStamp"] = DateTime.Now.ToString("O");
                Logger.Fatal("Unexpected error: " + exception.Message, exception);

                string exceptionString = GetExceptionString(exception);

                var responseStatus = new JsonResponseStatus(JsonOperationStatus.Failed, exceptionString);
                var exceptionResponse = new JsonResponse(null, responseStatus);
                exceptionResponse.Render(context);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the exception string.
        /// </summary>
        /// <param name="exception">
        /// The exception.
        /// </param>
        /// <returns>
        /// The exception string with the data.
        /// </returns>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>3/18/2015</created>
        private static string GetExceptionString(Exception exception)
        {
            string exceptionString = exception.ToString();
            if (exception.Data.Count > 0)
            {
                exceptionString += "\nException Data:\n";
            }

            return exception.Data.Cast<DictionaryEntry>()
                .Aggregate(
                    exceptionString, 
                    (current, entry) => current + string.Format("{0}: {1}\n", entry.Key, entry.Value));
        }

        #endregion
    }
}