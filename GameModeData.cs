using Terraria;
using Terraria.ModLoader;

namespace nexperience1dot4
{
    public class GameModeData
    {
        public GameModeBase GetBase { get { if (_base == null) { _base = nexperience1dot4.GetGameMode(GetGameModeID); } return _base; } }
        private GameModeBase _base;

        public string GetGameModeID { get { return GameModeID; } }
        private string GameModeID = "";
        public int GetLevel { get { return _Level; } private set{_Level = value;} }
        private int _Level = 1;
        public int GetExp { get { return _Exp; } }
        private int _Exp = 0;
        public int GetMaxExp { get { return _MaxExp; } }
        private int _MaxExp = 0;
        public int GetStatusPoints { get { return _StatusPoints; } }
        private int _StatusPoints = 0;
        public int GetEffectiveLevel { get { return _EffectiveLevel; } }
        private int _EffectiveLevel = 1;
        private PlayerStatusInfo[] MyStatus = new PlayerStatusInfo[0];
        private int LastMaxLife = 0, LastMaxMana = 0;
        private BiomeLevelStruct LastCheckedBiome = null;
        public BiomeLevelStruct GetMyBiome { get { return LastCheckedBiome; } }
        public float HealthPercentageChange = 1f, ManaPercentageChange = 1f;
        public float GenericDamagePercentage = 1, MeleeDamagePercentage = 1, RangedDamagePercentage = 1, MagicDamagePercentage = 1, SummonDamagePercentage = 1;
        public float MeleeCriticalPercentage = 1f, RangedCriticalPercentage = 1f, MagicCriticalPercentage = 1f;
        public float MeleeSpeedPercentage = 1f;
        public float ProjectileNpcDamagePercentage{get{ return GenericDamagePercentage;} set{ GenericDamagePercentage = value; }}
        public bool NpcIsFirstFrame{get{return MeleeDamagePercentage != 0; } set{ MeleeDamagePercentage = value ? 1 : 0; }}
        public float NpcHealthSum{ get{ return MeleeCriticalPercentage; } set{ MeleeCriticalPercentage = value; }}
        public float NpcHealthMult{ get{ return RangedCriticalPercentage; } set{ RangedCriticalPercentage = value; }}
        public float NpcDamageSum{ get{ return SummonDamagePercentage; } set{ SummonDamagePercentage = value; }}
        public float NpcDamageMult{ get{ return RangedDamagePercentage; } set{ RangedDamagePercentage = value; }}
        public float NpcDefense{ get{ return MagicDamagePercentage; } set{ MagicDamagePercentage = value; }}
        public int LastNpcDamage{ get{ return LastMaxLife; } set{ LastMaxLife = value; }}
        public int LastNpcDefense{ get{ return LastMaxMana; } set{ LastMaxMana = value; }}
        float _LevelExpValue = 0;
        public float GetLevelExpValue => _LevelExpValue;

        private byte BiomeUpdateDelay = 0;
        private const byte MaxBiomeUpdateDelay = 8;

        public void SetLevel(int NewLevel){
            GetLevel = NewLevel;
        }

        public void ResetEffectiveLevel()
        {
            _EffectiveLevel = 0;
        }

        public GameModeData(string GameModeID, bool PlayerStatus)
        {
            this.GameModeID = GameModeID;
            if(PlayerStatus)
            {
                MyStatus = new PlayerStatusInfo[GetBase.GameModeStatus.Length];
                for(int i = 0; i < MyStatus.Length; i++)
                {
                    MyStatus[i] = new PlayerStatusInfo();
                }
                UpdateMaxExp();
            }
        }

        public GameModeData(byte GameModeID, Terraria.ModLoader.IO.TagCompound tag, int ModVersion)
        {
            LoadGameMode(GameModeID, tag, ModVersion);
        }

        public void ChangeGameMode(string NewGameMode){
            this.GameModeID = NewGameMode;
        }

        public void CheckMyBiome(Player player)
        {
            LastCheckedBiome = GetBase.GetBiomeActiveForPlayer(player);
        }

        public bool ChangeExp(int Value)
        {
            try
            {
                checked
                {
                    _Exp += Value;
                }
            }
            catch
            {
                if (Value < 0)
                    return false;
                if (Value > 0)
                    _Exp = int.MaxValue;
            }
            if (_Exp < 0) return false;
            bool LeveledUp = false;
            int LastLevelValue = (int)(_Level * GetBase.LevelChangeFactor);
            while (_Exp >= GetMaxExp && (nexperience1dot4.InfiniteLeveling || GetLevel < GetBase.GetMaxLevel))
            {
                _Exp -= _MaxExp;
                _Level++;
                UpdateMaxExp();
                UpdateStatusPoints();
                //LeveledUp = true;
            }
            LeveledUp = (int)(_Level * GetBase.LevelChangeFactor) != LastLevelValue;
            RecalcLevelExpValue();
            return LeveledUp;
        }

        void RecalcLevelExpValue()
        {
            float LevelFactor = GetBase.LevelChangeFactor;
            _LevelExpValue = ((float)_Exp / _MaxExp) * LevelFactor;
            if (LevelFactor < 1)
            {
                int LevelsPerLevel = (int)(1f / LevelFactor);
                _LevelExpValue += LevelFactor * (_Level % LevelsPerLevel); 
            }
        }

        public int DoDeathExpPenalty(){
            int ExpDeduction = (int)(GetMaxExp * (nexperience1dot4.DeathExpPenalty * 0.01));
            if(ExpDeduction > GetExp)
                ExpDeduction = GetExp;
            ChangeExp(-ExpDeduction);
            return -ExpDeduction;
        }
        private void UpdateMaxExp()
        {
            _MaxExp = GetBase.GetLevelExp(_Level);
        }

        public void UpdateStatusPoints(){
            int MyStatusPoints = (int)(GetBase.InitialStatusPoints + GetBase.StatusPointsPerLevel * GetLevel);
            foreach(PlayerStatusInfo si in MyStatus)
            {
                MyStatusPoints -= si.StatusValue;
            }
            _StatusPoints = MyStatusPoints;
            if(_StatusPoints < 0)
            {
                ResetStatusPoints();
            }
        }

        public void UpdatePlayer(Player player)
        {
            UpdateBiomeCheck(player);
            UpdateEffectiveLevel();
            MeleeDamagePercentage = RangedDamagePercentage = MagicDamagePercentage = SummonDamagePercentage = GenericDamagePercentage = 
                MeleeCriticalPercentage = RangedCriticalPercentage = MagicCriticalPercentage = MeleeSpeedPercentage = 1;
            GetBase.UpdatePlayerStatus(player, this);
            foreach(StatusTranslator st in nexperience1dot4.GetStatusLists())
            {
                switch(st.GetClassToCountAs)
                {
                    case StatusTranslator.DC_Generic:
                        player.GetDamage(st.GetDamageClass) *= GenericDamagePercentage;
                        break;
                    case StatusTranslator.DC_Melee:
                        player.GetDamage(st.GetDamageClass) *= MeleeDamagePercentage;
                        player.GetCritChance(st.GetDamageClass) *= MeleeCriticalPercentage;
                        player.GetAttackSpeed(st.GetDamageClass) *= MeleeSpeedPercentage;
                        break;
                    case StatusTranslator.DC_Ranged:
                        player.GetDamage(st.GetDamageClass) *= RangedDamagePercentage;
                        player.GetCritChance(st.GetDamageClass) *= RangedCriticalPercentage;
                        break;
                    case StatusTranslator.DC_Magic:
                        player.GetDamage(st.GetDamageClass) *= MagicDamagePercentage;
                        player.GetCritChance(st.GetDamageClass) *= MagicCriticalPercentage;
                        break;
                    case StatusTranslator.DC_Summon:
                        player.GetDamage(st.GetDamageClass) *= SummonDamagePercentage;
                        break;
                }
            }

            HealthPercentageChange = (float)player.statLifeMax2 / player.statLifeMax;
            ManaPercentageChange = (float)player.statManaMax2 / player.statManaMax;
            if (LastMaxLife > 0 && player.statLifeMax2 != LastMaxLife)
            {
                bool LastWasDead = player.statLife <= 0;
                double HpPercentage = player.statLife <= 0 ? 0f : (player.statLife >= LastMaxLife ? 1f : (double)player.statLife / LastMaxLife);
                player.statLife = (int)(player.statLifeMax2 * HpPercentage);
                if(!LastWasDead && player.statLife <= 0)
                    player.statLife = 1;
            }
            if (LastMaxMana > 0 && player.statManaMax2 != LastMaxMana)
            {
                double MpPercentage = player.statMana <= 0 ? 0f : (player.statMana >= LastMaxMana ? 1f : (double)player.statMana / LastMaxMana);
                player.statMana = (int)(player.statManaMax2 * MpPercentage);
            }
            LastMaxLife = player.statLifeMax2;
            LastMaxMana = player.statManaMax2;
        }

        public void NormalizePointsInvested()
        {
            for(int s = 0; s < MyStatus.Length; s++)
            {
                _StatusPoints += MyStatus[s].StatusValue;
                MyStatus[s].StatusValue = 0;
            }
            int NormalizedValues = _StatusPoints == 0 || MyStatus.Length == 0 ? 0 : _StatusPoints / MyStatus.Length;
            for(int s = 0; s < MyStatus.Length; s++)
            {
                MyStatus[s].StatusValue = NormalizedValues;
                _StatusPoints -= NormalizedValues;
            }
            UpdateEffectiveStatus();
        }

        private void UpdateEffectiveLevel()
        {
            int LastLevel = _EffectiveLevel;
            _EffectiveLevel = _Level;
            if ((!nexperience1dot4.InfiniteLeveling || nexperience1dot4.CapLevelOnInfiniteLeveling) && _EffectiveLevel > GetBase.GetMaxLevel)
                _EffectiveLevel = GetBase.GetMaxLevel;
            if (nexperience1dot4.EnableBiomeLevelCapper && GetBase.EnableLevelCapping)
            {
                if (LastCheckedBiome != null && _EffectiveLevel > LastCheckedBiome.GetMaxLevel)
                    _EffectiveLevel = LastCheckedBiome.GetMaxLevel;
                for (int n = 0; n < 200; n++)
                {
                    if (Main.npc[n].active && Main.npc[n].chaseable && !Main.npc[n].townNPC)
                    {
                        int NpcLevel = NpcMod.GetNpcLevel(Main.npc[n]);
                        if (NpcLevel > _EffectiveLevel)
                        {
                            _EffectiveLevel = NpcLevel;
                            if (_EffectiveLevel > _Level)
                                _EffectiveLevel = _Level;
                        }
                    }
                }
            }
            if(_EffectiveLevel != LastLevel)
            {
                UpdateEffectiveStatus();
            }
        }

        public void UpdateEffectiveStatus()
        {
            //GetBase.InitialStatusPointsDistribution * MyStatus.Length +
            int EffectiveLevelStatus = (int)(GetBase.InitialStatusPoints + GetBase.StatusPointsPerLevel * GetEffectiveLevel),
                LevelStatus = (int)(GetBase.InitialStatusPoints + GetBase.StatusPointsPerLevel * GetLevel);
            int TotalStatusCount = 0;
            foreach(PlayerStatusInfo si in MyStatus)
            {
                TotalStatusCount += si.StatusValue;
                si.EffectiveStatusValue = si.StatusValue;
            }
            if(TotalStatusCount == 0 || TotalStatusCount <= EffectiveLevelStatus){
                return;
            }
            double Percentage = (double)EffectiveLevelStatus / LevelStatus;
            int ResultingStatusSum = 0;
            PlayerStatusInfo HighestIncreasedStatus = null;
            foreach(PlayerStatusInfo si in MyStatus)
            {
                if(si.StatusValue > 0 && (HighestIncreasedStatus == null || si.StatusValue > HighestIncreasedStatus.StatusValue)) HighestIncreasedStatus = si;
                int StatusChange = (int)(si.StatusValue * Percentage);
                ResultingStatusSum += StatusChange;
                si.EffectiveStatusValue = StatusChange;
            }
            if(HighestIncreasedStatus != null && ResultingStatusSum != EffectiveLevelStatus){
                HighestIncreasedStatus.EffectiveStatusValue += EffectiveLevelStatus - ResultingStatusSum;
            }
        }

        private void UpdateBiomeCheck(Player player)
        {
            if (BiomeUpdateDelay == 0)
            {
                CheckMyBiome(player);
                BiomeUpdateDelay = MaxBiomeUpdateDelay;
            }
            else
            {
                BiomeUpdateDelay--;
            }
        }

        public void UpdateNPC(NPC npc)
        {
            _EffectiveLevel = _Level;
            bool UpdateStats = TownNpcEffectiveLevelTweak(npc);
            //int npcHealthBackup = npc.lifeMax;
            //npc.lifeMax = npc.GetGlobalNPC<NpcMod>().GetOriginalHP;
            //GetBase.UpdateNpcStatus(npc, this);
            NpcMod.CheckNpcStatsChanged(npc, out bool HealthChanged, out bool DamageChanged, out bool DefenseChanged);
            if(UpdateStats || DamageChanged)
            {
                try
                {
                    checked
                    {
                        npc.damage = (int)((npc.damage + NpcDamageSum) * NpcDamageMult);
                    }
                }
                catch
                {
                    npc.damage = int.MaxValue;
                }
                LastNpcDamage = npc.damage;
            }
            if(UpdateStats || DefenseChanged)
            {
                try
                {
                    checked
                    {
                        npc.defense = (int)(npc.defense * NpcDefense);
                    }
                }
                catch
                {
                    npc.defense = int.MaxValue;
                }
                /*if (nexperience1dot4.MobDefenseToHealth && npc.defDefense < 1000)
                {
                    int Dif = npc.defense - npc.defDefense;
                    npc.lifeMax += Dif * GetBase.DefenseToHealthConversionRate;
                    npc.defense -= Dif;
                }*/
                LastNpcDefense = npc.defense;
            }
            /*if(npc.lifeMax != npcHealthBackup)
            {
                double Percentage = npc.life >= npcHealthBackup ? 1 : (double)npc.life / npcHealthBackup;
                npc.life = (int)(npc.lifeMax * Percentage);
            }*/
        }

        public bool TownNpcEffectiveLevelTweak(NPC npc, bool TransformIfDifferentLevel = true)
        {
            if (Main.gameMenu || !npc.townNPC) return false;
            int LastLevel = _EffectiveLevel;
            _EffectiveLevel = _Level;
            for (int p = 0; p < 255; p++)
            {
                if (!Main.player[p].active) continue;
                int PlayerLevel = Main.player[p].GetModPlayer<PlayerMod>().GetCurrentGamemode.GetEffectiveLevel;
                if (PlayerLevel > _EffectiveLevel)
                    _EffectiveLevel = PlayerLevel;
            }
            for (int n = 0; n < 200; n++)
            {
                if (Main.npc[n].active && !Main.npc[n].townNPC && Main.npc[n].CanBeChasedBy(null))
                {
                    NpcMod npcmod = Main.npc[n].GetGlobalNPC<NpcMod>();
                    if(_EffectiveLevel < npcmod.GetData.GetEffectiveLevel)
                    {
                        _EffectiveLevel = npcmod.GetData.GetEffectiveLevel;
                    }
                }
            }
            if (LastLevel != _EffectiveLevel)
            {
                npc.damage = npc.defDamage;
                npc.defense = npc.defDefense;
                GetBase.UpdateNpcStatus(npc, this);
            }
            return LastLevel != _EffectiveLevel;
        }

        public void SpawnNpcLevel(NPC npc)
        {
            int NewLevel = GetBase.GetMobLevel(npc);
            NpcIsFirstFrame = true;
            LastNpcDamage = 0;
            LastNpcDefense = 0;
            if (nexperience1dot4.SetEverythingToMyLevel || CanScaleToPlayerLevel(npc))
            {
                Player Closest = null;
                float NearestDistance = float.MaxValue;
                for(int i = 0; i < 255; i++)
                {
                    if (Main.player[i].active)
                    {
                        float Distance = (Main.player[i].Center - npc.Center).Length();
                        if (Distance < NearestDistance)
                        {
                            Closest = Main.player[i];
                            NearestDistance = Distance;
                        }
                    }
                }
                if (Closest != null)
                {
                    NewLevel = PlayerMod.GetPlayerLevel(Closest);
                }
            }
            if (NewLevel == 0)
            {
                if (npc.realLife > -1)
                {
                    NewLevel = NpcMod.GetNpcLevel(Main.npc[npc.realLife]);
                }
                else if (NpcMod.GetOriginNpc != null)
                {
                    if(NpcMod.GetOriginNpc == npc)
                        NewLevel = NpcMod.LastLoggedMonsterLevel;
                    else
                        NewLevel = NpcMod.GetNpcLevel(NpcMod.GetOriginNpc);
                }
                else
                {
                    if(NewLevel == 0) NewLevel = GetBase.GetNpcLevelProcedural(npc);
                    if (NewLevel == 0)
                    {
                        NPCSpawnInfo? nsi = NpcMod.GetLastSpawnInfo;
                        if (nsi.HasValue)
                        {
                            BiomeLevelStruct bls = PlayerMod.GetPlayerBiomeLevel(nsi.Value.Player);
                            if (bls != null) NewLevel = bls.GetRandomLevel;
                        }
                        else
                        {
                            NewLevel = 1;
                        }
                    }
                }
            }
            _Level = NewLevel;
            _EffectiveLevel = NewLevel;
            TownNpcEffectiveLevelTweak(npc, false);
            NpcHealthSum = 0;
            NpcDamageSum = 0;
            int LastMaxHealth = npc.lifeMax;
            GetBase.UpdateNpcStatus(npc, this);
            if(npc.type == Terraria.ID.NPCID.Nailhead)
                ProjectileNpcDamagePercentage = 1;
        }

        public void UpdateMobHealthChangePercentage(NPC npc, int LastMaxHealth)
        {
            HealthPercentageChange = (float)LastMaxHealth / npc.lifeMax;;
        }

        private bool CanScaleToPlayerLevel(NPC npc)
        {
            if (nexperience1dot4.Playthrough1dot5 && NPC.downedMoonlord)
            {
                return true;
            }
            else
            {
                switch (npc.type)
                {
                    case Terraria.ID.NPCID.EyeofCthulhu:
                    case Terraria.ID.NPCID.ServantofCthulhu:
                        return nexperience1dot4.BossesAsToughAsMe && NPC.downedBoss1;
                    case Terraria.ID.NPCID.EaterofWorldsBody:
                    case Terraria.ID.NPCID.EaterofWorldsHead:
                    case Terraria.ID.NPCID.EaterofWorldsTail:
                    case Terraria.ID.NPCID.BrainofCthulhu:
                    case Terraria.ID.NPCID.Creeper:
                        return nexperience1dot4.BossesAsToughAsMe && NPC.downedBoss2;
                    case Terraria.ID.NPCID.SkeletronHead:
                    case Terraria.ID.NPCID.SkeletronHand:
                        return nexperience1dot4.BossesAsToughAsMe && NPC.downedBoss3;
                    case Terraria.ID.NPCID.KingSlime:
                    case Terraria.ID.NPCID.SlimeSpiked:
                        return nexperience1dot4.BossesAsToughAsMe && NPC.downedSlimeKing;
                    case Terraria.ID.NPCID.QueenBee:
                        return nexperience1dot4.BossesAsToughAsMe && NPC.downedQueenBee;
                    case Terraria.ID.NPCID.WallofFlesh:
                    case Terraria.ID.NPCID.WallofFleshEye:
                    case Terraria.ID.NPCID.TheHungry:
                    case Terraria.ID.NPCID.TheHungryII:
                    case Terraria.ID.NPCID.LeechBody:
                    case Terraria.ID.NPCID.LeechHead:
                    case Terraria.ID.NPCID.LeechTail:
                        return nexperience1dot4.BossesAsToughAsMe && Main.hardMode;
                    case Terraria.ID.NPCID.TheDestroyer:
                    case Terraria.ID.NPCID.TheDestroyerBody:
                    case Terraria.ID.NPCID.TheDestroyerTail:
                        return nexperience1dot4.BossesAsToughAsMe && NPC.downedMechBoss1;
                    case Terraria.ID.NPCID.Spazmatism:
                    case Terraria.ID.NPCID.Retinazer:
                        return nexperience1dot4.BossesAsToughAsMe && NPC.downedMechBoss2;
                    case Terraria.ID.NPCID.SkeletronPrime:
                    case Terraria.ID.NPCID.PrimeCannon:
                    case Terraria.ID.NPCID.PrimeLaser:
                    case Terraria.ID.NPCID.PrimeSaw:
                    case Terraria.ID.NPCID.PrimeVice:
                        return nexperience1dot4.BossesAsToughAsMe && NPC.downedMechBoss3;
                    case Terraria.ID.NPCID.Plantera:
                    case Terraria.ID.NPCID.PlanterasHook:
                    case Terraria.ID.NPCID.PlanterasTentacle:
                        return nexperience1dot4.BossesAsToughAsMe && NPC.downedPlantBoss;
                    case Terraria.ID.NPCID.Golem:
                    case Terraria.ID.NPCID.GolemFistLeft:
                    case Terraria.ID.NPCID.GolemFistRight:
                    case Terraria.ID.NPCID.GolemHead:
                    case Terraria.ID.NPCID.GolemHeadFree:
                        return nexperience1dot4.BossesAsToughAsMe && NPC.downedGolemBoss;
                    case Terraria.ID.NPCID.DukeFishron:
                    case Terraria.ID.NPCID.Sharkron:
                    case Terraria.ID.NPCID.Sharkron2:
                        return nexperience1dot4.BossesAsToughAsMe && NPC.downedFishron;
                    case Terraria.ID.NPCID.AncientCultistSquidhead:
                    case Terraria.ID.NPCID.CultistBoss:
                    case Terraria.ID.NPCID.CultistBossClone:
                    case Terraria.ID.NPCID.CultistDragonBody1:
                    case Terraria.ID.NPCID.CultistDragonBody2:
                    case Terraria.ID.NPCID.CultistDragonBody3:
                    case Terraria.ID.NPCID.CultistDragonBody4:
                    case Terraria.ID.NPCID.CultistDragonHead:
                    case Terraria.ID.NPCID.CultistDragonTail:
                        return nexperience1dot4.BossesAsToughAsMe && NPC.downedAncientCultist;
                    case 379: //Cultist Archer
                        if (npc.type == 379)
                        {
                            if (npc.ai[3] < 0)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            return true;
                        }
                        break;
                    case 380:
                    //case 437:
                    case 438:
                        {
                            if (npc.type == 438 || npc.type == 379)
                            {
                                if (npc.ai[1] == 1)
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                return true;
                            }
                        }
                        break;
                    case Terraria.ID.NPCID.MoonLordCore:
                    case Terraria.ID.NPCID.MoonLordFreeEye:
                    case Terraria.ID.NPCID.MoonLordHand:
                    case Terraria.ID.NPCID.MoonLordHead:
                    case Terraria.ID.NPCID.MoonLordLeechBlob:
                    return nexperience1dot4.BossesAsToughAsMe && NPC.downedMoonlord;
                }
            }
            return false;
        }

        public int GetStatusValue(byte StatusIndex, out int EffectiveStatus){
            EffectiveStatus = 0;
            if(StatusIndex >= MyStatus.Length) return 0;
            EffectiveStatus = MyStatus[StatusIndex].EffectiveStatusValue;
            return MyStatus[StatusIndex].StatusValue;
        }

        public int GetStatusValue(byte StatusIndex){
            if(StatusIndex >= MyStatus.Length) return 0;
            return MyStatus[StatusIndex].StatusValue;
        }

        public int GetEffectiveStatusValue(byte StatusIndex){
            if(StatusIndex >= MyStatus.Length) return 0;
            return MyStatus[StatusIndex].EffectiveStatusValue;
        }

        public void ResetStatusPoints()
        {
            for(int s = 0; s < MyStatus.Length; s++)
            {
                _StatusPoints += MyStatus[s].StatusValue;
                MyStatus[s].StatusValue = 0;
            }   
            UpdateStatusPoints();
            UpdateEffectiveStatus();
        }

        public void AddStatusPoint(byte StatusIndex, int Count)
        {
            if(StatusIndex >= MyStatus.Length) return;
            if(Count > _StatusPoints)
                Count = _StatusPoints;
            MyStatus[StatusIndex].StatusValue += Count;
            _StatusPoints -= Count;
        }

        internal void ChangeStatusValue(byte StatusIndex, int Count)
        {
            if(StatusIndex >= MyStatus.Length) return;
            MyStatus[StatusIndex].StatusValue = Count;
        }

        public void SaveGameMode(byte ID, Terraria.ModLoader.IO.TagCompound tag)
        {
            Terraria.ModLoader.IO.TagCompound newtag = new Terraria.ModLoader.IO.TagCompound();
            newtag.Add("gamemodeid", GameModeID);
            newtag.Add("level", _Level);
            newtag.Add("exp", _Exp);
            newtag.Add("statuspoints", _StatusPoints);
            newtag.Add("statuscount", MyStatus.Length);
            for(byte i = 0; i < MyStatus.Length; i++)
            {
                newtag.Add("status" + i, MyStatus[i].StatusValue);
            }
            tag.Add("gamemodeinfo" + ID, newtag);
        }

        public void LoadGameMode(byte ID, Terraria.ModLoader.IO.TagCompound tag, int ModVersion)
        {
            Terraria.ModLoader.IO.TagCompound newtag = tag.Get<Terraria.ModLoader.IO.TagCompound>("gamemodeinfo" + ID);
            GameModeID = newtag.GetString("gamemodeid");
            _base = null;
            _Level = newtag.GetInt("level");
            _Exp = newtag.GetInt("exp");
            UpdateMaxExp();
            RecalcLevelExpValue();
            _StatusPoints = newtag.GetInt("statuspoints");
            int statuscount = newtag.GetInt("statuscount");
            MyStatus = new PlayerStatusInfo[GetBase.GameModeStatus.Length];
            for(byte i = 0; i < MyStatus.Length; i++)
            {
                if(newtag.ContainsKey("status" + i))
                    MyStatus[i] = new PlayerStatusInfo() { StatusValue = newtag.GetInt("status" + i) };
                else
                    MyStatus[i] = new PlayerStatusInfo() { StatusValue = newtag.GetInt("status" + i) };
            }
            UpdateStatusPoints();
            if(_EffectiveLevel == _Level)
                _EffectiveLevel = _Level +1;
        }
    }
}
