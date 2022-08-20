using Terraria;
using Terraria.ModLoader;

namespace nexperience1dot4
{
    public struct StatusTranslator
    {
        private DamageClass damageclass;
        private byte CountsAs;
        public DamageClass GetDamageClass{get{return damageclass;}}
        public byte GetClassToCountAs{get{return CountsAs; }}
        
        public StatusTranslator(DamageClass dClass, byte CountsAs)
        {
            damageclass = dClass;
            this.CountsAs = CountsAs;
            if(this.CountsAs >= 4)
                this.CountsAs = 255;
        }

        public const byte DC_Melee = 0, DC_Ranged = 1, DC_Magic = 2, DC_Summon = 3, DC_Generic = 255;
    }
}