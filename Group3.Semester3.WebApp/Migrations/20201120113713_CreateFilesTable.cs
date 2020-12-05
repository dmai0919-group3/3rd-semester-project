using FluentMigrator;

namespace Group3.Semester3.WebApp.Migrations
{
    [Migration(20201120113713)]
    public class CreateFilesTable : Migration
    {
        public override void Up()
        {
            Create.Table("Files")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("UserId").AsGuid().NotNullable()
                .WithColumn("AzureId").AsString().NotNullable()
                .WithColumn("Name").AsString().NotNullable();
            Create.ForeignKey() 
                .FromTable("Files").ForeignColumn("UserId")
                .ToTable("Users").PrimaryColumn("Id");
        }

        public override void Down()
        {
            Delete.Table("Files");
        }
    }
}
