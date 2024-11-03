namespace fish.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateBookingModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Bookings", "NgayHen", c => c.DateTime(nullable: false));
            AddColumn("dbo.Bookings", "GioHen", c => c.Time(nullable: false, precision: 7));
            AddColumn("dbo.Bookings", "MoTa", c => c.String());
            DropColumn("dbo.Bookings", "AppointmentDate");
            DropColumn("dbo.Bookings", "AppointmentTime");
            DropColumn("dbo.Bookings", "Description");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Bookings", "Description", c => c.String());
            AddColumn("dbo.Bookings", "AppointmentTime", c => c.String());
            AddColumn("dbo.Bookings", "AppointmentDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.Bookings", "MoTa");
            DropColumn("dbo.Bookings", "GioHen");
            DropColumn("dbo.Bookings", "NgayHen");
        }
    }
}
