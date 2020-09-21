namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomiseProduct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Product", "IsCustomiseProduct", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Product", "IsCustomiseProduct");
        }
    }
}
