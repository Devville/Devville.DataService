// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IServiceOperation.cs" company="Devville">
//   Copyright © 2015 All Right Reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Devville.DataService.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Web;

    /// <summary>
    ///     The ServiceOperation interface.
    /// </summary>
    public interface IServiceOperation
    {
        #region Public Properties

        /// <summary>
        ///     Gets the operation name to be used in the data service.
        ///     Make SURE that you use a unique name for your operation.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        string Name { get; }

        ///// <summary>
        ///// Gets the parameters.
        ///// </summary>
        ///// <value>
        ///// The parameters.
        ///// </value>
        ///// <author>Ahmed Magdy (amagdy@sure.com.sa)</author>
        ///// <created>1/13/2015</created>
        //Dictionary<string, Type> Parameters { get; }

            #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Executes the current service operation.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The Service Response
        /// </returns>
        IServiceResponse Execute(HttpContext context);

        #endregion
    }
}