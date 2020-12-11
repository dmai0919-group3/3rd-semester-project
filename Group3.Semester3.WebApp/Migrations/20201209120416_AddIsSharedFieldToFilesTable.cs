using FluentMigrator;

namespace Group3.Semester3.WebApp.Migrations
{
    [Migration(20201209120416)]
    public class AddIsSharedFieldToFilesTable : Migration
    {
        public override void Up()
        {
            Alter.Table("Files")
                .AddColumn("IsShared").AsBoolean().WithDefaultValue(false);
        }

        public override void Down()
        {
            Delete
               .Column("IsShared")
               .FromTable("Files");
        }
    }
}
