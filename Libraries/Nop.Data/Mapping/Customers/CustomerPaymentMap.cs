using Nop.Core.Domain.Customers;

namespace Nop.Data.Mapping.Customers
{
    public partial class CustomerPaymentMap : NopEntityTypeConfiguration<CustomerPayment>
    {
        public CustomerPaymentMap()
        {
            this.ToTable("CustomerPayment");
            this.HasKey(cr => cr.Id);
            this.Property(cr => cr.CreatedOnUtc).IsRequired();
            this.Property(cr => cr.Amount);
        }
    }
}