namespace fish.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateServiceIdToNullable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Bookings", "ServiceId", "dbo.Services");
            DropIndex("dbo.Bookings", new[] { "ServiceId" });
            AlterColumn("dbo.Bookings", "ServiceId", c => c.Int());
            CreateIndex("dbo.Bookings", "ServiceId");
            AddForeignKey("dbo.Bookings", "ServiceId", "dbo.Services", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Bookings", "ServiceId", "dbo.Services");
            DropIndex("dbo.Bookings", new[] { "ServiceId" });
            AlterColumn("dbo.Bookings", "ServiceId", c => c.Int(nullable: false));
            CreateIndex("dbo.Bookings", "ServiceId");
            AddForeignKey("dbo.Bookings", "ServiceId", "dbo.Services", "Id", cascadeDelete: true);
        }
    }
}
