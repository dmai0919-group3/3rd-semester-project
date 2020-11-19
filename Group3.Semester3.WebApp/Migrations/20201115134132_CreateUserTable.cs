using FluentMigrator;

namespace Group3.Semester3.WebApp.Migrations
{
    [Migration(20201115134132)]
    public class CreateUserTable : Migration
    {
        public override void Up()
        {
            Create.Table("Users")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("Email").AsString().NotNullable().Unique()
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("PasswordHash").AsString().NotNullable()
                .WithColumn("PasswordSalt").AsString().NotNullable();
        }

        public override void Down()
        {
            Delete.Table("Users");
        }
    }
}
