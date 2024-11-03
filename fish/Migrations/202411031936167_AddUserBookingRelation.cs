namespace fish.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserBookingRelation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Bookings", "UserId", c => c.Int(nullable: false));
            CreateIndex("dbo.Bookings", "UserId");
            AddForeignKey("dbo.Bookings", "UserId", "dbo.Users", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Bookings", "UserId", "dbo.Users");
            DropIndex("dbo.Bookings", new[] { "UserId" });
            DropColumn("dbo.Bookings", "UserId");
        }
    }
}
