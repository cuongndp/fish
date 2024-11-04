namespace fish.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddServiceAndUpdateRelations : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Services",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenDichVu = c.String(),
                        MoTaDichVu = c.String(),
                        Gia = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Bookings", "ServiceId", c => c.Int(nullable: false));
            CreateIndex("dbo.Bookings", "ServiceId");
            AddForeignKey("dbo.Bookings", "ServiceId", "dbo.Services", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Bookings", "ServiceId", "dbo.Services");
            DropIndex("dbo.Bookings", new[] { "ServiceId" });
            DropColumn("dbo.Bookings", "ServiceId");
            DropTable("dbo.Services");
        }
    }
}
