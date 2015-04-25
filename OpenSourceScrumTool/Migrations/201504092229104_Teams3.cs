namespace OpenSourceScrumTool.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Teams3 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TeamMembers", "Team_Id", "dbo.Teams");
            DropIndex("dbo.TeamMembers", new[] { "Team_Id" });
            AlterColumn("dbo.TeamMembers", "Team_Id", c => c.Int(nullable: false));
            CreateIndex("dbo.TeamMembers", "Team_Id");
            AddForeignKey("dbo.TeamMembers", "Team_Id", "dbo.Teams", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TeamMembers", "Team_Id", "dbo.Teams");
            DropIndex("dbo.TeamMembers", new[] { "Team_Id" });
            AlterColumn("dbo.TeamMembers", "Team_Id", c => c.Int());
            CreateIndex("dbo.TeamMembers", "Team_Id");
            AddForeignKey("dbo.TeamMembers", "Team_Id", "dbo.Teams", "Id");
        }
    }
}
