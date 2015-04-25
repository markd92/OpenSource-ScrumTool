namespace OpenSourceScrumTool.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial8 : DbMigration
    {
        public override void Up()
        {
            //DropForeignKey("dbo.Sprints", "Project_Id", "dbo.Projects");
            //DropIndex("dbo.Sprints", new[] { "Project_Id" });
            //AlterColumn("dbo.Sprints", "Project_Id", c => c.Int(nullable: false));
            //CreateIndex("dbo.Sprints", "Project_Id");
            //AddForeignKey("dbo.Sprints", "Project_Id", "dbo.Projects", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            //DropForeignKey("dbo.Sprints", "Project_Id", "dbo.Projects");
            //DropIndex("dbo.Sprints", new[] { "Project_Id" });
            //AlterColumn("dbo.Sprints", "Project_Id", c => c.Int());
            //CreateIndex("dbo.Sprints", "Project_Id");
            //AddForeignKey("dbo.Sprints", "Project_Id", "dbo.Projects", "Id");
        }
    }
}
