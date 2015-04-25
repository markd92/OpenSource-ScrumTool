namespace OpenSourceScrumTool.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial4 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Features", "Weight", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Features", "Weight", c => c.Int());
        }
    }
}
