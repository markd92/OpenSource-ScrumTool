namespace OpenSourceScrumTool.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial9 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "PreferedView", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "PreferedView");
        }
    }
}
