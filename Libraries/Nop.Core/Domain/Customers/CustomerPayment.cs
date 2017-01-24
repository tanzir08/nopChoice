using System.Collections.Generic;
using Nop.Core.Domain.Security;
using System;

namespace Nop.Core.Domain.Customers
{
    /// <summary>
    /// Represents a customer payment
    /// </summary>
    public partial class CustomerPayment : BaseEntity
    {
        /// <summary>
        /// Gets or sets the customer payment time
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets a value of customer payment
        /// </summary>
        public decimal Amount { get; set; }
    }

}