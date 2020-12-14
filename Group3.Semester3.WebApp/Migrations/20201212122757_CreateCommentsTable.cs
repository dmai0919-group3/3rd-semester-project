using System.Data;
using FluentMigrator;

namespace Group3.Semester3.WebApp.Migrations
{
    [Migration(20201212122757)]
    public class CreateCommentsTable : Migration
    {
        public override void Up()
        {
            Create.Table("Comments")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("FileId").AsGuid().NotNullable()
                .WithColumn("ParentId").AsGuid().NotNullable()
                .WithColumn("Text").AsString().NotNullable();
            Create.ForeignKey()
                .FromTable("Comments").ForeignColumn("FileId")
                .ToTable("Files").PrimaryColumn("Id")
                .OnDeleteOrUpdate(Rule.Cascade);
        }

        public override void Down()
        {
            Delete.Table("Comments");
        }
    }
}
