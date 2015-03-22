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
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Principal;
    using System.Web;

    using Devville.DataService.Contracts;
    using Devville.DataService.ServiceResponses;

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

        /// <summary>
        ///     The operations meta data
        /// </summary>
        /// <author>Ahmed Magdy (ahmed.magdy@devville.net)</author>
        /// <created>3/18/2015</created>
        private static readonly List<OperationMetaData> OperationsMetaData = new List<OperationMetaData>();

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
                WindowsImpersonationContext impersonationContext = WindowsIdentity.Impersonate(IntPtr.Zero);

                string binPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");

                var dllPaths = new List<string>();

                string search = ConfigurationManager.AppSettings["Devville.DataService:DllsPatterns"] ?? "*.dll";

                dllPaths.AddRange(Directory.GetFiles(binPath, search, SearchOption.AllDirectories));
                AppDomain appDomain = AppDomain.CreateDomain("DevvilleDataServiceLoader");

                foreach (string dll in dllPaths.Distinct())
                {
                    Logger.Trace("Assembly: {0}", dll);

                    if (!dll.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase) || !File.Exists(dll))
                    {
                        continue;
                    }

                    byte[] fileBytes = File.ReadAllBytes(dll);

                    Assembly loadedAssembly = appDomain.Load(fileBytes);

                    Logger.Trace("Assembly {0} loaded", loadedAssembly.FullName);

                    IEnumerable<Type> serviceOperationTypes =
                        loadedAssembly.GetExportedTypes()
                            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition && !t.IsInterface)
                            .Where(serviceOperationInterface.IsAssignableFrom);
                    foreach (Type serviceOperationType in serviceOperationTypes)
                    {
                        var operationInstance = (IServiceOperation)Activator.CreateInstance(serviceOperationType);
                        string operationType = string.Format(
                            "{0}, {1}", 
                            serviceOperationType.FullName, 
                            loadedAssembly.FullName);
                        var operationMetaData = new OperationMetaData
                                                    {
                                                        Name = operationInstance.Name, 
                                                        Description = operationInstance.Description, 
                                                        OperationType = operationType, 
                                                        AssemblyPath = dll, 
                                                        Parameters = operationInstance.Parameters
                                                    };
                        OperationsMetaData.Add(operationMetaData);
                    }
                }

                AppDomain.Unload(appDomain);

                impersonationContext.Undo();
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
            WindowsImpersonationContext impersonationContext = WindowsIdentity.Impersonate(IntPtr.Zero);
            AppDomain runnerAppDomain = AppDomain.CreateDomain("DevvilleDataServiceRunner");

            try
            {
                string operationName = context.Request["op"];
                if (string.IsNullOrWhiteSpace(operationName))
                {
                    var allOperations = new JsonResponse(new { Operations = OperationsMetaData });
                    allOperations.Render(context, true);
                    AppDomain.Unload(runnerAppDomain);
                    impersonationContext.Undo();
                    return;
                }

                OperationMetaData operationMetaData =
                    OperationsMetaData.FirstOrDefault(
                        o => o.Name.Equals(operationName, StringComparison.InvariantCultureIgnoreCase));

                if (operationMetaData == null)
                {
                    throw new IndexOutOfRangeException(string.Format("Operation {0} can't be found!", operationName));
                }

                Logger.Trace(
                    "Operation {0} has been found with class type: {1}", 
                    operationMetaData.Name, 
                    operationMetaData.OperationType);
                if (!File.Exists(operationMetaData.AssemblyPath))
                {
                    throw new FileNotFoundException(
                        "Can't find the assembly with path: ", 
                        operationMetaData.AssemblyPath);
                }

                byte[] assemblyBytes = File.ReadAllBytes(operationMetaData.AssemblyPath);
                Assembly assembly = runnerAppDomain.Load(assemblyBytes);
                string operationTypeName = operationMetaData.OperationType.Split(',').FirstOrDefault();
                Type operationType = assembly.GetType(operationTypeName);

                if (operationType == null)
                {
                    throw new InvalidOperationException("Can't load operation of type: " + operationTypeName);
                }

                var operation = (IServiceOperation)Activator.CreateInstance(operationType);
                Logger.Trace("Operation {0} has been loaded", operation.Name);
                IServiceResponse serviceResponse = operation.Execute(context);
                Logger.Trace("Operation {0} has been executed", operation.Name);
                serviceResponse.Render(context);
                Logger.Trace("Operation {0} has been rendered", operation.Name);
                AppDomain.Unload(runnerAppDomain);
                impersonationContext.Undo();
            }
            catch (Exception exception)
            {
                exception.Data["TimeStamp"] = DateTime.Now.ToString("O");
                Logger.Fatal("Unexpected error: " + exception.Message, exception);

                string exceptionString = GetExceptionString(exception);

                var responseStatus = new JsonResponseStatus(JsonOperationStatus.Failed, exceptionString);
                var exceptionResponse = new JsonResponse(null, responseStatus);
                exceptionResponse.Render(context);

                AppDomain.Unload(runnerAppDomain);
                impersonationContext.Undo();
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