using Microsoft.Xna.Framework;
using StarterMod.Projectiles;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarterMod.NPCs
{
    [AutoloadBossHead] // This attribute looks for a texture called "ClassName_Head_Boss" and automatically registers it as the NPC boss head icon
    public class LearningBossBody : ModNPC
    {

        // This boss has a second phase and we want to give it a second boss head icon, this variable keeps track of the registered texture from Load().
        // It is applied in the BossHeadSlot hook when the boss is in its second stage
        public static int secondStageHeadSlot = -1;

        // This code here is called a property: It acts like a variable, but can modify other things. In this case it uses the NPC.ai[] array that has four entries.
        // We use properties because it makes code more readable ("if (SecondStage)" vs "if (NPC.ai[0] == 1f)").
        // We use NPC.ai[] because in combination with NPC.netUpdate we can make it multiplayer compatible. Otherwise (making our own fields) we would have to write extra code to make it work (not covered here)
        public bool SecondStage
        {
            get => NPC.ai[0] == 1f;
            set => NPC.ai[0] = value ? 1f : 0f;
        }

        // More advanced usage of a property, used to wrap around to floats to act as a Vector2
        public Vector2 FirstStageDestination
        {
            get => new Vector2(NPC.ai[1], NPC.ai[2]);
            set
            {
                NPC.ai[1] = value.X;
                NPC.ai[2] = value.Y;
            }
        }

        public override void Load()
        {
            // We want to give it a second boss head icon, so we register one
            string texture = BossHeadTexture + "_SecondStage"; // Our texture is called "ClassName_Head_Boss_SecondStage"
            secondStageHeadSlot = Mod.AddBossHeadTexture(texture, -1); // -1 because we already have one registered via the [AutoloadBossHead] attribute, it would overwrite it otherwise
        }

        public override void BossHeadSlot(ref int index)
        {
            int slot = secondStageHeadSlot;
            if (SecondStage && slot != -1)
            {
                // If the boss is in its second stage, display the other head icon instead
                index = slot;
            }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flying Spaghetti Monster");
            Main.npcFrameCount[Type] = 6;

            // Add this in for bosses that have a summon item, requires corresponding code in the item (See MinionBossSummonItem.cs)
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            // Automatically group with other bosses
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            // Specify the debuffs it is immune to
            NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
            {
                SpecificallyImmuneTo = new int[] {
                    BuffID.Poisoned,

                    BuffID.Confused // Most NPCs have this
				}
            };
            NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);

            // Influences how the NPC looks in the Bestiary
            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                CustomTexturePath = "StarterMod/LearningBoss_Preview",
                PortraitScale = 0.6f, // Portrait refers to the full picture when clicking on the icon in the bestiary
                PortraitPositionYOverride = 0f,
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }

        public override void SetDefaults()
        {
            NPC.width = 110;
            NPC.height = 110;
            NPC.damage = 12;
            NPC.defense = 10;
            NPC.lifeMax = 20000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 5);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 10f; // Take up open spawn slots, preventing random NPCs from spawning during the fight

            // Custom AI, 0 is "bound town NPC" AI which slows the NPC down and changes sprite orientation towards the target
            NPC.aiStyle = -1;

            // Custom boss bar
            //NPC.BossBar = ModContent.GetInstance<MinionBossBossBar>();

            // The following code assigns a music track to the boss in a simple way.
            /*if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/Ropocalypse2");
            }*/
        }

        
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            // Sets the description of this NPC that is listed in the bestiary
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(), // Plain black background
				new FlavorTextBestiaryInfoElement("Too much spaghetti.")
            });
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses; // use the boss immunity cooldown counter, to prevent ignoring boss attacks by taking damage from other sources
            return true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.SpecialBow>(), 4));
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            // If the NPC dies, play a sound
            if (Main.netMode == NetmodeID.Server) {
                // For future: We don't want Mod.Find<ModGore> to run on servers as it will crash because gores are not loaded on servers
                return;
            }

            if (NPC.life <= 0) {
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
            }
        }

        public override void FindFrame(int frameHeight)
        {
            // This NPC animates with a simple "go from start frame to final frame, and loop back to start frame" rule
            // In this case: First stage: 0-1-2-0-1-2, Second stage: 3-4-5-3-4-5, 5 being "total frame count - 1"
            int startFrame = 0;
            int finalFrame = 2;

            if (SecondStage)
            {
                startFrame = 3;
                finalFrame = Main.npcFrameCount[NPC.type] - 1;

                if (NPC.frame.Y < startFrame * frameHeight)
                {
                    // If we were animating the first stage frames and then switch to second stage, immediately change to the start frame of the second stage
                    NPC.frame.Y = startFrame * frameHeight;
                }
            }

            int frameSpeed = 5;
            NPC.frameCounter += 0.5f;
            NPC.frameCounter += NPC.velocity.Length() / 10f; // Make the counter go faster with more movement speed
            if (NPC.frameCounter > frameSpeed)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;

                if (NPC.frame.Y > finalFrame * frameHeight)
                {
                    NPC.frame.Y = startFrame * frameHeight;
                }
            }
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active) {
                NPC.TargetClosest();
            }

            Player player = Main.player[NPC.target];

            if (player.dead) {
                // If the targeted player is dead, flee
                NPC.velocity.Y -= 0.04f;
                // This method makes it so when the boss is in "despawn range" (outside of the screen), it despawns in 30 ticks
                NPC.EncourageDespawn(30);
                return;
            }

            CheckSecondStage();

            if (SecondStage) {
                DoSecondStage(player);
            }
            else {
                DoFirstStage(player);
            }
        }

        private void CheckSecondStage()
        {
            if (SecondStage)
            {
                // No point checking if the NPC is already in its second stage
                return;
            }

            // If boss health is below half, we initiate the second stage, and notify other players that this NPC has reached its second stage
            // by setting NPC.netUpdate to true in this tick. It will send important data like position, velocity and the NPC.ai[] array to all connected clients
            // Because SecondStage is a property using NPC.ai[], it will get synced this way
            if (NPC.life <= NPC.lifeMax/2 && Main.netMode != NetmodeID.MultiplayerClient) {
                SecondStage = true;
                NPC.netUpdate = true;
            }
        }

        private void DoFirstStage(Player player)
        {
            Vector2 toPlayer = player.Center - NPC.Center;

            float offsetX = 300f;

            Vector2 abovePlayer = player.Top + new Vector2(NPC.direction * offsetX, -NPC.height);

            Vector2 toAbovePlayer = abovePlayer - NPC.Center;
            Vector2 toAbovePlayerNormalized = toAbovePlayer.SafeNormalize(Vector2.UnitY);

            // The NPC tries to go towards the offsetX position, but most likely it will never get there exactly, or close to if the player is moving
            // This checks if the npc is "70% there", and then changes direction
            float changeDirOffset = offsetX * 0.7f;

            if (NPC.direction == -1 && NPC.Center.X - changeDirOffset < abovePlayer.X ||
                NPC.direction == 1 && NPC.Center.X + changeDirOffset > abovePlayer.X)
            {
                NPC.direction *= -1;
            }

            float speed = 8f;
            float inertia = 40f;

            // If the boss is somehow below the player, move faster to catch up
            if (NPC.Top.Y > player.Bottom.Y)
            {
                speed = 12f;
            }

            Vector2 moveTo = toAbovePlayerNormalized * speed;
            NPC.velocity = (NPC.velocity * (inertia - 1) + moveTo) / inertia;

            ShootLaser(player);

            NPC.damage = NPC.defDamage;

            NPC.alpha = 0;

            NPC.rotation = toPlayer.ToRotation() - MathHelper.PiOver2;
        }

        private void DoSecondStage(Player player)
        {
            Vector2 toPlayer = player.Center - NPC.Center;

            float offsetX = 300f;
            float maxDistance = 400f;
            float sqrMaxDistance = maxDistance * maxDistance;
            float sqrDistanceToTarget = Vector2.DistanceSquared(player.Center, NPC.Center);

            Vector2 abovePlayer = player.Top + new Vector2(NPC.direction * offsetX, -NPC.height);

            Vector2 toAbovePlayer = abovePlayer - NPC.Center;
            Vector2 toAbovePlayerNormalized = toAbovePlayer.SafeNormalize(Vector2.UnitY);

            float changeDirOffset = offsetX * 0.7f;

            if (NPC.direction == -1 && NPC.Center.X - changeDirOffset < abovePlayer.X ||
                NPC.direction == 1 && NPC.Center.X + changeDirOffset > abovePlayer.X)
            {
                NPC.direction *= -1;
            }

            float speed = 8f;
            float inertia = 40f;

            // If the boss is too far from the player, get closer
            if (sqrMaxDistance < sqrDistanceToTarget)
            {
                //speed += 10f;
                Vector2 moveTo = toAbovePlayerNormalized * speed;
                NPC.velocity = (NPC.velocity * (inertia - 1) + moveTo) / inertia;
            }


            ShootLaser(player);

            SpawnHomingProj(player);

            DashingAttack(player);

            NPC.damage = NPC.defDamage;

            // Turn towards the player/target at end of tick
            NPC.rotation = toPlayer.ToRotation() - MathHelper.PiOver2;
        }

        private void ShootLaser(Player player)
        {
            // At 100% health, spawn every 300 ticks
            // Drops down until 20% health to spawn every 60 ticks
            float timerMax = Utils.Clamp((float)NPC.life / NPC.lifeMax, 0.2f, 1f) * 300;

            NPC.localAI[1]++;
            if (NPC.localAI[1] > timerMax)
            {
                NPC.localAI[1] = 0;
            }

            // Shoot some lasers
            if (NPC.HasValidTarget && NPC.localAI[1] == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                var source = NPC.GetSource_FromAI();
                Vector2 position = NPC.Center;
                Vector2 targetPosition = Main.player[NPC.target].Center;
                Vector2 direction = targetPosition - position;
                direction.Normalize();
                float speed = 10f;
                int type = ProjectileID.PinkLaser;
                int damage = NPC.damage; //the damage passed into NewProjectile will be applied doubled, and quadrupled if expert mode, so keep that in mind when balancing projectiles if you scale it off NPC.damage (which also increases for expert/master)
                Projectile.NewProjectile(source, position, direction * speed, type, damage, 0f, Main.myPlayer);
            }
        }

        private void SpawnHomingProj(Player player)
        {
            float timerMax = 300 - NPC.localAI[3];

            NPC.localAI[2]++;
            if (NPC.localAI[2] > timerMax) {
                NPC.localAI[2] = 0;
                if (NPC.localAI[3] < 120) {
                    NPC.localAI[3]++;
                }
            }

            if (NPC.HasValidTarget && NPC.localAI[2] == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                var source = NPC.GetSource_FromAI();
                float speed = 1f;
                Vector2 targetPosition = Main.player[NPC.target].Center;

                float kitingOffsetX = Utils.Clamp(player.velocity.X * 16, -100, 100);
                Vector2 position = player.Center + new Vector2(kitingOffsetX + Main.rand.Next(-100, 100), Main.rand.Next(-100, 100));

                Vector2 direction = targetPosition - position;

                int type = ModContent.ProjectileType<BossHomingProjectile>();
                int damage = 10;
                Projectile.NewProjectile(source, position, direction * speed, type, damage, 0f, Main.myPlayer);
            }
        }

        private void DashingAttack(Player player)
        {
            Vector2 toPlayer = player.Center - NPC.Center;

            float offsetX = 400f;
            float maxDistance = 600f;
            float sqrMaxDistance = maxDistance * maxDistance;
            float sqrDistanceToTarget = Vector2.DistanceSquared(player.Center, NPC.Center);

            float speed = 15f;
            NPC.damage = NPC.defDamage * 3; // Broken because 'main' DoSecondStage loop resets dmg too quick (next tick)

            // Dash at start of stage 2 and after spawning homingProj
            if (NPC.localAI[2] == 0) { 
                NPC.velocity = toPlayer.SafeNormalize(Vector2.Zero) * speed;
                NPC.rotation = toPlayer.ToRotation() - MathHelper.PiOver2;
            }

            // Double dash if below 25%
            // Only use 60 ticks after 1st dash
            if (NPC.life <= NPC.lifeMax * 0.25f && NPC.localAI[2] == 60)
            {
                SoundEngine.PlaySound(SoundID.Zombie10, NPC.Center); // QUACK
                NPC.velocity = toPlayer.SafeNormalize(Vector2.Zero) * speed;
                NPC.rotation = toPlayer.ToRotation() - MathHelper.PiOver2;
            }

            // Triple dash if below 15%
            // Only use 120 ticks after 1st dash, 60 ticks after 2nd
            if (NPC.life <= NPC.lifeMax * 0.15f && NPC.localAI[2] == 120)
            {
                SoundEngine.PlaySound(SoundID.Zombie11, NPC.Center); // QUACK 2
                NPC.velocity = toPlayer.SafeNormalize(Vector2.Zero) * speed;
                NPC.rotation = toPlayer.ToRotation() - MathHelper.PiOver2;
            }
        }

    }
}
