using DragonLib.Numerics;
using JetBrains.Annotations;

namespace Cethleann.Structure.DataStructs
{
    [PublicAPI]
    public struct CharacterInfo
    {
        public Vector3 Unknown_X0 { get; set; } // Color? Or something?

        public short Unknown_X10 { get; set; } // always -1?

        public short Name { get; set; } // Guess, always matches portrait and model.

        public short TitleProbably { get; set; } // so far zero, but then again I only checked 5 records.

        public short Portrait { get; set; } // Guess, always matches name and model

        public short Model { get; set; } // Changing this changes the model in-game.

        public byte Unknown_X1A { get; set; } // Always zero?

        public byte Age { get; set; }

        public byte Class { get; set; }

        public byte BirthDay { get; set; }

        public byte BirthMonth { get; set; }

        public sbyte Unknown_X1F { get; set; } // Always -1?
        public byte Unknown_X20 { get; set; } // Always 1?
        public byte IsMale { get; set; }
        public byte MaxHP { get; set; }
        public FactionHouse Faction { get; set; } // I'm guessing this is an index in a looooooooooooooooooong text table.
        public byte GrowthHP { get; set; }
        public byte IsFemale { get; set; }
        public byte BaseHP { get; set; }
        public byte Unknown_X28 { get; set; } // Honest to god no clue what this is.
        public byte CrestSlot1 { get; set; }
        public byte CrestSlot2 { get; set; }
        public byte Unknown_X2B { get; set; } // always 20?
        public byte Height { get; set; }
        public byte TimeskipHeight { get; set; }
        public byte Unknown_X2E { get; set; } // always zero?
        public byte Unknown_X2F { get; set; } // always zero?
        public byte BaseSTR { get; set; }
        public byte BaseMAG { get; set; }
        public byte BaseDEX { get; set; }
        public byte BaseSPD { get; set; }
        public byte BaseLCK { get; set; }
        public byte BaseDEF { get; set; }
        public byte BaseRES { get; set; }
        public byte BaseMOV { get; set; }
        public byte BaseCHA { get; set; }
        public byte GrowthSTR { get; set; }
        public byte GrowthMAG { get; set; }
        public byte GrowthDEX { get; set; }
        public byte GrowthSPD { get; set; }
        public byte GrowthLCK { get; set; }
        public byte GrowthDEF { get; set; }
        public byte GrowthRES { get; set; }
        public byte GrowthMOV { get; set; }
        public byte GrowthCHA { get; set; }
        public byte MaxSTR { get; set; }
        public byte MaxMAG { get; set; }
        public byte MaxDEX { get; set; }
        public byte MaxSPD { get; set; }
        public byte MaxLCK { get; set; }
        public byte MaxDEF { get; set; }
        public byte MaxRES { get; set; }
        public byte MaxMOV { get; set; }
        public byte MaxCHA { get; set; }
        public byte Unknown_X4B { get; set; } // always zero? tbh probably padding byte.
    }
}
