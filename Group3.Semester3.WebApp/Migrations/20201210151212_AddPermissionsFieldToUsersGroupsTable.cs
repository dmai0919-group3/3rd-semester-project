using FluentMigrator;

namespace Group3.Semester3.DesktopClient.Migrations
{
    [Migration(20201210151212)]
    public class AddPermissionsFieldToUsersGroupsTable : Migration
    {
        public override void Up()
        {
            Alter.Table("UsersGroups")
                .AddColumn("Permissions").AsInt16().WithDefaultValue(1);
        }

        public override void Down()
        {
            Delete
               .Column("Permissions")
               .FromTable("UsersGroups");
        }
    }
}
