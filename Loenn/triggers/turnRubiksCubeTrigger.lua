local faces = {"Down","Up","Left","Right","Front","Back"}
local directions = {"Clockwise", "Widdershins"}
local operations = {"Select Face", "Select Layer", "Select Direction", "Turn Selection", "All"}

local turnRubiksCubeTrigger = {
    name = "WrenbowHelper/TurnRubiksCubeTrigger",
    placements = {
        {
            name = "turn_rubiks_cube_trigger",
            data = {
                face = "Down",
                layer = 0,
                direction = "Clockwise",
                cubeID = "",
                operation = "All"
            }
        },
    },
    fieldInformation = {
        face = {
            options = faces
        },
        turnLayer = {
            fieldType = "integer",
            minimumValue = 0
        },
        direction = {
            options = directions
        },
        operation = {
            options = operations
        }
    },
}

return turnRubiksCubeTrigger