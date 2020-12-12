using System.Data;
using FluentMigrator;

namespace Group3.Semester3.WebApp.Migrations
{
    [Migration(20201209123550)]
    public class CreateShareFileLinksTable : Migration
    {
        public override void Up()
        {
            Create.Table("SharedFilesLinks")
                .WithColumn("FileId").AsGuid().NotNullable()
                .WithColumn("Hash").AsString().NotNullable();
            Create.ForeignKey()
                .FromTable("SharedFilesLinks").ForeignColumn("FileId")
                .ToTable("Files").PrimaryColumn("Id")
                .OnDeleteOrUpdate(Rule.Cascade);
            ;
        }

        public override void Down()
        {
            Delete.Table("SharedFilesLinks");
        }
    }
}
