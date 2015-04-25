namespace OpenSourceScrumTool.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Projects", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Projects", "ApplicationUser_Id1", "dbo.AspNetUsers");
            DropIndex("dbo.Projects", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.Projects", new[] { "ApplicationUser_Id1" });
            DropColumn("dbo.Projects", "ApplicationUser_Id");
            DropColumn("dbo.Projects", "ApplicationUser_Id1");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Projects", "ApplicationUser_Id1", c => c.String(maxLength: 128));
            AddColumn("dbo.Projects", "ApplicationUser_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.Projects", "ApplicationUser_Id1");
            CreateIndex("dbo.Projects", "ApplicationUser_Id");
            AddForeignKey("dbo.Projects", "ApplicationUser_Id1", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Projects", "ApplicationUser_Id", "dbo.AspNetUsers", "Id");
        }
    }
}
