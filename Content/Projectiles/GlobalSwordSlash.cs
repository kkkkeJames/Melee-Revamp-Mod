using MeleeRevamp.Content.Core;
using MeleeRevamp.Content.Particles;
using Microsoft.Build.Execution;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core;
using ReLogic.Content;
using SDL2;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace MeleeRevamp.Content.Projectiles
{
    public class ChargeEffect
    {
        public static Effect ChargeBorder = ModContent.Request<Effect>("MeleeRevamp/Content/Effects/Border", AssetRequestMode.ImmediateLoad).Value;
    }
    public class DissolveEffect
    {
        public static Effect Dissolve = ModContent.Request<Effect>("MeleeRevamp/Content/Effects/Dissolve", AssetRequestMode.ImmediateLoad).Value;
    }
    public abstract class GlobalSwordSlash : ProjectileStateMachine, IDrawWarp
    {
        public static Asset<Texture2D> SlashTexture;
        public static Asset<Texture2D> WarpTexture;
        public static Asset<Texture2D> DissolveTexture;
        public override void Load()
        {
            SlashTexture = ModContent.Request<Texture2D>("MeleeRevamp/Content/Assets/SlashTex");
            WarpTexture = ModContent.Request<Texture2D>("MeleeRevamp/Content/Assets/WarpTex");
            DissolveTexture = ModContent.Request<Texture2D>("MeleeRevamp/Content/Assets/DissolveTex");
        }
        public override void Unload()
        {
            SlashTexture = null;
            WarpTexture = null;
            DissolveTexture = null;
        }
        #region Globalvariables
        public Texture2D SwordTexture; // Sword Texture
        public bool SwordOwnTexture = false; // Whether to use the item texture or other textures
        public Item ShouldHeldItem; // The item player should hold
        public bool FlipSwordTexture = false; // Whether to draw the sword inversely
        public float SwordRadius; // Calculate the radius of sword
        public Vector2 ArmToSwordOffset; // The vector from arm to sword
        public bool ShouldDrawArm; // Whether the arm of player is provided by code
        public float ArmRotation; // The rotation of player arm if shoulddrawarm
        public bool DrawBehindPlayer = false; // Whether sword should be drawn behind the player
        public float DamageScale = 1; // The damage scale of the projectile
        public bool CanParryProj = false; // Whether the attack can block projectiles (not implemented)
        public bool ChargeShader = false; // Whether use the shader of charge border
        public int ShootProj = 0; // Projectile the sword should shoot
        public float ShootProjDamScale = 1; // Projectile damage scale
        public bool ApplyStuck = false; // Whether apply stuck frames
        public bool ApplySlashDust = false; // Whether apply dust for slash
        public bool AttackHit = false; // Whether the attack had landed, which serves as immunity frame count
        public float SwordPowerGaugeAdd = 0; // the sword power an attack should add
        public int AlternateAttackIndex = 0; // The index of the Alternate attack
        public int AlternateAttackCount = 0; // The number of Alternate attacks
        #endregion
        #region SMVariables
        public struct SwordPosStruct // The structure to store variables for defining sword position
        {
            public Vector2 SwordArmAdd; // The vector between sword and player arm
            public float SwordRot; // Angle of projectile
            public float ArmRot; // Angle of arm
            public float Scale; // Scale of projectile
            public bool ShouldSetStruct; // Whether to set the current struct
            public SwordPosStruct()
            {
                ShouldSetStruct = true;
            }
            public void SetCurrentStruct(Projectile proj) // Set the struct using the current sword projectile's data
            {
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                SwordArmAdd = projmod.ArmToSwordOffset;
                SwordRot = proj.rotation;
                ArmRot = projmod.ArmRotation;
                Scale = proj.scale;
                ShouldSetStruct = false;
            }
            public void SetStruct(Vector2 add, float rot, float armrot, float scale) // Set the struct using data
            {
                SwordArmAdd = add;
                SwordRot = rot;
                ArmRot = armrot;
                Scale = scale;
            }
            public bool DidSetStruct() // Return whether the struct is already set. If yes, then the struct is already set, no nead to read values again
            {
                return !ShouldSetStruct;
            }
        }
        public SwordPosStruct StartStruct = new SwordPosStruct(); // The structure for start position, to track the current position of sword, lerp between this and a target struct
        public SwordPosStruct TargetStruct1; // The structure for targeting position, to track the current position of sword, lerp between the start struct and this or another
        public SwordPosStruct TargetStruct2; // The structure for targeting position, to track the current position of sword, lerp between the start struct and this or another
        public float TimeMax; // The maximum time for a state/move
        public float StopTime; // The time to stop moving. Some attacks is better to end abruptly
        public bool ShouldCountMouse = true; // Whether it should consider
        public Vector2 MousePos; // The position of mouse
        public bool CouldHit = false; // Whether the projectile could hit
        public float ComboTimer = 0; // The timer for recovery in Recover state, within a few time, landing another attack will continue the combo
        public bool isCombo = false; // Whether to incremenet recovery timer
        public int ComboCount = 0; // The count of combo, reset when player stop attacking for a while
        public int MaxComboCount = 0; // The max count of combo
        public bool LeftClick = false; // Detect left click input
        public bool RightClick = false; // Detect right click input
        #endregion
        #region DrawVariables
        public bool ApplyDissolve; //Whether apply disoolve shader while drawing
        public float DissolveRate; //0-1, positively related to intensity of dissolve applied
        public Color SlashColor; //The color of vertex paint for slash
        public int SlashDrawTimer; //Timer for calculating draw of slash
        public int SlashDrawTimeMax; //The max timer for calculating draw of slash
        public bool DrawSword = true; //Whether draw the item itself
        public bool DrawSlash = true; //Whether draw the slash of item
        #endregion
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40; // At most store a cache of trail of 40 frames
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2; // This trailing mode allows storing of world position while replacing oldest ones with newest
        }
        public override bool ShouldUpdatePosition()
        {
            return false;
        }
        // This function sets the default of projectile. Since this projectile should behave same as the vanilla melee, damage and crit chance should not be set previously
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.extraUpdates = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.hide = true;
        }
        // This function sets what should be done when this projectile is spawned. We suppose that in this case, a melee weapon is just being held
        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];
            MeleeRevampPlayer MeleeRevampPlayer = Main.LocalPlayer.GetModPlayer<MeleeRevampPlayer>();
            if (source is EntitySource_ItemUse Use) // Get the type of item held by player and set the corresponding item type of this projectile same as that
            {
                ShouldHeldItem = Use.Item;
            }
            MeleeRevampPlayer.SwordPowerGauge = 0; // Clear sword power gauge as the player had just hold a melee weapon
        }
        // The logistic helper for calculate the process of wielding sword from 0 to 1, timer is the time percentage, scaler is the scaler parameter for timer
        public float SwordLogisticHelper(float timer, float scaler = 80) 
        {
            return MeleeRevampMathHelper.LogisticHelper(3.14f, 1.57f, -0.23f / 4, 1.57f, timer * scaler) / MeleeRevampMathHelper.LogisticHelper(3.14f, 1.57f, -0.23f / 4, 1.57f, 80);
        }
        // Lerp between two structs, elements are linearly lerped in default
        // Will automatically record the initial state using StartStruct, lerp between that and an input structure
        // timer is ranged from 0-1, which is the percentage of time processing
        // closer_swordangle is true if the sword rotation should be calculated using the closer angle (for example, from 350 degree to 10 degree should be 20 degree instead of 340 degree), similar for closer_armangle
        // logistic is true if both sword and arm angles should be calculated using logistic helper instead of linear lerp
        public void LerpSwordStruct(Projectile proj, SwordPosStruct TargetStruct, float timer, bool closer_swordangle = false, bool closer_armangle = false, bool logistic = false)
        {
            GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
            Player player = Main.player[proj.owner];
            if (!projmod.StartStruct.DidSetStruct()) //While iniset.flag is false, read in new set for iniset and set flag to true
                StartStruct.SetCurrentStruct(proj);
            if (timer >= 1) StartStruct.ShouldSetStruct = true;
            projmod.ArmToSwordOffset = Vector2.Lerp(projmod.StartStruct.SwordArmAdd, TargetStruct.SwordArmAdd, timer);
            proj.scale = MathHelper.Lerp(projmod.StartStruct.Scale, TargetStruct.Scale, timer);
            // Brute force to calcualte the rotation of arm and sword, taking its arm rotation and the range of angles in degrees into consideration
            if (closer_swordangle)
            {
                while (projmod.StartStruct.SwordRot - TargetStruct.SwordRot >= Math.PI * 2)
                    projmod.StartStruct.SwordRot -= (float)Math.PI * 2;
                while (projmod.StartStruct.SwordRot - TargetStruct.SwordRot <= -Math.PI * 2)
                    projmod.StartStruct.SwordRot += (float)Math.PI * 2;
                if (projmod.StartStruct.SwordRot - TargetStruct.SwordRot >= Math.PI)
                    projmod.StartStruct.SwordRot -= (float)Math.PI * 2;
                if (projmod.StartStruct.SwordRot - TargetStruct.SwordRot <= -Math.PI)
                    projmod.StartStruct.SwordRot += (float)Math.PI * 2;
            }
            if (closer_armangle)
            {
                while (projmod.StartStruct.ArmRot - TargetStruct.ArmRot >= Math.PI * 2)
                    projmod.StartStruct.ArmRot -= (float)Math.PI * 2;
                while (projmod.StartStruct.ArmRot - TargetStruct.ArmRot <= -Math.PI * 2)
                    projmod.StartStruct.ArmRot += (float)Math.PI * 2;
                if (projmod.StartStruct.ArmRot - TargetStruct.ArmRot >= Math.PI)
                    projmod.StartStruct.ArmRot -= (float)Math.PI * 2;
                if (projmod.StartStruct.ArmRot - TargetStruct.ArmRot <= -Math.PI)
                    projmod.StartStruct.ArmRot += (float)Math.PI * 2;
            }
            // Then use lerp to calculate rotation with timer
            if (logistic)
            {
                projmod.ArmRotation = MathHelper.Lerp(projmod.StartStruct.ArmRot, TargetStruct.ArmRot, SwordLogisticHelper(timer));
                proj.rotation = MathHelper.Lerp(projmod.StartStruct.SwordRot, TargetStruct.SwordRot, SwordLogisticHelper(timer));
            }
            else
            {
                proj.rotation = MathHelper.Lerp(projmod.StartStruct.SwordRot, TargetStruct.SwordRot, timer);
                projmod.ArmRotation = MathHelper.Lerp(projmod.StartStruct.ArmRot, TargetStruct.ArmRot, timer);
            }
        }
        // Initialize all (abstract) functions for state machine 
        public abstract void RegisterVariables();
        public abstract void Appear();
        public override void Initialize()
        {
            RegisterVariables();
            Projectile.rotation = -(float)Math.PI / 2;
            Appear();
            RegisterState(new Idle());
            RegisterState(new Wield());
            RegisterState(new Stab());
            RegisterState(new Recover());
        }
        public override void AIBefore()
        {
            Player player = Main.player[Projectile.owner];
            MeleeRevampPlayer MeleeRevampPlayer = Main.LocalPlayer.GetModPlayer<MeleeRevampPlayer>();
            SwordTexture = ModContent.Request<Texture2D>(Texture).Value; // Let the sword texture be what the player hold

            if (player.HeldItem != ShouldHeldItem) Projectile.Kill();
            else Projectile.timeLeft = 10; // If the player (do not) switch weapon, hold/kill this projectile. As AI is sealed for this class, we modify this in AIbefore
            Projectile.width = (int)(Projectile.scale * SwordTexture.Width);
            Projectile.height = (int)(Projectile.scale * SwordTexture.Height); //Projectile lenghts = texture lenghts * projectile scale
            SwordRadius = MeleeRevampMathHelper.PythagoreanHelper(Projectile.width, Projectile.height) / 2; // Use pythagorean to calculate radius of texture
            DrawBehindPlayer = false; // Draw the sword behind the player
            if (!MeleeRevampConfigClient.Instance.SwordPowerGaugeDisable)
                Projectile.damage = (int)(player.HeldItem.damage * DamageScale * (1 + MeleeRevampPlayer.SwordPowerGauge / 2f)); // Damage = item damage * damage scale (mostly modified by attacks) * (1 + SwordPowerGauge/2f)
            Projectile.CritChance = player.HeldItem.crit; // Crit = Item crit
            Projectile.knockBack = player.HeldItem.knockBack; // knockback = Item knockback
            Projectile.localNPCHitCooldown = (int)(player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 4;

            if (MeleeRevamp.SwitchAlternateAttack.JustPressed) // Switch Alternate Attack mode even before AI
                AlternateAttackIndex = AlternateAttackCount == 0 ? 0 : (AlternateAttackIndex + 1) % AlternateAttackCount;

            if (Main.mouseLeft && !Main.mouseLeftRelease && player.itemTime <= 0 && player.itemAnimation <= 0)
                LeftClick = true;
            else LeftClick = false;
            if (Main.mouseRight && !Main.mouseRightRelease && player.itemTime <= 0 && player.itemAnimation <= 0)
                RightClick = true;
            else RightClick = false;
        }
        // Idle : The projectile is invisible
        public bool IdleState = false;
        public class Idle : ProjectileState
        {
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                return;
            }
            public override void AI(ProjectileStateMachine projectile)
            {
                #region Variables
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                #endregion
                #region Variables of this state
                projmod.IdleState = true;
                #endregion
                #region Variables unrelated to player direction
                proj.rotation = 0.5f * (float)Math.PI; // The sword points downward
                proj.scale = 1.2f; // Scale = 1.2f
                projmod.DrawSword = false; // Do not draw sword
                projmod.CouldHit = false;
                projmod.ShouldDrawArm = false; // Do not affect arm angle
                projmod.DrawBehindPlayer = true; // Draw sword behind player
                #endregion
                #region Variables related to player direction
                if (player.direction == -1) projmod.FlipSwordTexture = true;
                else projmod.FlipSwordTexture = false;
                projmod.ArmToSwordOffset = new Vector2(0, 12);
                projmod.ArmRotation = 0;
                #endregion
            }
            public override void SwitchState(ProjectileStateMachine projectile)
            {
                throw new NotImplementedException();
            }
        }
        #region Wield variables
        public float[] WieldDrawRadius = new float[10001]; // The cache for storing past radii, used as inputs for vertex paint
        public bool WieldAttack = false; // Whether draw slash
        public int WieldStuckTimer = 0; // Timer of stuck frames
        public int WieldStopTime; // Time to stop attack
        public float WieldHandleLength; // The length of handle for wield
        public float WieldDrawLessLength = 0; // Distance from vertex paint to sword
        #endregion
        public class Wield : ProjectileState
        {
            #region Wield state variables
            public float WieldStandardScale; // The long radius' scale for ellipse
            public float WieldThinScale; // The ratio between the short and long radius of ellipse
            public float WieldHoldRot; // The hold rot
            public float WieldFinalRot; // The final rot
            public bool WieldCombo; // Whether start the combo recovery timer after this wield
            public int WieldDirection; // Player direction-just for debugging
            #endregion
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                if (args.Length < 6)
                    throw new Exception ("Not enough arguments for switching to Wield state.");
                bool countMouseAngle = (bool)args[0];
                float longRadius = (float)args[1];
                float shortRadiusScale = (float)args[2];
                float startAngle = (float)args[3];
                float endAngle = (float)args[4];
                float SPGaugeAdd = (float)args[5];
                float SPGaugeCost = args.Length >= 7 ? (float)args[6] : 0;
                bool wieldCombo = args.Length >= 8 ? (bool) args[7] : true;
                float handleLength = args.Length >= 9 ? (float)args[8] : 0f;
                bool applyStuckVisual = args.Length >= 10 ? (bool)args[9] : false;
                bool applyScreenShake = args.Length >= 11 ? (bool)args[10] : false;
                float stopRotTime = args.Length >= 12 ? (float)args[11] : 0f;
                float damageScale = args.Length >= 13 ? (float)args[12] : 1f;

                if (SPGaugeCost > 0.0f)
                {
                    player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge -= SPGaugeCost;
                    if (player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge < 0.0f) player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge = 0.0f;
                    player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge = MathF.Round(player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge, 1);
                }

                player.direction = (Main.MouseWorld - player.Center).X >= 0 ? 1 : -1; // Change player direction based on mouse position
                WieldCombo = wieldCombo;
                projectile.Timer = 0; // Reset timer
                projmod.ShouldCountMouse = countMouseAngle; // Should this wield count mouse angle into consideration
                if (countMouseAngle) projmod.MousePos = Main.MouseWorld - player.Center; // If yes, get mouse rotation
                WieldHoldRot = startAngle; // Get the final angle of hold
                WieldFinalRot = endAngle; // Get the final angle of wielding
                projmod.FlipSwordTexture = player.direction < 0 ? true : false; // Change direction of projectile draw based on player direction
                if (endAngle < startAngle) projmod.FlipSwordTexture = !projmod.FlipSwordTexture;
                if (player.direction < 0)
                {
                    WieldHoldRot = (float)Math.PI - WieldHoldRot; // If player's direction is left, change hold rot and final rot based on them
                    WieldFinalRot = (float)Math.PI - WieldFinalRot;
                }
                float scl = MeleeRevampMathHelper.EllipseRadiusHelper(longRadius, longRadius * shortRadiusScale, WieldHoldRot);
                if (countMouseAngle) // Take mouse into consideration
                {
                    WieldHoldRot += (float)Math.Atan(projmod.MousePos.Y / projmod.MousePos.X);
                    WieldFinalRot += (float)Math.Atan(projmod.MousePos.Y / projmod.MousePos.X);
                }
                WieldStandardScale = longRadius; // Change radius issue
                WieldThinScale = shortRadiusScale;
                projmod.WieldHandleLength = handleLength;
                projmod.WieldDrawLessLength = handleLength;
                projmod.TargetStruct1.SetStruct(new Vector2(-projmod.WieldHandleLength, 0).RotatedBy(WieldHoldRot), WieldHoldRot, WieldHoldRot - (float)Math.PI / 2f, scl);
                // Calculate the final rotation by all those inputs with the ellipse radius helper
                int targscaleflag = projmod.ShouldCountMouse ? 1 : 0;
                float targscale = MeleeRevampMathHelper.EllipseRadiusHelper(WieldStandardScale, WieldStandardScale * WieldThinScale, WieldFinalRot - (float)Math.Atan(projmod.MousePos.Y / projmod.MousePos.X) * targscaleflag);
                projmod.TargetStruct2.SetStruct(Vector2.Zero, WieldFinalRot, WieldFinalRot - (float)Math.PI / 2, targscale); // Set the targetset
                if (!projmod.DrawSword)
                {
                    projmod.DrawSword = true;
                    projmod.ApplyDissolve = true;
                }
                projmod.DamageScale = damageScale;
                projmod.ApplyStuck = applyStuckVisual;
                projmod.ApplySlashDust = true;
                projmod.ApplyScreenShake = applyScreenShake;
                projmod.SwordPowerGaugeAdd = SPGaugeAdd;
                projmod.isCombo = false;
                projmod.TimeMax = (player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 4;
                WieldDirection = player.direction;
            }
            public override void AI(ProjectileStateMachine projectile)
            {
                #region Basic settings
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                player.direction = WieldDirection;
                projmod.ShouldDrawArm = true; // Player's arm angle is determined by code
                player.itemAnimation = player.itemTime = 2; // The player is always in using weapon state
                int HoldupTimeMax = 24; // The time player hold up the sword, which is 6f in this case
                int WieldTimeMax = (int)projmod.TimeMax - HoldupTimeMax; // The time player wield the sword
                if (projmod.WieldStuckTimer > 0) // Modify stuck frames
                    projmod.WieldStuckTimer--;
                else projmod.Timer++;
                #endregion
                #region Hold up the sword
                if (projmod.Timer <= HoldupTimeMax) 
                {
                    float timer = (float)projmod.Timer / (float)HoldupTimeMax;
                    //projmod.StartStruct.SetCurrentStruct(proj);
                    projmod.LerpSwordStruct(proj, projmod.TargetStruct1, timer, true, true);
                    //projmod.MoveSwordSet(proj, projmod.TargetStruct1, timer);
                    if (projmod.ApplyDissolve) projmod.DissolveRate = timer;
                }
                #endregion
                #region Wield the sword
                else
                {
                    projmod.ApplyDissolve = false;
                    projmod.WieldAttack = true;
                    projmod.CouldHit = true;
                    int WieldTimer;
                    WieldTimer = projmod.SlashDrawTimer = projmod.Timer - HoldupTimeMax;
                    #region Modify angle, radius, etc.
                    projmod.LerpSwordStruct(proj, projmod.TargetStruct2, (float)(projmod.Timer - HoldupTimeMax) / (float)(projmod.TimeMax - HoldupTimeMax), false, false, true);
                    //projmod.RotSetTargetLogistic(proj, projmod.TargetStruct2, (float)(projmod.Timer - HoldupTimeMax) / (float)(projmod.TimeMax - HoldupTimeMax)); // Use logistics function to determine rotation of projectile
                    //projmod.ArmToSwordOffset = new Vector2(-projmod.WieldHandleLength, 0).RotatedBy(proj.rotation); // Modify the player's arm rotation
                    proj.scale = MeleeRevampMathHelper.EllipseRadiusHelper(WieldStandardScale, WieldStandardScale * WieldThinScale, projmod.Projectile.rotation - (projmod.ShouldCountMouse ? (float)Math.Atan(projmod.MousePos.Y / projmod.MousePos.X) : 0)); // Change sword's scale
                    //projmod.ArmRotation = proj.rotation - (float)Math.PI / 2; // The arm moves with the sword
                    projmod.WieldDrawRadius[WieldTimer] = projmod.SwordRadius;
                    #endregion
                    #region Shoot Projectile (If the projectile shot is set in the weapon item)
                    if (player.HeldItem.shoot != 0 && projmod.Timer == HoldupTimeMax + 1)
                    {
                        Projectile.NewProjectileDirect(proj.GetSource_FromThis(), player.Center, projmod.MousePos / projmod.MousePos.Length() * player.HeldItem.shootSpeed, player.HeldItem.shoot, (int)(proj.damage * 0.75f), proj.knockBack);
                    }
                    #endregion
                    #region (Not Implemented) Deflect projectiles
                    if (projmod.Timer >= -12 + HoldupTimeMax + WieldTimeMax / 4 && projmod.Timer <= 12 + HoldupTimeMax + WieldTimeMax / 4)
                    {
                        foreach (Projectile counterproj in Main.projectile)
                        {
                            Rectangle projhitbox = new Rectangle((int)(proj.Center.X + new Vector2(projmod.SwordRadius, 0).RotatedBy(proj.rotation).X) - 10, (int)(proj.Center.Y + new Vector2(projmod.SwordRadius, 0).RotatedBy(proj.rotation).Y) - 10, 20, 20);
                            Rectangle counterhitbox = counterproj.Hitbox;
                        }
                    }
                    #endregion
                    #region Switch states
                    if (projmod.Timer >= projmod.TimeMax)
                    {
                        SwitchState(projectile);
                    }
                    #endregion
                }
                #endregion
            }
            public override void SwitchState(ProjectileStateMachine projectile)
            {
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.StartStruct.SetStruct(projmod.ArmToSwordOffset, proj.rotation, projmod.ArmRotation, proj.scale);
                projmod.TargetStruct2.SetStruct(new Vector2(0, 0), player.direction > 0 ? 0.1f * (float)Math.PI : 0.9f * (float)Math.PI, 0, 1.6f);
                projmod.Timer = 0;
                projmod.TimeMax = 240;
                projmod.isCombo = WieldCombo;
                projmod.ComboCount = (projmod.ComboCount + (WieldCombo ? 1 : 0)) % projmod.MaxComboCount;
                projmod.SetState<Recover>();
            }
        }
        public bool StabAttack = false; 
        public bool StabAttackStopDraw = false; 
        /*
         * State: Stab
         * Structure is similar to wield, but more time based as their attack ways are totally different
         */
        public class Stab : ProjectileState
        {
            public Vector2 StabStartPosAdd;
            public Vector2 StabEndPosAdd;
            public bool StabCombo; // Whether start the combo recovery timer after this stab
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                if (args.Length < 2)
                    throw new Exception("Not enough arguments for switching to Stab state.");
                bool countMouseAngle = (bool)args[0];
                float SPGaugeAdd = (float)args[1];
                bool stabCombo = args.Length >= 3 ? (bool)args[2] : true;
                float damageScale = args.Length >= 4 ? (float)args[3] : 1f;
                StabCombo = stabCombo;
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                player.direction = (Main.MouseWorld - player.Center).X >= 0 ? 1 : -1; // Change player direction based on mouse position
                projmod.Timer = 0;
                projmod.StartStruct.SetStruct(projmod.ArmToSwordOffset, proj.rotation, projmod.ArmRotation, proj.scale);
                projmod.MousePos = Main.MouseWorld - player.Center;
                float exrot = projmod.MousePos.X > 0 ? 0 : (float)Math.PI;
                if (countMouseAngle)
                {
                    StabStartPosAdd = new Vector2(-projmod.SwordRadius / 2, 0).RotatedBy((float)Math.Atan(projmod.MousePos.Y / projmod.MousePos.X) + exrot);
                    StabEndPosAdd = new Vector2(0, 0).RotatedBy((float)Math.Atan(projmod.MousePos.Y / projmod.MousePos.X) + exrot);
                    projmod.TargetStruct1.SetStruct(StabStartPosAdd, (float)Math.Atan(projmod.MousePos.Y / projmod.MousePos.X) + exrot, (float)Math.Atan(projmod.MousePos.Y / projmod.MousePos.X) + exrot - (float)Math.PI / 2, 1.6f);
                    projmod.TargetStruct2.SetStruct(StabStartPosAdd, (float)Math.Atan(projmod.MousePos.Y / projmod.MousePos.X) + exrot, (float)Math.Atan(projmod.MousePos.Y / projmod.MousePos.X) + exrot - (float)Math.PI / 2, 1.6f);
                }
                else
                {
                    StabStartPosAdd = new Vector2(-projmod.SwordRadius / 2, 0).RotatedBy(exrot);
                    StabEndPosAdd = new Vector2(0, 0).RotatedBy(exrot);
                    projmod.TargetStruct1.SetStruct(StabStartPosAdd, exrot, exrot - (float)Math.PI / 2, 1.6f);
                    projmod.TargetStruct2.SetStruct(StabStartPosAdd, exrot, exrot - (float)Math.PI / 2, 1.6f);
                }
                projmod.ApplyStuck = true;
                projmod.ApplySlashDust = true;
                projmod.SwordPowerGaugeAdd = SPGaugeAdd;
                projmod.DamageScale = SPGaugeAdd;
                projmod.isCombo = false;
            }
            public override void AI(ProjectileStateMachine projectile)
            {
                #region Basic settings
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner]; 
                player.itemAnimation = player.itemTime = 2; 
                projmod.TimeMax = (player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 4;
                int HoldupTimeMax = (int)(projmod.TimeMax / 2); // Notice: hold up time = item.usetime / 4
                int StabTimeMax = (int)(projmod.TimeMax / 4); // Stab time = item.usetime / 2
                int RecoverTimeMax = (int)projmod.TimeMax - HoldupTimeMax - StabTimeMax; // recover time = item.usetime / 4
                projmod.Timer++; 
                #endregion
                #region Hold up the sword
                if (projmod.Timer <= HoldupTimeMax)
                {
                    projmod.LerpSwordStruct(proj, projmod.TargetStruct1, (float)projmod.Timer / HoldupTimeMax, true, true);
                    //projmod.MoveSwordSet(proj, projmod.TargetStruct1, (float)projmod.Timer / HoldupTimeMax);
                }
                #endregion
                #region Stab
                else if (projmod.Timer <= HoldupTimeMax + StabTimeMax)
                {
                    projmod.StabAttack = true;
                    projmod.CouldHit = true;
                    projmod.SlashDrawTimer = projmod.Timer - HoldupTimeMax;
                    projmod.SlashDrawTimeMax = StabTimeMax;
                    if (projmod.Timer == HoldupTimeMax + 1)
                    {
                        projmod.StartStruct.SetStruct(projmod.ArmToSwordOffset, proj.rotation, projmod.ArmRotation, proj.scale); 
                        projmod.TargetStruct2.SetStruct(StabEndPosAdd, proj.rotation, projmod.ArmRotation, proj.scale);
                    }
                    projmod.LerpSwordStruct(proj, projmod.TargetStruct2, (projmod.Timer - HoldupTimeMax) / (float)StabTimeMax, true, true);
                }
                #endregion
                #region Recover
                else
                {
                    projmod.StabAttack = false;
                    projmod.StabAttackStopDraw = true;
                    projmod.SlashDrawTimer = projmod.Timer - HoldupTimeMax - StabTimeMax;
                    projmod.SlashDrawTimeMax = RecoverTimeMax;
                    if (projmod.Timer == HoldupTimeMax + StabTimeMax + 1)
                    {
                        projmod.StartStruct.SetStruct(projmod.ArmToSwordOffset, proj.rotation, projmod.ArmRotation, proj.scale); 
                        projmod.TargetStruct2.SetStruct(StabStartPosAdd, proj.rotation, projmod.ArmRotation, proj.scale);
                    }
                    projmod.LerpSwordStruct(proj, projmod.TargetStruct2, (projmod.Timer - HoldupTimeMax - StabTimeMax) / (float)RecoverTimeMax, true, true);
                }
                #endregion
                #region Switch state
                if (projmod.Timer >= projmod.TimeMax)
                {
                    SwitchState(projectile);
                }
                #endregion
            }
            public override void SwitchState(ProjectileStateMachine projectile)
            {
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.StartStruct.SetStruct(projmod.ArmToSwordOffset, proj.rotation, projmod.ArmRotation, proj.scale);
                projmod.TargetStruct2.SetStruct(new Vector2(0, 0), player.direction > 0 ? 0.1f * (float)Math.PI : 0.9f * (float)Math.PI, 0, 1.6f);
                projmod.Timer = 0;
                projmod.TimeMax = 240;
                projmod.isCombo = StabCombo;
                projmod.ComboCount = (projmod.ComboCount + (StabCombo ? 1 : 0)) % projmod.MaxComboCount;
                projmod.SetState<Recover>();
            }
        }
        /* State : Recover
         * Hold the sword for 1s, when the player could switch the state of projectile
         * If the sword does not switch state, apply dissolve and make projectile invisible
        */
        public class Recover : ProjectileState
        {
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                return;
            }
            public override void AI(ProjectileStateMachine projectile)
            {
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.ShouldDrawArm = true;
                projmod.WieldAttack = false;
                projmod.StabAttack = false;
                projmod.StabAttackStopDraw = false;
                projmod.ChargeShader = false;
                projmod.CouldHit = false;
                projmod.DamageScale = 1f;
                projmod.ShootProjDamScale = 1f;
                projmod.ApplyStuck = false;
                projmod.ApplySlashDust = false;
                projmod.ApplyScreenShake = false;
                projmod.AttackHit = false;
                projmod.SwordPowerGaugeAdd = 0;
                projmod.Timer++;
                #region Switch state
                if (projmod.Timer > projmod.TimeMax / 10 && projmod.Timer <= projmod.TimeMax * 2 / 5)
                {
                    projmod.LerpSwordStruct(proj, projmod.TargetStruct2, (projmod.Timer - (projmod.TimeMax / 10)) / (projmod.TimeMax * 3 / 10), true, true);
                }
                else if (projmod.Timer > projmod.TimeMax * 2 / 5)
                {
                    proj.rotation = player.direction < 0 ? 0.9f * (float)Math.PI : 0.1f * (float)Math.PI;
                    projmod.FlipSwordTexture = player.direction < 0;
                }
                // Apply dissolve
                if (projmod.Timer > projmod.TimeMax * 4 / 5)
                {
                    projmod.ApplyDissolve = true;
                    projmod.DissolveRate = 1 - (projmod.Timer - projmod.TimeMax * 4 / 5) / (projmod.TimeMax / 5);
                }
                if (projmod.Timer > projmod.TimeMax) // End current state
                {
                    projmod.ApplyDissolve = false;
                    projmod.Timer = 0;
                    projmod.SetState<Idle>();
                    return;
                }
                #endregion
            }
            public override void SwitchState(ProjectileStateMachine projectile)
            {
                throw new NotImplementedException();
            }
        }

        public override void AIAfter()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.Center = player.Center + new Vector2(-4 * player.direction, -2) + new Vector2(0, 12).RotatedBy(ArmRotation) + ArmToSwordOffset + new Vector2(SwordRadius, 0).RotatedBy(Projectile.rotation); //表现为当剑和手臂的距离为0时剑刚好拿在手上
            if (ShouldDrawArm) player.SetCompositeArmFront(true, 0, ArmRotation - player.fullRotation);
            if (isCombo)
                ComboTimer++;
            else ComboTimer = 0;
            if (ComboTimer >= 120) // After 0.5s of recovery, reset ComboCount so the player ends a combo
            {
                ComboTimer = 0;
                isCombo = false;
                ComboCount = 0;
            }
            //Main.NewText(ComboCount+ " " +ComboTimer);
        }
        // Adjust the projectile hitbox. As the sword projectile does not have an appropriate hitbox, we build one through functions
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) 
        {
            float point = 0f;
            if (CouldHit)
            {
                if (StabAttack) // Stab has its own collision box
                    return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center + new Vector2(-SwordRadius, 0).RotatedBy(Projectile.rotation), Projectile.Center + new Vector2(SwordRadius * MathHelper.Lerp(1f, 3f, (Timer - TimeMax / 2f) / (TimeMax / 4f)), 0).RotatedBy(Projectile.rotation), 8, ref point);
                else return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center + new Vector2(-SwordRadius, 0).RotatedBy(Projectile.rotation), Projectile.Center + new Vector2(SwordRadius, 0).RotatedBy(Projectile.rotation), 8, ref point);
            } // The third argument is head, the fourth is tail, the fifth is the width of collision box, fix other arguments
            else return false;
        }
        // If the projectile can hit, it can hit breakable tiles (like vases)
        public override bool? CanCutTiles()
        {
            return CouldHit;
        }
        #region HitEffectVariables
        public bool ApplyScreenShake; //If next attact apply screen shake
        public bool ApplyGreaterScreenShake; //If next attact apply greater screen shake
        public int SwordDebuff; // Debuff applied to enemies being hit
        public int SwordDebuffTime; // Time of debuff applied to enemies in frames
        public int SwordDebuffRate; // The rate of applying debuff
        public int SwordDust1; // Dust 1, driven from target being hit while being affected by gravity
        public int SwordDust2; // Dust 2, driven from target being hit
        public int SwordDust3; // Dust 3, driven from the sword projectile
        #endregion
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            #region HitStuck
            if (ApplyStuck) WieldStuckTimer = 8;
            #endregion
            #region Knockback & Screen Shake
            if (ApplyScreenShake && !MeleeRevampConfigClient.Instance.CameraLockDisable)
            {
                Vector2 dis = target.position - Projectile.Center;
                Main.LocalPlayer.GetModPlayer<ScreenShake>().ScreenShakeShort((int)((ApplyGreaterScreenShake) ? 36 : 18 * MeleeRevampConfigClient.Instance.ShakeIntensity), (float)Math.Atan(dis.Y / dis.X));
            }
            Projectile.velocity.X = player.direction == 1 ? 1 : -1; //Change projectile direction to adjust knockback direction
            #endregion
            #region Modification of Sword Gauge
            if (!AttackHit)
            {
                player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge += SwordPowerGaugeAdd; // Change sword gauge by SwordPowerGaugeAdd
                if (player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge > player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGaugeMax)
                    player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge = player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGaugeMax;
                if (player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge < 0)
                    player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge = 0;
                player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge = (float)Math.Round(player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge, 1);
            }
            #endregion
            #region Apply debuff
            if (SwordDebuff != 0) // Apply when there is debuff
            {
                if (Main.rand.NextBool(SwordDebuffRate))
                    target.AddBuff(SwordDebuff, SwordDebuffTime * 60);
            }
            #endregion
            #region Apply slash dust
            if (ApplySlashDust)
            {
                for (int i = 0; i < 3; i++)
                {
                    float Rot = Main.rand.NextFloat(-0.5f, 0.5f);
                    float Vel = Main.rand.NextFloat(1.5f, 3f);
                    Vector2 direction = target.DirectionFrom(player.Center);
                    Vector2 position = target.Center - direction * 10;
                    ParticleSystem.NewParticle(position, direction.RotatedBy(Rot) * Vel * 12, new SlashParticle(), Color.LightGoldenrodYellow, 0.8f);
                }

                //Rectangle Intersect = Rectangle.Intersect(Projectile.Hitbox, target.Hitbox);
                //Dust dust = Dust.NewDustPerfect(new Vector2(Intersect.Center.X - 36 * player.direction, Intersect.Center.Y - 18 - 18 * player.direction), ModContent.DustType<SwordSlash>(), Vector2.Zero, 0, Color.LightGoldenrodYellow, Main.rand.NextFloat(0.6f, 0.8f));
                //dust.rotation = player.direction == 1 ? 0.5f : (float)Math.PI - 0.5f;
            }
            //Dust.NewDustPerfect(Projectile.Bottom + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(15), ModContent.DustType<GlowDust>(), Vector2.Zero, 0, new Color(50, 50, 255), 0.4f).fadeIn = 10;
            #endregion
            #region Apply hit dust
            if (SwordDust1 != 0) //Dust1受重力影响
            {
                int num1 = Main.rand.Next(4, 7);
                for (int i = 0; i < num1; i++)
                {
                    Dust dust2 = Dust.NewDustDirect(target.position, 0, 0, SwordDust1);
                    if (player.position.X < target.position.X)
                        dust2.velocity = new Vector2(Main.rand.NextFloat(3.6f, 7.2f), Main.rand.NextFloat(-2.4f, 3.6f));
                    else dust2.velocity = new Vector2(Main.rand.NextFloat(-7.2f, -3.6f), Main.rand.NextFloat(-2.4f, 3.6f));
                    dust2.scale = Main.rand.NextFloat(0.8f, 1f);
                    dust2.noGravity = false;
                }
            }
            if (SwordDust2 != 0) //Dust2自由逸散
            {
                int num2 = Main.rand.Next(4, 7);
                for (int i = 0; i < num2; i++)
                {
                    Dust dust2 = Dust.NewDustDirect(target.position, 0, 0, SwordDust2);
                    dust2.velocity = new Vector2(Main.rand.NextFloat(-3.6f, 3.6f), Main.rand.NextFloat(-3.6f, 3.6f));
                    dust2.scale = Main.rand.NextFloat(0.8f, 1.2f);
                    dust2.noGravity = true;
                }
            }
            #endregion
            #region Set hit flag to true
            if (!AttackHit) AttackHit = true;
            #endregion
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            // If the sword item itself should be drawn
            if (DrawSword)
            {
                // If dissolve shader is applied
                if (ApplyDissolve)
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
                    Effect effect = Filters.Scene["Dissolve"].GetShader().Shader;
                    effect.Parameters["tex"].SetValue(TextureAssets.Extra[193].Value);
                    effect.Parameters["uImageSize1"].SetValue(new Vector2(256, 256));
                    effect.Parameters["uDissolveRate"].SetValue(DissolveRate);
                    effect.CurrentTechnique.Passes[0].Apply();
                }
                #region Draw the sword
                if (ChargeShader)
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
                    // Calculate the intensity of applying charge border as shader input
                    float alphacalc = 1;
                    if (Timer % 64 > 0 && Timer % 64 <= 16) alphacalc = 0.25f;
                    if ((Timer % 64 > 16 && Timer % 64 <= 32) || (Timer % 64 > 48 && Timer % 64 <= 63) || Timer % 64 == 0) alphacalc = 0.5f;
                    ChargeEffect.ChargeBorder.Parameters["uImageSize"].SetValue(SwordTexture.Size() * Projectile.scale);
                    ChargeEffect.ChargeBorder.Parameters["alp"].SetValue(alphacalc);
                    ChargeEffect.ChargeBorder.CurrentTechnique.Passes[0].Apply();
                }
                Rectangle rect = new Rectangle(0, 0, SwordTexture.Width, SwordTexture.Height);
                float anglefix = !FlipSwordTexture ? 0 : (float)(0.5 * Math.PI);
                SpriteEffects effects = !FlipSwordTexture ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                // Set up all variables for entityspritedraw while applying a 45 degrees rotation, as most sword item sprites has a 45 degree counter clockwise pose
                Main.EntitySpriteDraw(SwordTexture, Projectile.Center - Main.screenPosition, rect, lightColor,
                Projectile.rotation + anglefix + (float)Math.Atan((float)SwordTexture.Height / (float)SwordTexture.Width),
                new Vector2(SwordTexture.Width / 2, SwordTexture.Height / 2), Projectile.scale, effects, 0);
                #endregion
            }
            #region Draw the slash of wield state using vertex paint
            if (WieldAttack)
            {
                List<VertexInfo2> slash = new List<VertexInfo2>();
                int iTimer = SlashDrawTimer < 39 ? SlashDrawTimer + 1 : Projectile.oldPos.Length;
                for (int i = 0; i < iTimer; i++) // Track the cache to read the key variables for drawing slash using vertex paint
                {
                    Vector2 pos = Projectile.Center - new Vector2(SwordRadius, 0).RotatedBy(Projectile.rotation) - Main.screenPosition; 
                    slash.Add(new VertexInfo2(pos + new Vector2(WieldDrawRadius[SlashDrawTimer - i] * 2, 0).RotatedBy(Projectile.oldRot[i]), new Vector3(1 - (float)i / SlashDrawTimer, 0, 1), SlashColor * MathHelper.Lerp(1, 0, (float)i / iTimer)));
                    slash.Add(new VertexInfo2(pos + new Vector2(WieldDrawRadius[SlashDrawTimer - i] / 2, 0).RotatedBy(Projectile.oldRot[i]), new Vector3(1 - (float)i / SlashDrawTimer, 0.5f, 1), SlashColor * MathHelper.Lerp(1, 0, (float)i / iTimer)));
                }
                #region Set up vertex paint
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                if (slash.Count >= 3)
                {
                    Main.graphics.GraphicsDevice.Textures[0] = SlashTexture.Value;
                    Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, slash.ToArray(), 0, slash.Count - 2);
                }
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                #endregion
            }
            #endregion
            #region Draw the slash of Stab state using vertex paint
            if (StabAttack) 
            {
                List<VertexInfo2> slash = new List<VertexInfo2>();
                for (int i = 0; i < 5; i++)
                {
                    float width = 16;
                    switch (i)
                    {
                        case 0: width = 12; break;
                        case 1: width = 12 + 4 / 3; break;
                        case 2: width = 12 + 8 / 3; break;
                        case 3: width = 16; break;
                        case 4: width = 0; break;
                    }
                    width *= MathHelper.Lerp(1, 0.5f, (float)SlashDrawTimer / SlashDrawTimeMax);
                    Vector2 pos = Projectile.Center - Main.screenPosition - new Vector2(SwordRadius, 0).RotatedBy(Projectile.rotation);
                    slash.Add(new VertexInfo2(pos + new Vector2(SwordRadius * i * MathHelper.Lerp(0, 1, (float)SlashDrawTimer / SlashDrawTimeMax), -width).RotatedBy(Projectile.rotation), new Vector3(i / 4f, 0.9f, 1), SlashColor));
                    slash.Add(new VertexInfo2(pos + new Vector2(SwordRadius * i * MathHelper.Lerp(0, 1, (float)SlashDrawTimer / SlashDrawTimeMax), width).RotatedBy(Projectile.rotation), new Vector3(i / 4f, 1, 1), SlashColor));
                }
                #region Set up vertex paint
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                if (slash.Count >= 3)
                {
                    Main.graphics.GraphicsDevice.Textures[0] = SlashTexture.Value;
                    Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, slash.ToArray(), 0, slash.Count - 2);
                }
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                #endregion
            }
            if (StabAttackStopDraw)
            {
                List<VertexInfo2> slash = new List<VertexInfo2>();
                for (int i = 0; i < 5; i++)
                {
                    float width = 16;
                    switch (i)
                    {
                        case 0: width = 12; break;
                        case 1: width = 12 + 4 / 3; break;
                        case 2: width = 12 + 8 / 3; break;
                        case 3: width = 16; break;
                        case 4: width = 0; break;
                    }
                    width *= 0.5f;
                    Vector2 pos = Projectile.Center - Main.screenPosition - new Vector2(SwordRadius, 0).RotatedBy(Projectile.rotation);
                    slash.Add(new VertexInfo2(pos + new Vector2(SwordRadius * i * MathHelper.Lerp(1, 0, (float)SlashDrawTimer / SlashDrawTimeMax), -width).RotatedBy(Projectile.rotation), new Vector3(i / 4f, 0.9f, 1), SlashColor));
                    slash.Add(new VertexInfo2(pos + new Vector2(SwordRadius * i * MathHelper.Lerp(1, 0, (float)SlashDrawTimer / SlashDrawTimeMax), width).RotatedBy(Projectile.rotation), new Vector3(i / 4f, 1, 1), SlashColor));
                }
                #region Set up vertex paint
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                if (slash.Count >= 3)
                {
                    Main.graphics.GraphicsDevice.Textures[0] = SlashTexture.Value;
                    Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, slash.ToArray(), 0, slash.Count - 2);
                }
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                #endregion
            }
            #endregion
            return false;
        }
        public override void PostDraw(Color lightColor)
        {
            Main.spriteBatch.End();// Close the spriteBatch after draw
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            if (DrawBehindPlayer)
                Main.instance.DrawCacheProjsBehindProjectiles.Add(index);
            else Main.instance.DrawCacheProjsOverPlayers.Add(index);
        }
        public override void OnKill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGaugeMax = 0;
        }
        // Draw the noise map for applying warp effects onto the screen
        public void DrawWarp()
        {
            Player player = Main.player[Projectile.owner];
            if (WieldAttack) // While the state is wield state
            {
                List<VertexInfo2> slash = new List<VertexInfo2>();
                int iTimer = SlashDrawTimer < 39 ? SlashDrawTimer + 1 : Projectile.oldPos.Length;
                for (int i = 0; i < iTimer; i++) // Track the cache to read the key variables for drawing slash using vertex paint
                {
                    Vector2 pos = Projectile.Center - new Vector2(SwordRadius, 0).RotatedBy(Projectile.rotation) - Main.screenPosition; // The center of rot is always the projectile center
                    slash.Add(new VertexInfo2(pos + new Vector2(WieldDrawRadius[SlashDrawTimer - i] * 2, 0).RotatedBy(Projectile.oldRot[i]), new Vector3(1 - (float)i / SlashDrawTimer, 0, 1), SlashColor * MathHelper.Lerp(1, 0, (float)i / SlashDrawTimer)));
                    slash.Add(new VertexInfo2(pos + new Vector2(WieldDrawRadius[SlashDrawTimer - i] / 2, 0).RotatedBy(Projectile.oldRot[i]), new Vector3(1 - (float)i / SlashDrawTimer, 0.12f, 1), SlashColor * MathHelper.Lerp(1, 0, (float)i / SlashDrawTimer)));
                }
                #region Set up vertex paint
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                if (slash.Count >= 3)
                {
                    Main.graphics.GraphicsDevice.Textures[1] = WarpTexture.Value;
                    Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, slash.ToArray(), 0, slash.Count - 2);
                }
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                #endregion
            }
        }

    }

    //The sword drive of sword while in Stab state
    public class GlobalSwordDrive : ModProjectile
    {
        public Color color = Color.White;
        public override string Texture => "MeleeRevamp/Content/Assets/SlashTex";
        public override void SetDefaults()
        {
            Projectile.width = 128;
            Projectile.height = 16;
            Projectile.timeLeft = 15;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
        }
        public override void AI()
        {
            Projectile.rotation = (float)Math.Atan(Projectile.velocity.Y / Projectile.velocity.X);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            List<VertexInfo2> slash = new List<VertexInfo2>();
            for (int i = 0; i < 4; i++)
            {
                int width = 8;
                switch (i)
                {
                    case 0: width = 6; break;
                    case 1: width = 7; break;
                    case 2: width = 8; break;
                    case 3: width = 0; break;
                }
                Vector2 pos = Projectile.Center - Main.screenPosition;
                slash.Add(new VertexInfo2(pos + new Vector2(-Projectile.width + Projectile.width / 2f * i, -width).RotatedBy(Projectile.rotation), new Vector3(i / 3f, 0.9f, 1), color));
                slash.Add(new VertexInfo2(pos + new Vector2(-Projectile.width + Projectile.width / 2f * i, width).RotatedBy(Projectile.rotation), new Vector3(i / 3f, 1, 1), color));
            }
            #region Set up vertex paint
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            if (slash.Count >= 3)
            {
                Main.graphics.GraphicsDevice.Textures[0] = ModContent.Request<Texture2D>(Texture).Value;
                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, slash.ToArray(), 0, slash.Count - 2);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            #endregion
            return false;
        }
    }
    public class GlowDustSword : ModDust
    {
        public override string Texture => "MeleeRevamp/Content/Assets/GlowDust";
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.frame = new Rectangle(0, 0, 64, 64);
            dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(MeleeRevamp.Instance.Assets.Request<Effect>("Effects/GlowDust", AssetRequestMode.ImmediateLoad).Value), "GlowingDust");
        }
        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }
        public override bool Update(Dust dust)
        {
            dust.scale *= 0.95f;
            dust.position += dust.velocity;
            dust.shader.UseColor(dust.color);
            if (!dust.noGravity)
                dust.velocity.Y += 0.1f;

            dust.velocity *= 0.99f;
            dust.color *= 0.95f;

            if (!dust.noLight)
                Lighting.AddLight(dust.position, dust.color.ToVector3());

            if (dust.scale < 0.05f)
                dust.active = false;

            return false;
        }
    }
}