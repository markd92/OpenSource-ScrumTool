namespace OpenSourceScrumTool.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TeamVelocity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Teams", "Velocity", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Teams", "Velocity");
        }
    }
}
