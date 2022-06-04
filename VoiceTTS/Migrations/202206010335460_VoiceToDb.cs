namespace VoiceTTS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VoiceToDb : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.VoiceInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Engine = c.String(maxLength: 4000),
                        VoiceId = c.String(maxLength: 4000),
                        VoiceGender = c.String(maxLength: 4000),
                        VoiceWebname = c.String(maxLength: 4000),
                        Country = c.String(maxLength: 4000),
                        Language = c.String(maxLength: 4000),
                        LanguageName = c.String(maxLength: 4000),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.VoiceInfoes");
        }
    }
}
