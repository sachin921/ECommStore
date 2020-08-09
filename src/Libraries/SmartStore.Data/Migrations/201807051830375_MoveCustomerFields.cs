namespace SmartStore.Data.Migrations
{
    using System;
	using System.Data.Entity.Migrations;
    using System.Linq;
    using SmartStore.Core.Domain.Common;
    using SmartStore.Core.Domain.Customers;
    using SmartStore.Data.Setup;
	using SmartStore.Data.Utilities;

	public partial class MoveCustomerFields : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
	{
        public override void Up()
        {
            AddColumn("dbo.Customer", "Salutation", c => c.String(maxLength: 50));
            AddColumn("dbo.Customer", "Title", c => c.String(maxLength: 100));
            AddColumn("dbo.Customer", "FirstName", c => c.String(maxLength: 225));
            AddColumn("dbo.Customer", "LastName", c => c.String(maxLength: 225));
            AddColumn("dbo.Customer", "FullName", c => c.String(maxLength: 450));
            AddColumn("dbo.Customer", "Company", c => c.String(maxLength: 255));
            AddColumn("dbo.Customer", "CustomerNumber", c => c.String(maxLength: 100));
            AddColumn("dbo.Customer", "BirthDate", c => c.DateTime());
            CreateIndex("dbo.Customer", "FullName", name: "IX_Customer_FullName");
            CreateIndex("dbo.Customer", "Company", name: "IX_Customer_Company");
            CreateIndex("dbo.Customer", "CustomerNumber", name: "IX_Customer_CustomerNumber", unique: false);
            CreateIndex("dbo.Customer", "BirthDate", name: "IX_Customer_BirthDate");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Customer", "IX_Customer_BirthDate");
            DropIndex("dbo.Customer", "IX_Customer_CustomerNumber");
            DropIndex("dbo.Customer", "IX_Customer_Company");
            DropIndex("dbo.Customer", "IX_Customer_FullName");
            DropColumn("dbo.Customer", "BirthDate");
            DropColumn("dbo.Customer", "CustomerNumber");
            DropColumn("dbo.Customer", "Company");
            DropColumn("dbo.Customer", "FullName");
            DropColumn("dbo.Customer", "LastName");
            DropColumn("dbo.Customer", "FirstName");
            DropColumn("dbo.Customer", "Title");
            DropColumn("dbo.Customer", "Salutation");
        }

		public bool RollbackOnFailure => true;

		public void Seed(SmartObjectContext context)
		{
            context.MigrateLocaleResources(MigrateLocaleResources);

            // Perf
            var numDeletedAttrs = DataMigrator.DeleteGuestCustomerGenericAttributes(context, TimeSpan.FromDays(30));
            var numDeletedCustomers = DataMigrator.DeleteGuestCustomers(context, TimeSpan.FromDays(30));

            var candidates = new[] { "Title", "FirstName", "LastName", "Company", "CustomerNumber", "DateOfBirth" };
            var numUpdatedCustomers = DataMigrator.MoveCustomerFields(context, UpdateCustomer, candidates);
		}

        private static void UpdateCustomer(Customer customer, GenericAttribute attr)
        {
            switch (attr.Key)
            {
                case "Title":
                    customer.Title = attr.Value?.Truncate(100);
                    break;
                case "FirstName":
                    customer.FirstName = attr.Value?.Truncate(225);
                    break;
                case "LastName":
                    customer.LastName = attr.Value?.Truncate(225);
                    break;
                case "Company":
                    customer.Company = attr.Value?.Truncate(255);
                    break;
                case "CustomerNumber":
                    customer.CustomerNumber = attr.Value?.Truncate(100);
                    break;
                case "DateOfBirth":
                    customer.BirthDate = attr.Value?.Convert<DateTime?>();
                    break;
            }

            // Update FullName
            var parts = new[] { customer.Title, customer.FirstName, customer.LastName };
            customer.FullName = string.Join(" ", parts.Where(x => x.HasValue())).NullEmpty();
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.Delete(
				"Admin.Customers.Customers.List.SearchFirstName",
				"Admin.Customers.Customers.List.SearchFirstName.Hint",
				"Admin.Customers.Customers.List.SearchLastName",
				"Admin.Customers.Customers.List.SearchLastName.Hint",
				"Admin.Customers.Customers.List.SearchCompany",
				"Admin.Customers.Customers.List.SearchCompany.Hint");

			builder.AddOrUpdate("Admin.Customers.Customers.List.SearchTerm",
				"Search term",
				"Suchbegriff",
				"Name or company",
				"Name oder Firma");
		}
	}
}
