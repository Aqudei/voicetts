namespace VoiceTTS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeoutputdevice : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Profiles", "OutputDevice");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Profiles", "OutputDevice", c => c.String(maxLength: 4000));
        }
    }
}
