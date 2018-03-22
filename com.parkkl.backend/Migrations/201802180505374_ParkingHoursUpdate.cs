namespace com.parkkl.backend.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ParkingHoursUpdate : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Penalties", "ExceededHours", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Penalties", "ExceededHours", c => c.Int(nullable: false));
        }
    }
}
