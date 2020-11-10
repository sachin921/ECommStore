namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class customimage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ShoppingCartItem", "CustomeImage", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ShoppingCartItem", "CustomeImage");
        }
    }
}
