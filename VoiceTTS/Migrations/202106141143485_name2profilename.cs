namespace VoiceTTS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class name2profilename : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Profiles", "ProfileName", c => c.String(maxLength: 4000));
            DropColumn("dbo.Profiles", "Name");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Profiles", "Name", c => c.String(maxLength: 4000));
            DropColumn("dbo.Profiles", "ProfileName");
        }
    }
}
