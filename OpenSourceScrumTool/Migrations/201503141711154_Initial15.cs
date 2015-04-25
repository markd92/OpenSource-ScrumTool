namespace OpenSourceScrumTool.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial15 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.SprintFeatures", new[] { "Feature_ID" });
            CreateIndex("dbo.SprintFeatures", "Feature_Id");
        }
        
        public override void Down()
        {
            DropIndex("dbo.SprintFeatures", new[] { "Feature_Id" });
            CreateIndex("dbo.SprintFeatures", "Feature_ID");
        }
    }
}
