namespace OpenSourceScrumTool.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial7 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sprints", "Project_Id", c => c.Int());
            CreateIndex("dbo.Sprints", "Project_Id");
            AddForeignKey("dbo.Sprints", "Project_Id", "dbo.Projects", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Sprints", "Project_Id", "dbo.Projects");
            DropIndex("dbo.Sprints", new[] { "Project_Id" });
            DropColumn("dbo.Sprints", "Project_Id");
        }
    }
}
