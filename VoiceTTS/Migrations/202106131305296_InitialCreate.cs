namespace VoiceTTS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Profiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Engine = c.String(maxLength: 4000),
                        LanguageCode = c.String(maxLength: 4000),
                        VoiceId = c.String(maxLength: 4000),
                        Effect = c.String(maxLength: 4000),
                        MasterSpeed = c.Int(nullable: false),
                        MasterVolume = c.Int(nullable: false),
                        MasterPitch = c.Int(nullable: false),
                        Name = c.String(maxLength: 4000),
                        AccentCode = c.String(maxLength: 4000),
                        DefaultEffect = c.String(maxLength: 4000),
                        DefaultPause = c.String(maxLength: 4000),
                        DefaultEmphasis = c.String(maxLength: 4000),
                        DefaultSpeed = c.String(maxLength: 4000),
                        DefaultPitch = c.String(maxLength: 4000),
                        DefaultVolume = c.String(maxLength: 4000),
                        DefaultSayAs = c.String(maxLength: 4000),
                        OutputDevice = c.String(maxLength: 4000),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Profiles");
        }
    }
}
