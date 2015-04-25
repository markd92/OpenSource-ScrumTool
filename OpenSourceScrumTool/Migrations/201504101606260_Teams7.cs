namespace OpenSourceScrumTool.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Teams7 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Users_Colleagues",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Colleague_ID = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Id, t.Colleague_ID })
                .ForeignKey("dbo.AspNetUsers", t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Colleague_ID)
                .Index(t => t.Id)
                .Index(t => t.Colleague_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Users_Colleagues", "Colleague_ID", "dbo.AspNetUsers");
            DropForeignKey("dbo.Users_Colleagues", "Id", "dbo.AspNetUsers");
            DropIndex("dbo.Users_Colleagues", new[] { "Colleague_ID" });
            DropIndex("dbo.Users_Colleagues", new[] { "Id" });
            DropTable("dbo.Users_Colleagues");
        }
    }
}
