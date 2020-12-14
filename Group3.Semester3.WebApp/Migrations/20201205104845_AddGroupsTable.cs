using System.Data;
using FluentMigrator;

namespace Group3.Semester3.DesktopClient.Migrations
{
    [Migration(20201205104845)]
    public class AddGroupsTable : Migration
    {
        public override void Up()
        {
            Create.Table("Groups")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("Name").AsString().NotNullable();
            Alter.Table("Files")
                .AddColumn("GroupId").AsGuid().Nullable();
            Create.Table("UsersGroups")
                .WithColumn("UserId").AsGuid().NotNullable()
                .WithColumn("GroupId").AsGuid().NotNullable();
            Create.ForeignKey()
                .FromTable("UsersGroups").ForeignColumn("UserId")
                .ToTable("Users").PrimaryColumn("Id")
                .OnDeleteOrUpdate(Rule.Cascade);
            Create.ForeignKey()
                .FromTable("UsersGroups").ForeignColumn("GroupId")
                .ToTable("Groups").PrimaryColumn("Id")
                .OnDeleteOrUpdate(Rule.Cascade);
            
        }

        public override void Down()
        {
            Delete.Table("Groups");
            Delete.Table("UsersGroups");
            Delete
                .Column("GroupId")
                .FromTable("Files");
        }
    }
}
