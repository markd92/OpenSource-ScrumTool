namespace OpenSourceScrumTool.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial5 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Features", "Weight", c => c.Single(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Features", "Weight", c => c.Int(nullable: false));
        }
    }
}
