namespace OpenSourceScrumTool.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Teams : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Tasks", newName: "ScrumTasks");
            DropForeignKey("dbo.ProjectAccesses", "ProjectId", "dbo.Projects");
            DropForeignKey("dbo.ProjectAccesses", "User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.ProjectAccesses", new[] { "ProjectId" });
            DropIndex("dbo.ProjectAccesses", new[] { "User_Id" });
            CreateTable(
                "dbo.Teams",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TeamName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TeamMembers",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Role = c.Int(nullable: false),
                        Team_Id = c.Int(),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Teams", t => t.Team_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.Team_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.TeamProjects",
                c => new
                    {
                        Team_Id = c.Int(nullable: false),
                        Project_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Team_Id, t.Project_Id })
                .ForeignKey("dbo.Teams", t => t.Team_Id, cascadeDelete: true)
                .ForeignKey("dbo.Projects", t => t.Project_Id, cascadeDelete: true)
                .Index(t => t.Team_Id)
                .Index(t => t.Project_Id);
            
            AddColumn("dbo.Projects", "ApplicationUser_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.AspNetUsers", "Fullname", c => c.String());
            AddColumn("dbo.AspNetUsers", "ApplicationUser_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.Projects", "ApplicationUser_Id");
            CreateIndex("dbo.AspNetUsers", "ApplicationUser_Id");
            AddForeignKey("dbo.AspNetUsers", "ApplicationUser_Id", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Projects", "ApplicationUser_Id", "dbo.AspNetUsers", "Id");
            DropTable("dbo.ProjectAccesses");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ProjectAccesses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProjectId = c.Int(nullable: false),
                        Priority = c.Int(nullable: false),
                        User_Id = c.String(maxLength: 128),
                        AccessType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            DropForeignKey("dbo.TeamProjects", "Project_Id", "dbo.Projects");
            DropForeignKey("dbo.TeamProjects", "Team_Id", "dbo.Teams");
            DropForeignKey("dbo.TeamMembers", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Projects", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.TeamMembers", "Team_Id", "dbo.Teams");
            DropIndex("dbo.TeamProjects", new[] { "Project_Id" });
            DropIndex("dbo.TeamProjects", new[] { "Team_Id" });
            DropIndex("dbo.AspNetUsers", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.TeamMembers", new[] { "User_Id" });
            DropIndex("dbo.TeamMembers", new[] { "Team_Id" });
            DropIndex("dbo.Projects", new[] { "ApplicationUser_Id" });
            DropColumn("dbo.AspNetUsers", "ApplicationUser_Id");
            DropColumn("dbo.AspNetUsers", "Fullname");
            DropColumn("dbo.Projects", "ApplicationUser_Id");
            DropTable("dbo.TeamProjects");
            DropTable("dbo.TeamMembers");
            DropTable("dbo.Teams");
            CreateIndex("dbo.ProjectAccesses", "User_Id");
            CreateIndex("dbo.ProjectAccesses", "ProjectId");
            AddForeignKey("dbo.ProjectAccesses", "User_Id", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.ProjectAccesses", "ProjectId", "dbo.Projects", "Id", cascadeDelete: true);
            RenameTable(name: "dbo.ScrumTasks", newName: "Tasks");
        }
    }
}
