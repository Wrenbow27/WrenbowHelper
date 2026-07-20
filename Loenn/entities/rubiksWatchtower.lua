local drawableSprite = require("structs.drawable_sprite")
local drawableRect = require("structs.drawable_rectangle")

local rubiksWatchtower = {
    name = "WrenbowHelper/RubiksWatchtower",

    placements = {
        {
            name = "rubiks_watchtower",
            data = {
                cubeName = "Cube0"
            }
        }
    },
    fieldInformation = {
        cubeName = {
            fieldType = "string"
        }
    },
    nodeLimits = {1, 1},
    texture = "objects/lookout/lookout05",
    justification = {0.5, 1.0}
}

return rubiksWatchtower