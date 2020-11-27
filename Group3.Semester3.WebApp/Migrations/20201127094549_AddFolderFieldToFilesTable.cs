using FluentMigrator;
using System;

namespace Group3.Semester3.WebApp.Migrations
{
    [Migration(20201127094549)]
    public class AddFolderFieldToFilesTable : Migration
    {
        public override void Up()
        {
            var emptyGuid = Guid.Empty;

            Alter.Table("Files")
                .AddColumn("ParentId").AsGuid().WithDefaultValue(emptyGuid)
                .AddColumn("IsFolder").AsBoolean().WithDefaultValue(false);
        }

        public override void Down()
        {
            Delete
                .Column("ParentId")
                .Column("IsFolder")
                .FromTable("Files");
        }
    }
}
