namespace VoiceTTS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class withhotkeys : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Profiles", "KeyManualSend", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyVolumeUp", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyVolumeDown", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyPitchDown", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyPitchUp", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeySpeedUp", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeySpeedDown", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyAppendVoice", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyAppendPause", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyAppendEmphasis", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyAppendSpeed", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyAppendPitch", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyAppendVolume", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyAppendSayAs", c => c.String(maxLength: 4000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Profiles", "KeyAppendSayAs");
            DropColumn("dbo.Profiles", "KeyAppendVolume");
            DropColumn("dbo.Profiles", "KeyAppendPitch");
            DropColumn("dbo.Profiles", "KeyAppendSpeed");
            DropColumn("dbo.Profiles", "KeyAppendEmphasis");
            DropColumn("dbo.Profiles", "KeyAppendPause");
            DropColumn("dbo.Profiles", "KeyAppendVoice");
            DropColumn("dbo.Profiles", "KeySpeedDown");
            DropColumn("dbo.Profiles", "KeySpeedUp");
            DropColumn("dbo.Profiles", "KeyPitchUp");
            DropColumn("dbo.Profiles", "KeyPitchDown");
            DropColumn("dbo.Profiles", "KeyVolumeDown");
            DropColumn("dbo.Profiles", "KeyVolumeUp");
            DropColumn("dbo.Profiles", "KeyManualSend");
        }
    }
}
