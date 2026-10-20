using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly string entityFromID;
    private readonly string[] entityToIDs;
    private readonly string flag;
    private readonly string flagIf;
    private readonly float maxRangeSq;
    private readonly bool solids;
    private readonly bool lightBlockers;
    private readonly bool drawSprite;

    private bool initialized = false;
    private bool active = false;
    private readonly bool hasFlag = false;
    private static Dictionary<int, Entity> queryEntities = new();
    private int keyFrom;
    private int[] keysTo;
    private Entity entityFrom;
    private Entity[] entitiesTo;
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
        entityFromID = data.String("entityFrom", "self");
        entityToIDs = data.Attr("entityTo", "player").Split(',');
        flag = data.String("flag", "");
        flagIf = data.String("flagIf", "");
        maxRangeSq = data.Float("maxRange", 60f);
        maxRangeSq *= maxRangeSq;
        solids = data.Bool("solids", true);
        lightBlockers = data.Bool("lightBlockers", true);
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
                int? tryKeyFrom = ParseID(eye.entityFromID);
                int?[] tryKeysTo = ParseIDs(eye.entityToIDs);

                if (tryKeyFrom is not null && !tryKeysTo.Contains(null))
                {
                    eye.keyFrom = (int)tryKeyFrom;
                    eye.keysTo = tryKeysTo.Where(x => x.HasValue).Select(x => x.Value).ToArray();
                    ids.Add(eye.keyFrom);
                    ids.AddRange(eye.keysTo);
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
            entityFrom = QueryEntity(scene, keyFrom);

            entitiesTo = new Entity[keysTo.Length];

            for (int i = 0; i < keysTo.Length; i++)
            {
                entitiesTo[i] = QueryEntity(scene, keysTo[i]);
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

    private int?[] ParseIDs(string[] ids)
    {
        int?[] parsedIDs = new int?[ids.Length];
        int i = 0;
        foreach (string id in ids)
        {
            parsedIDs[i] = ParseID(id);
            i++;
        }
        return parsedIDs;
    }

    private Entity QueryEntity(Scene scene, int id)
    {
        Entity entity = null;
        switch (id)
        {
            case (-2):
                entity = this;
                break;
            case (-1):
                entity = scene.Tracker.GetEntity<Player>();
                break;
            default:
                if (queryEntities.ContainsKey(id)){
                    entity = queryEntities[id];
                    if (entity is Solid)
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
        return entity;
    }

    public override void Update()
    {
        base.Update();

        if (active)
        { // has valid settings
            if (!(hasFlag && !SceneAs<Level>().Session.GetFlag(flagIf)))
            { // not being disabled via flag
                Vector2 pos1 = entityFrom.Center;
                float? minDistSq = null;
                Entity closest = null;
                bool anyInRange = false;
                foreach (Entity entityTo in entitiesTo)
                {
                    Vector2 pos2 = entityTo.Center;
                    float distanceSq = (pos1 - pos2).LengthSquared();
                    if ( distanceSq <= maxRangeSq)
                    { // entity in range
                        anyInRange = true;
                        if (solids && CollideCheckSolids(pos1, pos2))
                        {
                            continue;
                        }
                        if (lightBlockers && CollideCheckOccluders(pos1, pos2))
                        {
                            continue;
                        }
                        
                        // line of sight found
                        if (minDistSq is null || (float)minDistSq > distanceSq)
                        {
                            minDistSq = distanceSq;
                            closest = entityTo;
                        }
                    }
                }

                if (closest is not null) // has any non null or non zero values
                {
                    SceneAs<Level>().Session.SetFlag(flag, true);
                    MovePupil(closest.Center - entityFrom.Center);
                    OpenEye();
                }
                else
                {
                    SceneAs<Level>().Session.SetFlag(flag, false);
                    MovePupil(Vector2.Zero);
                    if (anyInRange)
                    {
                        OpenEye();
                    }
                    else
                    {
                        CloseEye();
                    }
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
                if (solid != entityFrom && !entitiesTo.Contains(solid))
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
                entitiesTo.Contains(occluder.Entity) ||
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