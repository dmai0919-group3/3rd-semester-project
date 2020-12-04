using FluentMigrator;

namespace Group3.Semester3.WebApp.Migrations
{
    [Migration(20201127100723)]
    public class AlterAzureIdColumnInFilesTable : Migration
    {
        public override void Up()
        {
            Alter.Table("Files")
                .AlterColumn("AzureId").AsString().Nullable();
        }

        public override void Down()
        {
            Alter.Table("Files")
                .AlterColumn("AzureId").AsString().NotNullable();
        }
    }
}
