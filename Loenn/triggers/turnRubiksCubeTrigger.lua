local faces = {"Down","Up","Left","Right","Front","Back"}
local directions = {"Clockwise", "Widdershins"}

local turnRubiksCubeTrigger = {
    name = "WrenbowHelper/TurnRubiksCubeTrigger",
    placements = {
        {
            name = "turn_rubiks_cube_trigger",
            data = {
                face = "Down",
                turnDepth = 0;
                direction = "Clockwise"
            }
        },
    },
    fieldInformation = {
        face = {
            options = faces
        },
        turnDepth = {
            fieldType = "integer",
            minimumValue = 0
        },
        direction = {
            options = directions
        }
    },
}

return turnRubiksCubeTrigger