using FluentMigrator;
using System;

namespace Group3.Semester3.WebApp.Migrations
{
    [Migration(20201206110432)]
    public class AlterGroupIdColumnInGroupsTable : Migration
    {
        public override void Up()
        {
            var emptyGuid = Guid.Empty;

            Alter.Table("Files")
                .AlterColumn("GroupId").AsGuid().WithDefaultValue(emptyGuid);
        }

        public override void Down()
        {
            Alter.Table("Files")
                .AlterColumn("GroupId").AsGuid().Nullable();
        }
    }
}
