namespace VoiceTTS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DefaultKey : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Profiles", "KeyAppendDefault", c => c.String(maxLength: 4000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Profiles", "KeyAppendDefault");
        }
    }
}
