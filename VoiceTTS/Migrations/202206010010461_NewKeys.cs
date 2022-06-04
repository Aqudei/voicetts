namespace VoiceTTS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewKeys : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Profiles", "KeyAppendBreathing", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyAppendSoft", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyAppendWhispered", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyAppendConversational", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyAppendNews", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyAppendCustomerSupport", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyAppendAssistant", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyAppendHappy", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyAppendEmphatic", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyAppendClam", c => c.String(maxLength: 4000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Profiles", "KeyAppendClam");
            DropColumn("dbo.Profiles", "KeyAppendEmphatic");
            DropColumn("dbo.Profiles", "KeyAppendHappy");
            DropColumn("dbo.Profiles", "KeyAppendAssistant");
            DropColumn("dbo.Profiles", "KeyAppendCustomerSupport");
            DropColumn("dbo.Profiles", "KeyAppendNews");
            DropColumn("dbo.Profiles", "KeyAppendConversational");
            DropColumn("dbo.Profiles", "KeyAppendWhispered");
            DropColumn("dbo.Profiles", "KeyAppendSoft");
            DropColumn("dbo.Profiles", "KeyAppendBreathing");
        }
    }
}
