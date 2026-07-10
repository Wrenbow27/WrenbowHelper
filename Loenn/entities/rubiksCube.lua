local displayTypes = {"NetL"}

local rubiksCube = {
    name = "WrenbowHelper/RubiksCube",

    placements = {
    {
        name = "rubiks_cube",
        data = {
            size = 3,
            startScrambled = true,
            scrambleDepth = 30,
            solvedFlag = "",
            displayType = "NetL"
            }
        }
    },
    fieldInformation = {
        size = {
            fieldType = "integer",
            minimumValue = 2,
            maximumValue = 5
        },
        scrambleDepth = {
            fieldType = "integer",
            minimumValue = 0
        },
        solvedFlag = {
            fieldType = "string"
        },
        displayType = {
            options = displayTypes
        }
    },
}

return rubiksCube