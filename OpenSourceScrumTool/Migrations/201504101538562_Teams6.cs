namespace OpenSourceScrumTool.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Teams6 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Projects", "User_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.Projects", "User_Id");
            AddForeignKey("dbo.Projects", "User_Id", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Projects", "User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Projects", new[] { "User_Id" });
            DropColumn("dbo.Projects", "User_Id");
        }
    }
}
