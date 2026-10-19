using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Celeste.Mod.WrenbowHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.WrenbowHelper.Triggers;

[CustomEntity("WrenbowHelper/LineOfSightEye")]
[Tracked]
public class LineOfSightEye : Entity 
{
    //Lonn Initialized
    private readonly string entityID1;
    private readonly string entityID2;
    private readonly string flag;
    private readonly string flagIf;
    private readonly float maxRange;
    private readonly bool solids;
    private readonly bool lightBlockers;
    private readonly bool drawSprite;

    private bool initialized = false;
    private bool active = false;
    private readonly bool hasFlag = false;
    private static Dictionary<int, Entity> queryEntities = new();
    private int?[] keys = [null, null];
    private Entity entityFrom;
    private Entity entityTo;
    private bool cheapCheck = true;

    
    private MTexture eyeTexture;
	private MTexture pupilTexture;
	private Sprite eyelid;
	private Vector2 pupilPosition;
	private Vector2 pupilTarget;
    private bool wasOpen;
    private bool isBG;

    public LineOfSightEye(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        entityID1 = data.String("entityFrom");
        entityID2 = data.String("entityTo");
        flag = data.String("flag");
        flagIf = data.String("flagIf");
        maxRange = data.Float("maxRange");
        solids = data.Bool("solids");
        lightBlockers = data.Bool("lightBlockers");
        drawSprite = data.Bool("drawSprite", true);

        if (!string.IsNullOrEmpty(flagIf))
        {
            hasFlag = true;
        }
    }

    public override void Added(Scene scene)
	{
        base.Added(scene);
        
        if (!drawSprite)
        {
            return;
        }

		isBG = !scene.CollideCheck<Solid>(Position);
		if (isBG)
		{
			eyeTexture = GFX.Game["scenery/temple/eye/bg_eye"];
			pupilTexture = GFX.Game["scenery/temple/eye/bg_pupil"];
			Add(eyelid = new Sprite(GFX.Game, "scenery/temple/eye/bg_lid"));
			base.Depth = 8990;
		}
		else
		{
			eyeTexture = GFX.Game["scenery/temple/eye/fg_eye"];
			pupilTexture = GFX.Game["scenery/temple/eye/fg_pupil"];
			Add(eyelid = new Sprite(GFX.Game, "scenery/temple/eye/fg_lid"));
			base.Depth = -10001;
		}
        eyelid.AddLoop("open", "", 0f, 0);
        eyelid.AddLoop("closed", "", 0f, 1);
        eyelid.Add("opening", "", 0.08f, "open", 1, 2, 3, 0);
        eyelid.Add("closing", "", 0.08f, "closed", 0, 3, 2, 1);
		eyelid.Play("closed");
        eyelid.CenterOrigin();
        wasOpen = false;
	}

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        if (!initialized) // run once when a room with this entity is loaded
        {
            queryEntities.Clear();
            List<int> ids = [];
            foreach (LineOfSightEye eye in scene.Tracker.GetEntities<LineOfSightEye>())
            {
                int? key1 = ParseID(eye.entityID1);
                int? key2 = ParseID(eye.entityID2);

                if (key1 is not null && key2 is not null)
                {
                    eye.keys = [key1, key2];
                    ids.Add((int)key1);
                    ids.Add((int)key2);
                    eye.active = true;
                }
                else
                {
                    Logger.Log(LogLevel.Error, "WrenbowHelper", "Entity IDs in LineOfSightEye [" + eye.SourceId.ID + "] not able to be parsed.");
                }
                eye.initialized = true;
            }

            foreach (Entity entity in scene.Entities)
            {
                int id = entity.SourceId.ID;
                if (entity is not Player &&
                    ids.Contains(id) &&
                    !queryEntities.ContainsKey(id))
                {
                    queryEntities[id] = entity;
                }
            }
        }

        if (active) // run by every entity with valid ids set
        {
            int key1 = (int)keys[0];
            switch (key1)
            {
                case (-2):
                    entityFrom = this;
                    break;
                case (-1):
                    entityFrom = scene.Tracker.GetEntity<Player>();
                    break;
                default:
                    if (queryEntities.ContainsKey(key1)){
                        entityFrom = queryEntities[key1];
                        if (entityFrom is Solid)
                        {
                            cheapCheck = false;
                        }
                    }
                    else
                    {
                        active = false;
                    }
                    break;
            }

            int key2 = (int)keys[1];
            switch (key2)
            {
                case (-2):
                    entityTo = this;
                    break;
                case (-1):
                    entityTo = scene.Tracker.GetEntity<Player>();
                    break;
                default:
                    if (queryEntities.ContainsKey(key2))
                    {
                        entityTo = queryEntities[key2];
                        if (entityTo is Solid)
                        {
                            cheapCheck = false;
                        }
                    }
                    else
                    {
                        active = false;
                    }
                    break;
            }
        }
    }

    private int? ParseID(string id)
    {
        if (id == "player")
        {
            return -1;
        }
        if (id == "self")
        {
            return -2;
        }
        if (int.TryParse(id, out int result))
        {
            return result;
        }
        return null;
    }

    public override void Update()
    {
        base.Update();

        if (active)
        { // has valid settings
            if (!(hasFlag && !SceneAs<Level>().Session.GetFlag(flagIf)))
            { // not being disabled via flag
                Vector2 pos1 = entityFrom.Center;
                Vector2 pos2 = entityTo.Center;
                if ((pos1 - pos2).Length() <= maxRange)
                { // entity in range
                    bool canSee = true;
                    if (solids && CollideCheckSolids(pos1, pos2))
                    {
                        canSee = false;
                    }
                    if (canSee && lightBlockers && CollideCheckOccluders(pos1, pos2))
                    {
                        canSee = false;
                    }

                    SceneAs<Level>().Session.SetFlag(flag, canSee);

                    OpenEye();
                    if (canSee)
                    {
                        MovePupil(entityTo.Center - entityFrom.Center);
                    }
                    else
                    {
                        MovePupil(Vector2.Zero);
                    }
                }
                else
                { // entity not in range
                    SceneAs<Level>().Session.SetFlag(flag, false);
                    MovePupil(Vector2.Zero);
                    CloseEye();
                }
            }
            else
            { // being disabled via flag
                SceneAs<Level>().Session.SetFlag(flag, false);
                MovePupil(Vector2.Zero);
                CloseEye();
            }
        }
    }

    private bool CollideCheckSolids(Vector2 from, Vector2 to)
    {
        if (cheapCheck)
        {
            return Scene.CollideCheck<Solid>(from, to);
        } else {
            foreach (Solid solid in Scene.CollideAll<Solid>(from, to))
            {
                if (solid != entityFrom && solid != entityTo)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool CollideCheckOccluders(Vector2 from, Vector2 to)
    {
        foreach (LightOcclude occluder in Scene.Tracker.GetComponents<LightOcclude>())
        {
            if (!occluder.Visible ||
                occluder.Entity is null ||
                !occluder.Entity.Visible ||
                occluder.Entity == entityFrom ||
                occluder.Entity == entityTo ||
                occluder.Alpha < 0.5f)
            {
                continue;
            }
            if (Collide.RectToLine(occluder.Left, occluder.Top, occluder.Width, occluder.Height, from, to))
            {
                return true;
            }
        }
        return false;
    }

    public override void Render()
    {
        if (drawSprite)
        {
            eyeTexture.DrawCentered(Position);
            pupilTexture.DrawCentered(Position + pupilPosition);
        }
        base.Render();
    }

    private void OpenEye()
    {
        if (drawSprite && !wasOpen)
        {
            wasOpen = true;
            eyelid.Play("opening");
            pupilPosition = Vector2.Zero;
        }
    }

    private void CloseEye()
    {
        if (drawSprite && wasOpen)
        {
            wasOpen = false;
            eyelid.Play("closing");
            pupilTarget = Vector2.Zero;
        }
    }

    private void MovePupil(Vector2 dir)
    {
        if (drawSprite)
        {
            pupilTarget = dir.SafeNormalize();
            pupilPosition = Calc.Approach(pupilPosition, pupilTarget * 3f, Engine.DeltaTime * 16f);
        }
    }
}