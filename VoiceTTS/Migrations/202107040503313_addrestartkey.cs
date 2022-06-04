namespace VoiceTTS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addrestartkey : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Profiles", "KeyAppRestart", c => c.String(maxLength: 4000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Profiles", "KeyAppRestart");
        }
    }
}
