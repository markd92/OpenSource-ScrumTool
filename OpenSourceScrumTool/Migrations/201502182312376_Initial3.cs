namespace OpenSourceScrumTool.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProjectAccesses", "Priority", c => c.Int(nullable: false));
            DropColumn("dbo.Projects", "Priority");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Projects", "Priority", c => c.Int(nullable: false));
            DropColumn("dbo.ProjectAccesses", "Priority");
        }
    }
}
