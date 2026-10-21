local drawable_sprite = require("structs.drawable_sprite")

local lineOfSightEye = {
    name = "WrenbowHelper/LineOfSightEye",
    placements = {
        {
            name = "line_of_sight_eye",
            data = {
                entityFrom = "self",
                entityTo = "player",
                flag = "",
                flagIf = "",
                maxRange = 60,
                solids = true,
                lightBlockers = true,
                drawSprite = true,
            }
        },
    },
    fieldInformation = {
        entityTo = {
            fieldType = "list",
            minimumElements = 0,
            elementDefault = "",
            elementOptions = {
                fieldType = "string"
            }
        }
    },
    fieldOrder = {
        "x",
        "y",
        "entityFrom",
        "entityTo",
        "flag",
        "flagIf",
        "maxRange",
        "solids",
        "lightBlockers",
        "drawSprite" 
    },
}

local function isBackground(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0

    local tx = math.floor(x / 8) + 1
    local ty = math.floor(y / 8) + 1

    return room.tilesFg.matrix:get(tx, ty, "0") == "0"
end

function lineOfSightEye.depth(room, entity)
    return isBackground(room, entity) and 8990 or -10001
end

function lineOfSightEye.sprite(room, entity)
    local layer = isBackground(room, entity) and "bg" or "fg"

    local eye = drawable_sprite.fromTexture("scenery/temple/eye/" .. layer .. "_eye", entity)
    eye:setJustification(0.5, 0.5)
    local lid = drawable_sprite.fromTexture("scenery/temple/eye/" .. layer .. "_lid00", entity)
    lid:setJustification(0.5, 0.5)
    local pupil = drawable_sprite.fromTexture("scenery/temple/eye/" .. layer .. "_pupil", entity)
    pupil:setJustification(0.5, 0.5)
    local sprites = {eye, pupil, lid}

    return sprites
end

function lineOfSightEye.drawSelected(room, layer, entity)
    love.graphics.setColor(1, 1, 1, 0.5)
    love.graphics.circle("line", entity.x, entity.y, entity.maxRange or 60, 64)
    love.graphics.setColor(1, 1, 1, 1)
    return
end

return lineOfSightEye