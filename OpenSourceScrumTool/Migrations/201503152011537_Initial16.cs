namespace OpenSourceScrumTool.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial16 : DbMigration
    {
        public override void Up()
        {
            //This if for changeset 127
            //We need to change the EnumTaskState values here as I've changed them to be 0 based
            //Here is an SQL Script to fix the problem, it should only be ran once. #risky...

            const string updateSql = @"
            UPDATE [dbo].[Tasks]
            SET [State] = [State] - 1
            GO";
            Sql(updateSql);
        }

        public override void Down()
        {
            const string updateSql = @"
            UPDATE [dbo].[Tasks]
            SET [State] = [State] + 1
            GO";
            Sql(updateSql);
        }
    }
}
