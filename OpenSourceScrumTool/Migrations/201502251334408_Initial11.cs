namespace OpenSourceScrumTool.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial11 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Features", "Weight", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Features", "Weight", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
