using System;
using FluentMigrator;

namespace Group3.Semester3.WebApp.Migrations
{
    [Migration(20201202173626)]
    public class AddUpdatedFieldToFilesTable : Migration
    {
        public override void Up()
        {
            Alter.Table("Files")
                .AddColumn("Updated").AsDateTime2().Nullable();
        }

        public override void Down()
        {
            Delete
                .Column("Updated")
                .FromTable("Files");
        }
    }
}
