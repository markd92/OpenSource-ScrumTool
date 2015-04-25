namespace OpenSourceScrumTool.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Teams10 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UserColleagues", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.UserColleagues", new[] { "ApplicationUser_Id" });
            RenameColumn(table: "dbo.UserColleagues", name: "ApplicationUser_Id", newName: "UserId");
            DropPrimaryKey("dbo.UserColleagues");
            AlterColumn("dbo.UserColleagues", "UserId", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.UserColleagues", new[] { "ColleagueId", "UserId" });
            CreateIndex("dbo.UserColleagues", "UserId");
            AddForeignKey("dbo.UserColleagues", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: false);
            DropColumn("dbo.UserColleagues", "Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UserColleagues", "Id", c => c.Long(nullable: false, identity: true));
            DropForeignKey("dbo.UserColleagues", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.UserColleagues", new[] { "UserId" });
            DropPrimaryKey("dbo.UserColleagues");
            AlterColumn("dbo.UserColleagues", "UserId", c => c.String(maxLength: 128));
            AddPrimaryKey("dbo.UserColleagues", "Id");
            RenameColumn(table: "dbo.UserColleagues", name: "UserId", newName: "ApplicationUser_Id");
            CreateIndex("dbo.UserColleagues", "ApplicationUser_Id");
            AddForeignKey("dbo.UserColleagues", "ApplicationUser_Id", "dbo.AspNetUsers", "Id");
        }
    }
}
