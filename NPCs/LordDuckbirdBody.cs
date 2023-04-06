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
using static Terraria.ModLoader.PlayerDrawLayer;

namespace StarterMod.NPCs
{
    [AutoloadBossHead]
    public class LordDuckbirdBody : ModNPC
    {
        // QUACK

        public int Stage
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
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

        // Auto-implemented property, acts exactly like a variable by using a hidden backing field
        public Vector2 LastFirstStageDestination { get; set; } = Vector2.Zero;

        private const int FirstStageTimerMax = 90;
        // This is a reference property. It lets us write FirstStageTimer as if it's NPC.localAI[1], essentially giving it our own name
        public ref float FirstStageTimer => ref NPC.localAI[1];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lord Duckbird");
            Main.npcFrameCount[Type] = 15;

            // Specify the debuffs it is immune to
            NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
            {
                SpecificallyImmuneTo = new int[] {
                    BuffID.Confused // Most NPCs have this
				}
            };
            NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);
        }

        public override void SetDefaults()
        {
            NPC.width = 60;
            NPC.height = 60;
            NPC.damage = 40;
            NPC.defense = 0;
            NPC.lifeMax = 20000;
            NPC.HitSound = SoundID.NPCHit50;
            NPC.DeathSound = SoundID.DD2_KoboldExplosion;
            NPC.knockBackResist = 0f;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.value = Item.buyPrice(platinum: 1);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 10f; // Take up open spawn slots, preventing random NPCs from spawning during the fight

            // Custom AI, 0 is "bound town NPC" AI which slows the NPC down and changes sprite orientation towards the target
            NPC.aiStyle = -1;

        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses; // use the boss immunity cooldown counter, to prevent ignoring boss attacks by taking damage from other sources
            return true;
        }

        public override void FindFrame(int frameHeight)
        {
            // This NPC animates with a simple "go from start frame to final frame, and loop back to start frame" rule
            // In this case: First stage: 0-1-2-0-1-2, Second stage: 3-4-5-3-4-5, 5 being "total frame count - 1"
            int startFrame = 3;
            int finalFrame = 10;

            if (Stage == 2)
            {
                startFrame = 11;
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

        public override void HitEffect(int hitDirection, double damage)
        {
            // If the NPC dies, spawn gore and play a sound
            if (Main.netMode == NetmodeID.Server)
            {
                // We don't want Mod.Find<ModGore> to run on servers as it will crash because gores are not loaded on servers
                return;
            }

            if (NPC.life <= 0)
            {
                // These gores work by simply existing as a texture inside any folder which path contains "Gores/"
                int backGoreType = Mod.Find<ModGore>("Gore_558").Type;
                int frontGoreType = Mod.Find<ModGore>("Gore_559").Type;
                int wingGoreType = Mod.Find<ModGore>("Gore_560").Type;

                var entitySource = NPC.GetSource_Death();

                for (int i = 0; i < 2; i++)
                {
                    Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), backGoreType);
                    Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), frontGoreType);
                    Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), wingGoreType);
                }

                SoundEngine.PlaySound(SoundID.Zombie10, NPC.Center);
            }
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }

            Player player = Main.player[NPC.target];

            if (player.dead)
            {
                // If the targeted player is dead, flee
                NPC.velocity.Y -= 0.04f;
                // This method makes it so when the boss is in "despawn range" (outside of the screen), it despawns in 30 ticks
                NPC.EncourageDespawn(30);
                return;
            }

            // TODO: Quacking Everything

            CheckStage();

            if (Stage == 3) {
                DoThirdStage(player);
            }
            else if (Stage == 2) {
                DoSecondStage(player);
            }
            else {
                DoFirstStage(player);
            }
        }

        private void CheckStage()
        {
            // Already in Stage 3, no need to check anything else
            if (Stage == 3) {
                return;
            }

            if (NPC.life <= NPC.lifeMax * 0.2f && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Stage = 3;
                NPC.localAI[0] = 0f;
                NPC.netUpdate = true;
                return;
            }

            // Already in Stage 2, don't update again
            if (Stage == 2) {
                return;
            }

            if (NPC.life <= NPC.lifeMax * 0.75f && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Stage = 2;
                NPC.noGravity = true; // Fly baby flyyy
                NPC.netUpdate = true;
            }
        }

        private void DoFirstStage(Player player)
        {
            Vector2 toPlayer = player.Center - NPC.Center;
            Vector2 toPlayerNormalized = toPlayer.SafeNormalize(Vector2.UnitY);

            float speed = 5f;
            float inertia = 40f;

            //Vector2 moveTo = toPlayerNormalized * speed;

            if (toPlayer.X > 0)
            {
                Vector2 walkTo = new Vector2(1f, 0f) * speed;
                NPC.velocity = (NPC.velocity * (inertia - 1) + walkTo) / inertia;
            }
            else if (toPlayer.X < 0)
            {
                Vector2 walkTo = new Vector2(-1f, 0f) * speed;
                NPC.velocity = (NPC.velocity * (inertia - 1) + walkTo) / inertia;
            }
        }

        private void DoSecondStage(Player player)
        {

            // Each time the timer is 0, pick a random position a fixed distance away from the player but towards the opposite side
            // The NPC moves directly towards it with fixed speed, while displaying its trajectory as a telegraph

            FirstStageTimer++;
            if (FirstStageTimer > FirstStageTimerMax)
            {
                FirstStageTimer = 0;
            }

            float distance = 200; // Distance in pixels behind the player

            if (FirstStageTimer == 0)
            {
                Vector2 fromPlayer = NPC.Center - player.Center;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Important multiplayer concideration: drastic change in behavior (that is also decided by randomness) like this requires
                    // to be executed on the server (or singleplayer) to keep the boss in sync

                    float angle = fromPlayer.ToRotation();
                    float twelfth = MathHelper.Pi / 6;

                    angle += MathHelper.Pi + Main.rand.NextFloat(-twelfth, twelfth);
                    if (angle > MathHelper.TwoPi)
                    {
                        angle -= MathHelper.TwoPi;
                    }
                    else if (angle < 0)
                    {
                        angle += MathHelper.TwoPi;
                    }

                    Vector2 relativeDestination = angle.ToRotationVector2() * distance;

                    FirstStageDestination = player.Center + relativeDestination;
                    NPC.netUpdate = true;
                }
            }

            // Move along the vector
            Vector2 toDestination = FirstStageDestination - NPC.Center;
            Vector2 toDestinationNormalized = toDestination.SafeNormalize(Vector2.UnitY);
            float speed = Math.Min(distance, toDestination.Length());
            NPC.velocity = toDestinationNormalized * speed / 30;

            if (FirstStageDestination != LastFirstStageDestination)
            {
                // If destination changed
                NPC.TargetClosest(); // Pick the closest player target again

                // "Why is this not in the same code that sets FirstStageDestination?" Because in multiplayer it's ran by the server.
                // The client has to know when the destination changes a different way. Keeping track of the previous ticks' destination is one way
                if (Main.netMode != NetmodeID.Server)
                {
                    // For visuals regarding NPC position, netOffset has to be concidered to make visuals align properly
                    NPC.position += NPC.netOffset;

                    // Draw a line between the NPC and its destination, represented as dusts every 20 pixels
                    Dust.QuickDustLine(NPC.Center + toDestinationNormalized * NPC.width, FirstStageDestination, toDestination.Length() / 20f, Color.Yellow);

                    NPC.position -= NPC.netOffset;
                }
            }
            LastFirstStageDestination = FirstStageDestination;

            NPC.rotation = NPC.velocity.ToRotation() - MathHelper.PiOver2;
        }

        private void DoThirdStage(Player player)
        {
            RegenToFull(180); // Only once at the beginning of the phase

            SpinningLaserAttack(player);
        }

        private void RegenToFull(int ticks)
        {
            NPC.localAI[0]++;
            if (NPC.localAI[0] > ticks) {
                return;
            }

            int heal = NPC.lifeMax / ticks;
            if (NPC.life + heal <= NPC.lifeMax) {
                NPC.life += heal;
            }
            else
            {
                NPC.life = NPC.lifeMax;
                NPC.localAI[0] = ticks+1f;
            }
        }

        private void SpinningLaserAttack(Player player)
        {
            float timerMax = 60 * 10;

            if (NPC.localAI[2] > timerMax)
            {
                NPC.localAI[2] = 0;
            }

            if (NPC.localAI[2] == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                // Teleport somewhere above the player & stop moving
                NPC.velocity = Vector2.Zero;
                Vector2 offsetVec = new Vector2(Main.rand.NextFloat(-300f, 301f), Main.rand.NextFloat(-400f, -200f));
                NPC.Center = player.Center + offsetVec;


                // Create the lasers
                var source = NPC.GetSource_FromAI();

                Vector2 position = NPC.Center;
                Vector2 directionX = new Vector2(1f, 0f);
                Vector2 directionY = new Vector2(0f, 1f);
                float speed = 1f;

                //int type = ModContent.ProjectileType<BossHomingProjectile>();
                int type = ProjectileID.PurpleLaser;
                int damage = 100;

                Projectile.NewProjectile(source, position+directionX*50f, directionX * speed, type, damage, 0f, Main.myPlayer);
                Projectile.NewProjectile(source, position-directionX*50f, -directionX * speed, type, damage, 0f, Main.myPlayer);
                Projectile.NewProjectile(source, position+directionY*50f, directionY * speed, type, damage, 0f, Main.myPlayer);
                Projectile.NewProjectile(source, position-directionY*50f, -directionY * speed, type, damage, 0f, Main.myPlayer);
            }

            NPC.localAI[2]++;
        }

    }
}
