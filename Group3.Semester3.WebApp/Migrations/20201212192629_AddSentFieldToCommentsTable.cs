using FluentMigrator;

namespace Group3.Semester3.WebApp.Migrations
{
    [Migration(20201212192629)]
    public class AddSentFieldToCommentsTable : Migration
    {
        public override void Up()
        {
            Alter.Table("Comments")
               .AddColumn("Sent").AsDateTime2().Nullable();
        }

        public override void Down()
        {
            Delete
               .Column("Sent")
               .FromTable("Comments");
        }
    }
}
