namespace com.parkkl.backend.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WalletParkingUpdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Parkings", "HasPenalty", c => c.Boolean(nullable: false));
            AddColumn("dbo.Penalties", "ParkingId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Penalties", "ParkingId");
            DropColumn("dbo.Parkings", "HasPenalty");
        }
    }
}
