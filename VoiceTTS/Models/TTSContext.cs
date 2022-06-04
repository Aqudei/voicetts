using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceTTS.Models
{
    public class TTSContextInitializer : DropCreateDatabaseIfModelChanges<TTSContext>
    {
        protected override void Seed(TTSContext context)
        {
            IList<Profile> defaultStandards = new List<Profile>();

            for (int i = 0; i < 10; i++)
            {
                defaultStandards.Add(new Profile()
                {
                    ProfileName = $"Profile {i}",
                    // LanguageCode = "en-US",
                    VoiceId = "ai1-Ivy",
                    MasterSpeed = 0,
                    Engine = "neural",
                    MasterVolume = 0,
                    MasterPitch = 0,
                    Effect = "default",
                    AccentCode = "en-US"
                });
            }

            context.Profiles.AddRange(defaultStandards);

            base.Seed(context);
        }
    }
    public class TTSContext : DbContext
    {
        public DbSet<Profile> Profiles { get; set; }

        public DbSet<VoiceInfo> Voices { get; set; }

        public TTSContext() : base("CompactDBContext")
        {
            Database.SetInitializer(new TTSContextInitializer());
        }
    }
}
