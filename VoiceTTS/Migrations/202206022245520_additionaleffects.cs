namespace VoiceTTS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class additionaleffects : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Profiles", "KeyAppendSad", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyAppendAngry", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyAppendExcited", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyAppendFriendly", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyAppendHopeful", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyAppendShouting", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyAppendTerrified", c => c.String(maxLength: 4000));
            AddColumn("dbo.Profiles", "KeyAppendUnFriendly", c => c.String(maxLength: 4000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Profiles", "KeyAppendUnFriendly");
            DropColumn("dbo.Profiles", "KeyAppendTerrified");
            DropColumn("dbo.Profiles", "KeyAppendShouting");
            DropColumn("dbo.Profiles", "KeyAppendHopeful");
            DropColumn("dbo.Profiles", "KeyAppendFriendly");
            DropColumn("dbo.Profiles", "KeyAppendExcited");
            DropColumn("dbo.Profiles", "KeyAppendAngry");
            DropColumn("dbo.Profiles", "KeyAppendSad");
        }
    }
}
