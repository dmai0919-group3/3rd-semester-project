using FluentMigrator;

namespace Group3.Semester3.WebApp.Migrations
{
    [Migration(20201211115257)]
    public class AddSizeFieldToFilesTable : Migration
    {
        public override void Up()
        {
            Alter.Table("Files")
                .AddColumn("Size").AsInt64().Nullable();
        }

        public override void Down()
        {
            Delete
               .Column("Size")
               .FromTable("Files");
        }
    }
}
