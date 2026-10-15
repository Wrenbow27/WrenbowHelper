using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Celeste.Mod.WrenbowHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.WrenbowHelper.Triggers;

[CustomEntity("WrenbowHelper/FlagIfLineOfSight")]
[Tracked]
public class FlagIfLineOfSight : Entity 
{
    //Lonn Initialized
    private readonly string entityID1;
    private readonly string entityID2;
    private readonly string flag;
    private readonly string flagIf;
    private readonly float maxRange;
    private readonly bool solids;
    private readonly bool lightBlockers;

    private bool initialized = false;
    private bool active = false;
    private static Dictionary<int, Entity> queryEntities = new();
    private int?[] keys = [null, null];

    private Entity entity1;
    private Entity entity2;
    private bool cheapCheck = true;

    public FlagIfLineOfSight(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        entityID1 = data.String("entity1");
        entityID2 = data.String("entity2");
        flag = data.String("flag");
        flagIf = data.String("flagIf");
        maxRange = data.Float("maxRange");
        solids = data.Bool("solids");
        lightBlockers = data.Bool("lightBlockers");
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        if (!initialized)
        {
            queryEntities.Clear();
            List<int> ids = [];
            foreach (FlagIfLineOfSight eye in scene.Tracker.GetEntities<FlagIfLineOfSight>())
            {
                int? key1 = ParseID(eye.entityID1);
                int? key2 = ParseID(eye.entityID2);

                if (key1 is not null && key2 is not null)
                {
                    eye.keys = [key1, key2];
                    ids.Add((int)key1);
                    ids.Add((int)key2);
                    eye.active = true;
                } else {
                    Logger.Log(LogLevel.Error, "WrenbowHelper", "Entity IDs in FlagIfLineOfSight [" + eye.SourceId.ID + "] not able to be parsed.");
                }
                eye.initialized = true;
            }

            foreach (Entity entity in scene.Entities)
            {
                int id = entity.SourceId.ID;
                if (entity is not Player &&
                    entity is not FlagIfLineOfSight &&
                    ids.Contains(id) &&
                    !queryEntities.ContainsKey(id))
                {
                    queryEntities[id] = entity;
                }
            }
        }

        if (active)
        {
            int key1 = (int)keys[0];
            switch (key1)
            {
                case (-2):
                    entity1 = this;
                    break;
                case (-1):
                    entity1 = scene.Tracker.GetEntity<Player>();
                    break;
                default:
                    entity1 = queryEntities[key1];
                    if (entity1 is Solid)
                    {
                        cheapCheck = false;
                    }
                    break;
            }

            int key2 = (int)keys[1];
            switch (key2)
            {
                case (-2):
                    entity2 = this;
                    break;
                case (-1):
                    entity2 = scene.Tracker.GetEntity<Player>();
                    break;
                default:
                    entity2 = queryEntities[key2];
                    if (entity2 is Solid)
                    {
                        cheapCheck = false;
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
        {
            Vector2 pos1 = entity1.Center;
            Vector2 pos2 = entity2.Center;
            if ((pos1 - pos2).Length() <= maxRange)
            {
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
            } else {
                SceneAs<Level>().Session.SetFlag(flag, false);
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
                if (solid != entity1 && solid != entity2)
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
                occluder.Entity == entity1 ||
                occluder.Entity == entity2 ||
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
}