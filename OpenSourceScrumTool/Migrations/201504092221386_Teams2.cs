namespace OpenSourceScrumTool.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Teams2 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.TeamMembers");
            DropColumn("dbo.TeamMembers", "Id");
            AddColumn("dbo.TeamMembers", "Id", c => c.Long(nullable: false, identity: true));
            AddPrimaryKey("dbo.TeamMembers", "Id");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.TeamMembers");
            DropColumn("dbo.TeamMembers", "Id");
            AddColumn("dbo.TeamMembers", "Id", c => c.Guid(nullable: false, identity: true));
            AddPrimaryKey("dbo.TeamMembers", "Id");
        }
    }
}
