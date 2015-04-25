namespace OpenSourceScrumTool.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial6 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Sprints",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Iteration = c.Int(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SprintFeatures",
                c => new
                    {
                        Sprint_Id = c.Int(nullable: false),
                        Feature_ID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Sprint_Id, t.Feature_ID })
                .ForeignKey("dbo.Sprints", t => t.Sprint_Id, cascadeDelete: true)
                .ForeignKey("dbo.Features", t => t.Feature_ID, cascadeDelete: true)
                .Index(t => t.Sprint_Id)
                .Index(t => t.Feature_ID);
            
            AddColumn("dbo.Projects", "SprintDuration", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SprintFeatures", "Feature_ID", "dbo.Features");
            DropForeignKey("dbo.SprintFeatures", "Sprint_Id", "dbo.Sprints");
            DropIndex("dbo.SprintFeatures", new[] { "Feature_ID" });
            DropIndex("dbo.SprintFeatures", new[] { "Sprint_Id" });
            DropColumn("dbo.Projects", "SprintDuration");
            DropTable("dbo.SprintFeatures");
            DropTable("dbo.Sprints");
        }
    }
}
