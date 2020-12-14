using FluentMigrator;

namespace Group3.Semester3.WebApp.Migrations
{
    [Migration(20201206140146)]
    public class AddActivatedColumnToUserTable : Migration
    {
        public override void Up()
        {
            Alter.Table("Users")
                .AddColumn("Activated").AsBoolean().WithDefaultValue(false);
        }

        public override void Down()
        {
            Delete
                .Column("Activated")
                .FromTable("Users");
        }
    }
}
