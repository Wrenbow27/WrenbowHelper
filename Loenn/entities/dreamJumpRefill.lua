local dreamJumpRefill = {
    name = "WrenbowHelper/DreamJumpRefill",

    placements = {
    {
        name = "dream_jump_refill",
        data = {
            oneUse = false,
            persistent = true
            }
        }
    },
    texture = function()
        return "objects/WrenbowHelper/DreamJumpRefill/idle00"
    end,
    depth = -100
}

return dreamJumpRefill