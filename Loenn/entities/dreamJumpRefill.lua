local dreamJumpRefill = {
    name = "WrenbowHelper/DreamJumpRefill",

    placements = {
    {
        name = "dream_jump_refill",
        data = {
            oneUse = false,
            earlyLeniencyFrames = 0,
            lateLeniencyFrames = 0,
            persistent = true,
            trailIndicator = true,
            trailColor = "c81ec8"
            }
        }
    },
    fieldInformation = {
        earlyLeniencyFrames = {
            fieldType = "integer"
        },
        lateLeniencyFrames = {
            fieldType = "integer"
        },
        trailColor = {
            fieldType = "color",
            allowXNAColors = true,
            useAlpha = false
        },
    },
    texture = function()
        return "objects/WrenbowHelper/DreamJumpRefill/idle00"
    end,
    depth = -100
}

return dreamJumpRefill