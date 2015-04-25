namespace OpenSourceScrumTool.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Teams8 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Users_Colleagues", "Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Users_Colleagues", "Colleague_ID", "dbo.AspNetUsers");
            DropIndex("dbo.Users_Colleagues", new[] { "Id" });
            DropIndex("dbo.Users_Colleagues", new[] { "Colleague_ID" });
            CreateTable(
                "dbo.UserColleagues",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ColleagueId = c.String(nullable: false, maxLength: 128),
                        ApplicationUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ColleagueId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id)
                .Index(t => t.ColleagueId)
                .Index(t => t.ApplicationUser_Id);
            
            DropTable("dbo.Users_Colleagues");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Users_Colleagues",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Colleague_ID = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Id, t.Colleague_ID });
            
            DropForeignKey("dbo.UserColleagues", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserColleagues", "ColleagueId", "dbo.AspNetUsers");
            DropIndex("dbo.UserColleagues", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.UserColleagues", new[] { "ColleagueId" });
            DropTable("dbo.UserColleagues");
            CreateIndex("dbo.Users_Colleagues", "Colleague_ID");
            CreateIndex("dbo.Users_Colleagues", "Id");
            AddForeignKey("dbo.Users_Colleagues", "Colleague_ID", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Users_Colleagues", "Id", "dbo.AspNetUsers", "Id");
        }
    }
}
