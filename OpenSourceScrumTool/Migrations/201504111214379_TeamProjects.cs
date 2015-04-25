namespace OpenSourceScrumTool.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TeamProjects : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TeamProjects", "Team_Id", "dbo.Teams");
            DropForeignKey("dbo.TeamProjects", "Project_Id", "dbo.Projects");
            DropIndex("dbo.TeamProjects", new[] { "Team_Id" });
            DropIndex("dbo.TeamProjects", new[] { "Project_Id" });
            DropTable("dbo.TeamProjects");
            CreateTable(
                "dbo.TeamProjects",
                c => new
                    {
                        ProjectId = c.Int(nullable: false),
                        TeamId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ProjectId, t.TeamId })
                .ForeignKey("dbo.Teams", t => t.TeamId, cascadeDelete: true)
                .ForeignKey("dbo.Projects", t => t.ProjectId, cascadeDelete: true)
                .Index(t => t.ProjectId)
                .Index(t => t.TeamId);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TeamProjects", "ProjectId", "dbo.Projects");
            DropForeignKey("dbo.TeamProjects", "TeamId", "dbo.Teams");
            DropIndex("dbo.TeamProjects", new[] { "TeamId" });
            DropIndex("dbo.TeamProjects", new[] { "ProjectId" });
            DropTable("dbo.TeamProjects");
            CreateTable(
                "dbo.TeamProjects",
                c => new
                    {
                        Team_Id = c.Int(nullable: false),
                        Project_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Team_Id, t.Project_Id });
            CreateIndex("dbo.TeamProjects", "Project_Id");
            CreateIndex("dbo.TeamProjects", "Team_Id");
            AddForeignKey("dbo.TeamProjects", "Project_Id", "dbo.Projects", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TeamProjects", "Team_Id", "dbo.Teams", "Id", cascadeDelete: true);
        }
    }
}
