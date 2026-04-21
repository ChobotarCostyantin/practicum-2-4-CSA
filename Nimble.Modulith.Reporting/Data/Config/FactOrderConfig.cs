using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nimble.Modulith.Reporting.Models;

namespace Nimble.Modulith.Reporting.Data.Config;
public class FactOrderConfig : IEntityTypeConfiguration<FactOrder>
{
    public void Configure(EntityTypeBuilder<FactOrder> builder)
    {
        builder.ToTable("FactOrders");
        builder.HasKey(f => new { f.OrderId, f.OrderItemId });
    }
}