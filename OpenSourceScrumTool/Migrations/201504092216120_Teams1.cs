namespace OpenSourceScrumTool.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Teams1 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.TeamMembers");
            AlterColumn("dbo.TeamMembers", "Id", c => c.Guid(nullable: false, identity: true, defaultValueSql: "newid()"));
            AddPrimaryKey("dbo.TeamMembers", "Id");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.TeamMembers");
            AlterColumn("dbo.TeamMembers", "Id", c => c.Guid(nullable: false));
            AddPrimaryKey("dbo.TeamMembers", "Id");
        }
    }
}
