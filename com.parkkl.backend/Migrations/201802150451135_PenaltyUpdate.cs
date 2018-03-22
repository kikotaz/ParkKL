namespace com.parkkl.backend.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PenaltyUpdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Penalties", "PenaltyPaid", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Penalties", "PenaltyPaid");
        }
    }
}
