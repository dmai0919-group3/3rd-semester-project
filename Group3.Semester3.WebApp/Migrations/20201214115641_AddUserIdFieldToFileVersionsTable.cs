using FluentMigrator;
using System.Data;

namespace Group3.Semester3.WebApp.Migrations
{
    [Migration(20201214115641)]
    public class AddUserIdFieldToFileVersionsTable : Migration
    {
        public override void Up()
        {
            Alter.Table("FileVersions")
               .AddColumn("UserId").AsGuid().Nullable();

            Create.ForeignKey()
                .FromTable("FileVersions").ForeignColumn("UserId")
                .ToTable("Users").PrimaryColumn("Id")
                .OnDeleteOrUpdate(Rule.Cascade);
        }

        public override void Down()
        {
            Delete
               .Column("UserId")
               .FromTable("FileVersions");
        }
    }
}
