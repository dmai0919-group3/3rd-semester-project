﻿using System.Data;
using FluentMigrator;

namespace Group3.Semester3.DesktopClient.Migrations
{
    [Migration(20201208163709)]
    public class CreateSharedFilesTable : Migration
    {
        public override void Up()
        {
            Create.Table("SharedFiles")
                .WithColumn("FileId").AsGuid().NotNullable()
                .WithColumn("UserId").AsGuid().NotNullable();
            
            Create.ForeignKey()
                .FromTable("SharedFiles").ForeignColumn("UserId")
                .ToTable("Users").PrimaryColumn("Id")
                .OnDeleteOrUpdate(Rule.Cascade);
            Create.ForeignKey()
                .FromTable("SharedFiles").ForeignColumn("FileId")
                .ToTable("Files").PrimaryColumn("Id")
                .OnDeleteOrUpdate(Rule.Cascade);
        }

        public override void Down()
        {
            Delete.Table("SharedFiles");
        }
    }
}
