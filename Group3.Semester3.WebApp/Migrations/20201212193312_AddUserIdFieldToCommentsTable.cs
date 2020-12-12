using FluentMigrator;
using System.Data;

namespace Group3.Semester3.WebApp.Migrations
{
    [Migration(20201212193312)]
    public class AddUserIdFieldToCommentsTable : Migration
    {
        public override void Up()
        {
            Alter.Table("Comments")
               .AddColumn("UserId").AsGuid().NotNullable();

            Create.ForeignKey()
                .FromTable("Comments").ForeignColumn("UserId")
                .ToTable("Users").PrimaryColumn("Id")
                .OnDeleteOrUpdate(Rule.Cascade);
        }

        public override void Down()
        {
            Delete
               .Column("UserId")
               .FromTable("Comments");
        }
    }
}
