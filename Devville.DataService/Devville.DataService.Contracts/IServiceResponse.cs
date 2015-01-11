// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IServiceResponse.cs" company="Devville">
//   Copyright © 2015 All Right Reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Devville.DataService.Contracts
{
    using System.Web;

    /// <summary>
    ///     The ServiceResponse interface.
    /// </summary>
    public interface IServiceResponse
    {
        #region Public Properties

        /// <summary>
        ///     Gets the type of the content.
        /// </summary>
        /// <value>
        ///     The type of the content.
        /// </value>
        string ContentType { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        void Render(HttpContext context);

        #endregion
    }
}