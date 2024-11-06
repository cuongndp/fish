namespace fish.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDoctorToBooking : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Bookings", "UserId", "dbo.Users");
            AddColumn("dbo.Bookings", "DoctorId", c => c.Int());
            AddColumn("dbo.Bookings", "User_Id", c => c.Int());
            CreateIndex("dbo.Bookings", "DoctorId");
            CreateIndex("dbo.Bookings", "User_Id");
            AddForeignKey("dbo.Bookings", "DoctorId", "dbo.Users", "Id");
            AddForeignKey("dbo.Bookings", "User_Id", "dbo.Users", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Bookings", "User_Id", "dbo.Users");
            DropForeignKey("dbo.Bookings", "DoctorId", "dbo.Users");
            DropIndex("dbo.Bookings", new[] { "User_Id" });
            DropIndex("dbo.Bookings", new[] { "DoctorId" });
            DropColumn("dbo.Bookings", "User_Id");
            DropColumn("dbo.Bookings", "DoctorId");
            AddForeignKey("dbo.Bookings", "UserId", "dbo.Users", "Id", cascadeDelete: true);
        }
    }
}
