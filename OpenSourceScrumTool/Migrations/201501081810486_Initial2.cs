namespace OpenSourceScrumTool.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ProjectAccesses", "Project_ID", "dbo.Projects");
            DropIndex("dbo.ProjectAccesses", new[] { "Project_ID" });
            RenameColumn(table: "dbo.Features", name: "Product_ID", newName: "ProjectId");
            RenameColumn(table: "dbo.Tasks", name: "PBI_ID", newName: "FeatureId");
            RenameColumn(table: "dbo.ProjectAccesses", name: "Project_ID", newName: "ProjectId");
            RenameIndex(table: "dbo.Features", name: "IX_Product_ID", newName: "IX_ProjectId");
            RenameIndex(table: "dbo.Tasks", name: "IX_PBI_ID", newName: "IX_FeatureId");
            AlterColumn("dbo.ProjectAccesses", "ProjectId", c => c.Int(nullable: false));
            CreateIndex("dbo.ProjectAccesses", "ProjectId");
            AddForeignKey("dbo.ProjectAccesses", "ProjectId", "dbo.Projects", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProjectAccesses", "ProjectId", "dbo.Projects");
            DropIndex("dbo.ProjectAccesses", new[] { "ProjectId" });
            AlterColumn("dbo.ProjectAccesses", "ProjectId", c => c.Int());
            RenameIndex(table: "dbo.Tasks", name: "IX_FeatureId", newName: "IX_PBI_ID");
            RenameIndex(table: "dbo.Features", name: "IX_ProjectId", newName: "IX_Product_ID");
            RenameColumn(table: "dbo.ProjectAccesses", name: "ProjectId", newName: "Project_ID");
            RenameColumn(table: "dbo.Tasks", name: "FeatureId", newName: "PBI_ID");
            RenameColumn(table: "dbo.Features", name: "ProjectId", newName: "Product_ID");
            CreateIndex("dbo.ProjectAccesses", "Project_ID");
            AddForeignKey("dbo.ProjectAccesses", "Project_ID", "dbo.Projects", "ID");
        }
    }
}
