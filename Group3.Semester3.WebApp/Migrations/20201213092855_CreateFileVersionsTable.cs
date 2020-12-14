using System.Data;
using FluentMigrator;

namespace Group3.Semester3.WebApp.Migrations
{
    [Migration(20201213092855)]
    public class CreateFileVersionsTable : Migration
    {
        public override void Up()
        {
            Create.Table("FileVersions")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("FileId").AsGuid().NotNullable()
                .WithColumn("AzureName").AsString().NotNullable()
                .WithColumn("Note").AsString().NotNullable()
                .WithColumn("Created").AsDateTime2().NotNullable();
            
            Create.ForeignKey()
                .FromTable("FileVersions").ForeignColumn("FileId")
                .ToTable("Files").PrimaryColumn("Id")
                .OnDeleteOrUpdate(Rule.Cascade);
        }

        public override void Down()
        {
            Delete.Table("FileVersions");
        }
    }
}
